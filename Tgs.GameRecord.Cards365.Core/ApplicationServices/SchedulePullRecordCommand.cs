namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

public record SchedulePullRecordCommand(
	Uri ApiUrl,
	string Website,
	string Cert,
	string Extension1);
