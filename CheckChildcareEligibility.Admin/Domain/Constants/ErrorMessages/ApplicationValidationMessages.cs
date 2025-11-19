namespace CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;

public static class ValidationMessages
{
    public const string RequiredLastName = "Enter parent or guardian's last name";
    public const string RequiredDOB = "Enter parent or guardian's date of birth";
    public const string RequiredNI = "Enter parent or guardian's National Insurance number";
    public const string ValidLastName = "Parent or guardian's last name should not contain numbers";
    public const string ValidDOB = "The date of birth must be in yyyy-mm-d or dd-mm-yyyy format";
    public const string ValidNI = "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C";
    public const string ChildDOB = "Child Date of birth is required:- (yyyy-mm-dd)";
    public const string ChildLastName = "Child LastName is required";
    public const string ChildFirstName = "Child FirstName is required";
    public const string RequiredEligibilityCode = "Enter an eligibility code that is 11 digits long";
    public const string EligibilityCodeNumber  = "Eligibility code must only contain numbers";
    public const string EligibilityCodeIncorrectLength = "Eligibility code must be 11 digits long";
}