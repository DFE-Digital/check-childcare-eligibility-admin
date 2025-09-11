﻿using System.ComponentModel;

namespace CheckChildcareEligibility.Admin.Domain.Enums;

public enum ApplicationStatus
{
    [Description("Entitled")] Entitled,
    [Description("Receiving Entitlement")] Receiving,
    [Description("Evidence Needed")] EvidenceNeeded,
    [Description("Sent for Review")] SentForReview,
    [Description("Reviewed Entitled")] ReviewedEntitled,
    [Description("Reviewed Not Entitled")] ReviewedNotEntitled
}