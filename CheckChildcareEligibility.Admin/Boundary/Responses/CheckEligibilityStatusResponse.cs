﻿namespace CheckChildcareEligibility.Admin.Boundary.Responses;

public class CheckEligibilityStatusResponse
{
    public StatusValue Data { get; set; }
}

public class StatusValue
{
    public string Status { get; set; }
}