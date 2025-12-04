namespace CheckChildcareEligibility.Admin.Controllers.Constants
{
    public static class BulkCheckControllerValidationMessages
    {
        public const string NoFileSelected = "Select a CSV file";
        public const string IncorrectFileType = "The selected file must be a CSV";
        public const string FileTooLarge = "The selected file must be smaller than 10MB";
        public static string BulkUploadAttemptLimitReached (string bulkUploadAttemptLimit) { 
            return $"No more than {bulkUploadAttemptLimit} batch check requests can be made per hour"; 
        }
        public const string EmptyFile = "The selected file is empty";
    }
}
