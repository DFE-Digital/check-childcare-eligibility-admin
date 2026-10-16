namespace CheckChildcareEligibility.Admin.Usecases.Constants
{
    public static class BulkCheckUseCaseValidationMessages
    {
        public const string MissingReferenceHeader = "The selected file must include the column heading Reference (optional).";
        public const string InvalidHeaders = "The column headings in the selected file must exactly match the template";
        public static string TooManyRows(int rowCountLimit) { return $"The selected file must contain fewer than {rowCountLimit} rows"; }

    }
}