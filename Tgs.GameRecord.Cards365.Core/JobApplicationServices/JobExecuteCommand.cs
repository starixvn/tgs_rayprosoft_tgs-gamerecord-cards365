namespace Tgs.GameRecord.Cards365.Core.JobApplicationServices;

public record JobExecuteCommand(
    string GameProviderCode,
    string Name,
    Uri ApiUrl,
    string Website,
    string Cert,
    string Extension1);