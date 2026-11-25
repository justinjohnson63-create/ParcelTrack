namespace ParcelTrack.Services
{
    // Part D: delivery fee in one place so the page and the API both use the same rule.
    public interface IDeliveryFeeCalculator
    {
        decimal Calculate(double weightKg, string parish);
    }
}
