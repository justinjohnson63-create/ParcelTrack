using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ParcelTrack.Validation
{
    // Part E: tracking code must be PT- followed by exactly 6 digits.
    // Empty values pass so [Required] can handle them.
    public class TrackingCodeAttribute : ValidationAttribute
    {
        private static readonly Regex Pattern = new Regex(@"^PT-\d{6}$", RegexOptions.Compiled);

        public TrackingCodeAttribute()
        {
            ErrorMessage = "Tracking code must look like PT-123456";
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }

            string? text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            return Pattern.IsMatch(text);
        }
    }
}
