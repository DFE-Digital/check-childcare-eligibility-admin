using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class SubmissionDateAttribute : ValidationAttribute
{
    private readonly string _isTodaySelectedPropertyName;
    private readonly string _dayPropertyName;
    private readonly string _monthPropertyName;
    private readonly string _yearPropertyName;
    private readonly bool _isRequired;

    public SubmissionDateAttribute(
        string isTodaySelectedPropertyName,
        string dayPropertyName,
        string monthPropertyName,
        string yearPropertyName,
        bool isRequired = true,
        string? errorMessage = null)
        : base(errorMessage)
    {
        _isTodaySelectedPropertyName = isTodaySelectedPropertyName;
        _dayPropertyName = dayPropertyName;
        _monthPropertyName = monthPropertyName;
        _yearPropertyName = yearPropertyName;
        _isRequired = isRequired;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        var isTodaySelectedProperty =
            validationContext.ObjectType.GetProperty(_isTodaySelectedPropertyName);

        var isTodaySelected =
            (bool?)isTodaySelectedProperty?.GetValue(model);

        if (isTodaySelected == true)
        {
            return ValidationResult.Success;
        }
        else if (isTodaySelected == null)
        {
            return ValidationResult.Success; 
        }

        var dayString = GetPropertyStringValue(model, _dayPropertyName);
        var monthString = GetPropertyStringValue(model, _monthPropertyName);
        var yearString = GetPropertyStringValue(model, _yearPropertyName);

        var allFieldsEmpty = string.IsNullOrEmpty(dayString) &&
                                 string.IsNullOrEmpty(monthString) &&
                                 string.IsNullOrEmpty(yearString);

        if (!_isRequired && allFieldsEmpty) return ValidationResult.Success;

        // Collect all invalid fields for highlighting
        var errorFields = new List<string>();

        // Check for missing fields first
        var hasEmptyFields = false;
        if (string.IsNullOrWhiteSpace(dayString))
        {
            errorFields.Add("Day");
            hasEmptyFields = true;
        }

        if (string.IsNullOrWhiteSpace(monthString))
        {
            errorFields.Add("Month");
            hasEmptyFields = true;
        }

        if (string.IsNullOrWhiteSpace(yearString))
        {
            errorFields.Add("Year");
            hasEmptyFields = true;
        }

        // Check for invalid values even if some fields are empty
        if (!string.IsNullOrWhiteSpace(yearString))
        {
            if (int.TryParse(yearString, out var yearInt))
            {
                if (yearInt < 1900)
                    if (!errorFields.Contains("Year"))
                        errorFields.Add("Year");
            }
            else if (!errorFields.Contains("Year"))
            {
                errorFields.Add("Year");
            }
        }

        if (!string.IsNullOrWhiteSpace(monthString))
        {
            if (int.TryParse(monthString, out var monthInt))
            {
                if (monthInt < 1 || monthInt > 12)
                    if (!errorFields.Contains("Month"))
                        errorFields.Add("Month");
            }
            else if (!errorFields.Contains("Month"))
            {
                errorFields.Add("Month");
            }
        }

        if (!string.IsNullOrWhiteSpace(dayString))
        {
            if (int.TryParse(dayString, out var dayInt))
            {
                if (dayInt < 1 || dayInt > 31)
                    if (!errorFields.Contains("Day"))
                        errorFields.Add("Day");
            }
            else if (!errorFields.Contains("Day"))
            {
                errorFields.Add("Day");
            }
        }

        // Always add SubmissionDate to error fields if we have any errors
        if (errorFields.Any() && !errorFields.Contains("SubmissionDate")) errorFields.Insert(0, "SubmissionDate");

        // Determine the appropriate error message while maintaining all error fields
        string message = "";
        if (hasEmptyFields)
        {
            if (errorFields.Count == 2) // One field missing (plus SubmissionDate)
            {
                var missingField = errorFields[1]; // [0] is SubmissionDate
                message = $"Application submitted on date must include a {missingField.ToLower()}";
            }
            else if (errorFields.Count == 4) // All fields missing
            {
                if (yearString != null && yearString.Length < 4)
                {
                    message = "Year must include 4 numbers";
                }
                else
                {
                    message = "Enter application submitted on date";
                }
            }
            else // Multiple but not all fields missing
            {
                message = "Enter a complete application submitted on date";
            }
        }
        else if (errorFields.Any())
        {
            if (yearString.Length < 4)
            {
                message = "Year must include 4 numbers";
            }
            else
            {
                message = "Application submitted on date must be a real date";
            }
        }
        else
        {
            try
            {
                var yearInt = int.Parse(yearString);
                var monthInt = int.Parse(monthString);
                var dayInt = int.Parse(dayString);

                var submissionDate = new DateTime(yearInt, monthInt, dayInt);

                if (submissionDate > DateTime.Now)
                    return new ValidationResult("Application submitted on date must be in the past",
                        new[] { "SubmissionDate", "Day", "Month", "Year" });

                DateTime backdateWindow = DateTime.Now.AddDays(-31);
                if (submissionDate < DateTime.Now.AddDays(-31))
                    return new ValidationResult("The application submitted on date must be after " + @backdateWindow.ToString("d MMMM yyyy"),
                        new[] { "SubmissionDate", "Day", "Month", "Year" });

                return ValidationResult.Success;
            }
            catch
            {
                message = "Application submitted on date must be a real date";
                if (!errorFields.Contains("Day")) errorFields.Add("Day");
                if (!errorFields.Contains("Month")) errorFields.Add("Month");
                if (!errorFields.Contains("Year")) errorFields.Add("Year");
            }
        }

        return new ValidationResult(message, errorFields);
    }

    private string GetPropertyStringValue(object model, string propertyName)
    {
        return model.GetType().GetProperty(propertyName)?.GetValue(model) as string;
    }
}