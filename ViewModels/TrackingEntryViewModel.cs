using ProductTrackingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductTrackingSystem.ViewModels;

public class TrackingEntryViewModel
{
    [Required] public string TagNumber { get; set; } = string.Empty;
    public int? ToDepartmentId { get; set; }
    public string? Location { get; set; }
    [Required] public ProductStatus Status { get; set; }
    [Required] public LinenStage Stage { get; set; }
    public string? Remarks { get; set; }
    public string? HandheldReaderId { get; set; }
}
