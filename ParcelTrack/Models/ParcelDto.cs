namespace ParcelTrack.Models
{
    // What the API sends back for one parcel: plain data, no navigation properties.
    // Sending the Parcel itself would loop: Parcel.Branch.Parcels points back at the parcel.
    public class ParcelDto
    {
        public int Id { get; set; }
        public string TrackingCode { get; set; } = "";
        public string Recipient { get; set; } = "";
        public double WeightKg { get; set; }
        public string Status { get; set; } = "";
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal DeliveryFee { get; set; }
    }
}
