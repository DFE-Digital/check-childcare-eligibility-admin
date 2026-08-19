using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.ViewModels
{
    public class SearchFosterFamiliesRecordsViewModel
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalNumberOfRecords { get; set; }

        public IEnumerable<FosterFamiliesSearchItemResponse> Data { get; set; }
            = [];
    }

    public class SearchFosterFamiliesRecords2ViewModel
    {
        public SearchFosterFamiliesRecords2ViewModel()
        {
            Data = new List<SearchFosterFamiliesRecordsViewModel>();
        }

        public List<SearchFosterFamiliesRecordsViewModel> Data { get; set; }

    }
}
