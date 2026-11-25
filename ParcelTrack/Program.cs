using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ParcelTrack.Data;
using ParcelTrack.Middleware;
using ParcelTrack.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core: any controller that asks for an AppDbContext gets one,
// connected to the database named in appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ParcelTrackDb")));

// Identity: user accounts and roles, stored through AppDbContext.
// RequireConfirmedAccount = false: this app can't send email.
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// A policy: "CanManageParcels" = Admin or Staff.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageParcels", policy => policy.RequireRole("Admin", "Staff"));
});

// Part A: JWT Bearer authentication (issuer, audience, expiry, signing key).
string jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// API calls must get 401 / 403, never a 302 to the login page.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// Part D: delivery fee calculator (Scoped: one instance per request).
builder.Services.AddScoped<IDeliveryFeeCalculator, DeliveryFeeCalculator>();

var app = builder.Build();

// Start-up seed: the roles and the two demo accounts. A scope, because there is no request yet.
using (IServiceScope scope = app.Services.CreateScope())
{
    await RoleSeed.Run(scope.ServiceProvider, app.Configuration);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Part B: request id + console log. After static files (so CSS/images are not logged),
// before routing and authentication (so 401/403 are still logged).
app.UseMiddleware<RequestLogMiddleware>();

app.UseRouting();

// Who are you? first, then: are you allowed? The order matters.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// The Register, Log in and Log out pages come with the Identity.UI package.
app.MapRazorPages();

app.Run();
