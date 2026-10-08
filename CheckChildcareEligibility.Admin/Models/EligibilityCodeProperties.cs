using CheckChildcareEligibility.Admin.Boundary.Responses;
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
            ChildIsTooYoung = source.ChildTooYoung;
        }

        public EligibilityCodeProperties(FosterCodePreviewResponse source)
        {
            Type = EligibilityCodeType.Foster;
            Status = CheckEligibilityStatus.eligible;
            ValidityStartDate = source.ValidityStartDate;
            ValidityEndDate = source.ValidityEndDate;
            GracePeriodEndDate = source.GracePeriodEndDate;
            IsGracePeriodEndDateApplied = source.IsGracePeriodEndDateApplied;
            TermValidity = source.TermValidity;
            ReconfirmationProperties = source.ReconfirmationProperties;
            ChildIsTooYoung = source.ChildTooYoung;
        }

        public EligibilityCodeProperties(CheckEligibilityItemWorkingFamilies source)
        {
            Type = source.EligibilityCodeType ?? EligibilityCodeType.Standard;
            Status = Enum.Parse<CheckEligibilityStatus>(source.Status);
            EligibilityCode = source.EligibilityCode;
            ValidityStartDate = source.ValidityStartDate;
            ValidityEndDate = source.ValidityEndDate;
            GracePeriodEndDate = source.GracePeriodEndDate;
            ChildDateOfBirth = DateTime.Parse(source.DateOfBirth);
            TermValidity = source.TermValidity;
            ReconfirmationProperties = source.ReconfirmationProperties;
            ChildIsTooYoung = source.ChildTooYoung;
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
            ChildIsTooYoung = item.ChildTooYoung;
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

        public bool ChildIsTooYoung { get; set; }

        // Calculated properties

        public bool IsEligible => Status == CheckEligibilityStatus.eligible;

        public bool IsExpired => GracePeriodEndDate < DateTime.UtcNow.Date;

        public Term CurrentTerm => TermValidity.Current ?? Term.None;

        public Term NextTerm => TermValidity.Next ?? Term.None;

        public bool ChildIsTooOld => ReconfirmationProperties.Status == ReconfirmationStatus.ChildTooOld;

        public bool IsInGracePeriod => DateTime.UtcNow.Date > ValidityEndDate && DateTime.UtcNow.Date <= GracePeriodEndDate;

        public bool IsNotValidForCurrentTerm => CurrentTerm.Name == TermName.None && NextTerm.Name != TermName.None;

        public bool IsNeverValid => CurrentTerm.Name == TermName.None && NextTerm.Name == TermName.None;

        public bool IsReconfirmed => IsEligible && !IsNotValidForCurrentTerm && !IsInGracePeriod && NextTerm.Name != TermName.None;

    }
}