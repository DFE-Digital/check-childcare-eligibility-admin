namespace CheckChildcareEligibility.Admin.Boundary.Requests
{
    public class FosterFamiliesSearchRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string NINOFilter { get; set; } = string.Empty;

        public FosterFamiliesSearchRequest(int pageNumber, int pageSize, string ninoFilter = "")
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            NINOFilter = ninoFilter;
        }
    }
}
