using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelTrack.Data;
using ParcelTrack.Filters;
using ParcelTrack.Models;
using ParcelTrack.Services;

namespace ParcelTrack.Controllers.Api
{
    // The same parcels as ParcelsController, sent as JSON instead of HTML pages.
    // [ApiController]: bad input gets an automatic 400, and an object parameter is read from the body.
    // ControllerBase, not Controller: an API has no views.
    [ApiController]
    [Route("api/parcels")]
    public class ParcelsApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IDeliveryFeeCalculator _feeCalculator;

        public ParcelsApiController(AppDbContext db, IDeliveryFeeCalculator feeCalculator)
        {
            _db = db;
            _feeCalculator = feeCalculator;
        }

        // Turns one Parcel (a database row) into one ParcelDto (what the API sends)
        private ParcelDto ToDto(Parcel parcel)
        {
            ParcelDto dto = new ParcelDto();
            dto.Id = parcel.Id;
            dto.TrackingCode = parcel.TrackingCode;
            dto.Recipient = parcel.Recipient;
            dto.WeightKg = parcel.WeightKg;
            dto.Status = parcel.Status;
            dto.BranchId = parcel.BranchId;
            if (parcel.Branch != null)
            {
                dto.BranchName = parcel.Branch.Name;
                dto.DeliveryFee = _feeCalculator.Calculate(parcel.WeightKg, parcel.Branch.Parish);
            }
            return dto;
        }

        // GET /api/parcels: every parcel, sorted by tracking code. 200 and a JSON list.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Parcel> parcels = _db.Parcels
                .Include(parcel => parcel.Branch)
                .OrderBy(parcel => parcel.TrackingCode)
                .ToList();

            List<ParcelDto> result = new List<ParcelDto>();
            foreach (Parcel parcel in parcels)
            {
                result.Add(ToDto(parcel));
            }
            return Ok(result);
        }

        // GET /api/parcels/3: one parcel. 200, or 404 if there is no such Id.
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public IActionResult GetOne(int id)
        {
            Parcel? parcel = _db.Parcels
                .Include(each_parcel => each_parcel.Branch)
                .FirstOrDefault(each_parcel => each_parcel.Id == id);
            if (parcel == null)
            {
                return NotFound();
            }
            return Ok(ToDto(parcel));
        }

        // POST /api/parcels with a JSON body: add a parcel.
        // 201 Created + a Location header, 400 if a rule fails, 409 if the tracking code is taken.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "CanManageParcels")]
        [RequireClient]
        [HttpPost]
        public IActionResult Create(ParcelInput input)
        {
            Parcel parcel = new Parcel();
            parcel.TrackingCode = input.TrackingCode;
            parcel.Recipient = input.Recipient;
            parcel.WeightKg = input.WeightKg;
            parcel.Status = input.Status;
            parcel.BranchId = input.BranchId;

            _db.Parcels.Add(parcel);
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // The unique index said no (or the branchId doesn't exist)
                return Conflict(new { message = "Another parcel already has that tracking code, or there is no such branch" });
            }

            // Fetch the branch, so the reply can include its name
            _db.Entry(parcel).Reference(saved_parcel => saved_parcel.Branch).Load();

            return CreatedAtAction(nameof(GetOne), new { id = parcel.Id }, ToDto(parcel));
        }

        // PUT /api/parcels/2 with a JSON body: replace parcel 2's details.
        // 204 No Content, 404 if there is no parcel 2, 400 if a rule fails, 409 if the code is another parcel's.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "CanManageParcels")]
        [RequireClient]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, ParcelInput input)
        {
            Parcel? parcel = _db.Parcels.Find(id);
            if (parcel == null)
            {
                return NotFound();
            }
            parcel.TrackingCode = input.TrackingCode;
            parcel.Recipient = input.Recipient;
            parcel.WeightKg = input.WeightKg;
            parcel.Status = input.Status;
            parcel.BranchId = input.BranchId;

            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Another parcel already has that tracking code, or there is no such branch" });
            }
            return NoContent();
        }

        // DELETE /api/parcels/5: 204 No Content, or 404 if it is already gone.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [RequireClient]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            Parcel? parcel = _db.Parcels.Find(id);
            if (parcel == null)
            {
                return NotFound();
            }
            _db.Parcels.Remove(parcel);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
