﻿// Ignore Spelling: Fsm

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CheckChildcareEligibility.Admin.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum CheckEligibilityType
{
    None = 0,
    [Description("Free School Meals")] FreeSchoolMeals,
    EarlyYearPupilPremium,
    TwoYearOffer
}