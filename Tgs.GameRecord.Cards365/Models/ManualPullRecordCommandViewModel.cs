using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.Cards365.Models;

public class ManualPullRecordCommandViewModel
{
    [Required]
    public Uri ApiUrl { get; init; } = default!;

    [Required]
    public string Website { get; init; } = default!;

    [Required]
    public string Cert { get; init; } = default!;

    [Required]
    public string Extension1 { get; init; } = default!;

    [Required]
    public DateTimeOffset StartTime { get; init; }

    [Required]
    public DateTimeOffset EndTime { get; init; }
}