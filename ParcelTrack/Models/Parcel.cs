using System.ComponentModel.DataAnnotations;
using ParcelTrack.Validation;

namespace ParcelTrack.Models
{
    // One parcel on its way to a branch.
    public class Parcel
    {
        public int Id { get; set; }

        // The code printed on the label, e.g. PT-100101. No two parcels share one (a unique index, see AppDbContext).
        [Display(Name = "Tracking code")]
        [Required(ErrorMessage = "Give the tracking code, e.g. PT-100101")]
        [StringLength(9, ErrorMessage = "The tracking code is at most 9 characters")]
        [TrackingCode]
        public string TrackingCode { get; set; } = "";

        [Display(Name = "Recipient")]
        [Required(ErrorMessage = "Give the name of the person it is for")]
        [StringLength(40, ErrorMessage = "The name is at most 40 characters")]
        public string Recipient { get; set; } = "";

        [Display(Name = "Weight (kg)")]
        [Range(0.1, 30.0, ErrorMessage = "Weight must be from 0.1 kg to 30 kg")]
        public double WeightKg { get; set; }

        // Received, In transit, Ready for pickup or Delivered
        [Display(Name = "Status")]
        [Required(ErrorMessage = "Pick a status")]
        [StringLength(20)]
        public string Status { get; set; } = "Received";

        // Foreign key: the branch the parcel goes to
        [Display(Name = "Branch")]
        [Required(ErrorMessage = "Pick a branch")]
        public int? BranchId { get; set; }

        // Navigation property: the Branch itself. null until Include fills it.
        public Branch? Branch { get; set; }
    }
}
