using System.ComponentModel.DataAnnotations.Schema;
using CheckChildcareEligibility.Admin.Attributes;

namespace CheckChildcareEligibility.Admin.Models;

public class EvidenceFile
{
    [NotMapped] public int FileIndex { get; set; }

    public string? FileName { get; set; }

    public string? FileType { get; set; }
    public string StorageAccountReference { get; set; }
}