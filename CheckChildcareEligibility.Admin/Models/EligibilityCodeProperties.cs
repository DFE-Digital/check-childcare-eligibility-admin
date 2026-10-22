using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Constants.Generic;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Enums.WorkingFamilies;

namespace CheckChildcareEligibility.Admin.Models
{
    public class EligibilityCodeProperties
    {

        public EligibilityCodeProperties(FosterChildResponse source)
        {
            Type = EligibilityCodeType.Foster;
            Status = CheckEligibilityStatus.eligible;
            EligibilityCode = source.EligibilityCode;
            ValidityStartDate = source.ValidityStartDate;
            ValidityEndDate = source.ValidityEndDate;
            GracePeriodEndDate = source.GracePeriodEndDate;
            IsGracePeriodEndDateApplied = source.IsGracePeriodEndDateApplied;
            ChildDateOfBirth = source.ChildDateOfBirth;
            TermValidity = source.TermValidity;
            ReconfirmationProperties = source.ReconfirmationProperties;
            ChildTooYoung = source.ChildTooYoung;
            ChildTooOld = source.ChildTooOld ||
                source.ReconfirmationProperties?.Status == ReconfirmationStatus.ChildTooOld;
        }

        public EligibilityCodeProperties(FosterCodePreviewResponse source, DateTime childDateOfBirth)
        {
            Type = EligibilityCodeType.Foster;
            Status = CheckEligibilityStatus.eligible;
            ValidityStartDate = source.ValidityStartDate;
            ValidityEndDate = source.ValidityEndDate;
            GracePeriodEndDate = source.GracePeriodEndDate;
            IsGracePeriodEndDateApplied = source.IsGracePeriodEndDateApplied;
            ChildDateOfBirth = childDateOfBirth;
            TermValidity = source.TermValidity;
            ReconfirmationProperties = source.ReconfirmationProperties;
            ChildTooYoung = source.ChildTooYoung;
            ChildTooOld = source.ChildTooOld ||
                source.ReconfirmationProperties?.Status == ReconfirmationStatus.ChildTooOld;
        }

        public EligibilityCodeProperties(CheckEligibilityItemWorkingFamilies source)
        {
            Type = source?.EligibilityCodeType ?? EligibilityCodeType.Standard;
            Status = Enum.Parse<CheckEligibilityStatus>(source.Status);
            EligibilityCode = source.EligibilityCode;
            ValidityStartDate = source.ValidityStartDate;
            ValidityEndDate = source.ValidityEndDate;
            GracePeriodEndDate = source.GracePeriodEndDate;
            IsGracePeriodEndDateApplied = source.IsGracePeriodEndDateApplied;
            ChildDateOfBirth = DateTime.Parse(source.DateOfBirth);
            TermValidity = source.TermValidity;
            ReconfirmationProperties = source.ReconfirmationProperties;
            ChildTooYoung = source.ChildTooYoung;
            ChildTooOld = source.ChildTooOld ||
                source.ReconfirmationProperties?.Status == ReconfirmationStatus.ChildTooOld;
        }

        public EligibilityCodeProperties(FosterFamiliesSearchItemResponse item)
        {
            Type = EligibilityCodeType.Foster;
            Status = CheckEligibilityStatus.eligible;
            EligibilityCode = item.EligibilityCode;
            ValidityStartDate = item.ValidityStartDate;
            ValidityEndDate = item.ValidityEndDate;
            IsGracePeriodEndDateApplied = item.IsGracePeriodEndDateApplied;
            GracePeriodEndDate = item.GracePeriodEndDate;
            ChildDateOfBirth = item.ChildDateOfBirth;
            TermValidity = item.TermValidity;
            ReconfirmationProperties = item.ReconfirmationProperties;
            ChildTooYoung = item.ChildTooYoung;
            ChildTooOld = item.ChildTooOld ||
                item.ReconfirmationProperties?.Status == ReconfirmationStatus.ChildTooOld;
        }

        public CheckEligibilityStatus Status { get; init; }

        public EligibilityCodeType Type { get; init; }

        public string EligibilityCode { get; init; }

        public DateTime ChildDateOfBirth { get; init; }

        public DateTime ValidityStartDate { get; init; }

        public DateTime ValidityEndDate { get; init; }

        public DateTime GracePeriodEndDate { get; init; }

        public bool IsGracePeriodEndDateApplied { get; init; }

        public ReconfirmationProperties ReconfirmationProperties { get; init; }

        public TermValidity TermValidity { get; init; }

        public bool ChildTooYoung { get; set; }

        public bool ChildTooOld { get; set; }

        // Calculated properties

        public bool IsEligible => Status == CheckEligibilityStatus.eligible;

        public bool IsExpired => (IsGracePeriodEndDateApplied ? GracePeriodEndDate : ValidityEndDate) < DateTime.UtcNow.Date;

        public Term CurrentTerm => TermValidity.Current ?? Term.None;

        public Term NextTerm => TermValidity.Next ?? Term.None;

        public bool IsInGracePeriod => DateTime.UtcNow.Date > ValidityEndDate && DateTime.UtcNow.Date <= GracePeriodEndDate && IsGracePeriodEndDateApplied;

        public bool IsNotValidForCurrentTerm => CurrentTerm.Name == TermName.None && NextTerm.Name != TermName.None;

        public bool IsNeverValid => CurrentTerm.Name == TermName.None && NextTerm.Name == TermName.None;

        public bool IsReconfirmed => IsEligible && !IsNotValidForCurrentTerm && !IsInGracePeriod && NextTerm.Name != TermName.None;


        public string GracePeriodEndDisplay
        {
            get
            {
                if (ChildTooOld)
                {
                    return WorkingFamiliesResponseDetails.StatusNotApplicable;
                }
                if(IsExpired && !IsGracePeriodEndDateApplied)
                {
                    return WorkingFamiliesResponseDetails.StatusNotApplicable;
                }
                if ((IsEligible && ChildTooYoung) || IsNotValidForCurrentTerm || !IsGracePeriodEndDateApplied)
                {
                    return WorkingFamiliesResponseDetails.GracePeriodEndDateNotAvailable;
                }
                return GracePeriodEndDate.ToString("d MMMM yyyy");
            }
        }

        public string GracePeriodEndLabel => ChildTooOld
                ? "Grace period ended"
                : "Grace period ends";

    }
}