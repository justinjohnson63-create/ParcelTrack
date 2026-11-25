namespace ParcelTrack.Services
{
    // Rule: J$500 base + J$150 per kg (weight rounded up) + J$300 if branch is outside Kingston / St Andrew.
    public class DeliveryFeeCalculator : IDeliveryFeeCalculator
    {
        public decimal Calculate(double weightKg, string parish)
        {
            decimal fee = 500m;
            int kgRoundedUp = (int)Math.Ceiling(weightKg);
            fee += kgRoundedUp * 150m;

            string p = parish?.Trim() ?? "";
            if (!p.Equals("Kingston", StringComparison.OrdinalIgnoreCase) &&
                !p.Equals("St Andrew", StringComparison.OrdinalIgnoreCase))
            {
                fee += 300m;
            }

            return fee;
        }
    }
}
