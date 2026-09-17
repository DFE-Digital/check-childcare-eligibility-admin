namespace CheckChildcareEligibility.Admin.Boundary.Requests
{
    public class FosterFamiliesSearchRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public FosterFamiliesSearchRequest(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
