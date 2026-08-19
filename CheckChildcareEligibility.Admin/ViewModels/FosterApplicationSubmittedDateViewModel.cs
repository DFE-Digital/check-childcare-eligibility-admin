using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class FosterApplicationSubmittedDateViewModel
    {
        [Required(ErrorMessage = "Select whether to use today's date or another date")]
        public bool? IsTodaySelected { get; set; }

        [NotMapped]
        [SubmissionDate(
            nameof(IsTodaySelected),
            nameof(Day),
            nameof(Month),
            nameof(Year))]
        public DateTime SubmissionDate { get; set; }

        public string? Day { get; set; }
        public string? Month { get; set; }
        public string? Year { get; set; }
    }
}