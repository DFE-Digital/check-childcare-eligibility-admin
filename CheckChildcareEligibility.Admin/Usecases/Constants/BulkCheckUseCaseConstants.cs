namespace CheckChildcareEligibility.Admin.Usecases.Constants
{
    public static class BulkCheckUseCaseConstants
    {
        public const string InvalidHeadersErrorMessage = "The column headings in the selected file must exactly match the template";
        public static string TooManyRowsErrorMessage(int rowCountLimit) { return $"The selected file must contain fewer than {rowCountLimit} rows"; } 
    }
}
