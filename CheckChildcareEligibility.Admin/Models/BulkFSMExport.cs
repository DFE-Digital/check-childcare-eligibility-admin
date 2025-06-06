﻿using CsvHelper.Configuration.Attributes;

namespace CheckChildcareEligibility.Admin.Models;

public class BulkFSMExport
{
    [Name("Parent NI Number")] public string NI { get; set; }

    [Name("Parent Date of Birth")] public string DOB { get; set; }

    [Name("Parent Last Name")] public string LastName { get; set; }

    [Name("Outcome")] public string Outcome { get; set; }
}