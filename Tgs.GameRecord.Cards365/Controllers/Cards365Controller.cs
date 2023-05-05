using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.Cards365.Core.ApplicationServices;
using Tgs.GameRecord.Cards365.Models;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Controllers;

[Route("[controller]")]
[ApiController]
public class Cards365Controller : ControllerBase
{
    /// <summary>
    /// Games the record asynchronous.
    /// </summary>
    /// <param name="commandService">The command service.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///
    ///         {
    ///             "apiUrl": "https://g5ky3i.365gaming.cc/",
    ///             "website": "card365pega001",
    ///             "cert": "40D237D0A995DFEAE0590F99552D3F6F",
    ///             "extension1": "cardp101",
    ///             "startTime": "2023-05-04T00:00:00+08:00",
    ///             "endTime": "2023-05-05T00:00:00+08:00"
    ///         }
    /// </remarks>
    [HttpPost]
    public ValueTask GameRecordAsync(
       [FromServices] ICommandService<ManualPullRecordCommand> commandService,
       [FromBody] ManualPullRecordCommandViewModel source)
        => commandService
        .ExecuteAsync(
            command: new ManualPullRecordCommand(
                    source.ApiUrl,
                    source.Website,
                    source.Cert,
                    source.Extension1,
                    source.StartTime,
                    source.EndTime),
            cancellationToken: HttpContext.RequestAborted);
}