using Microsoft.Extensions.Logging;

namespace Quartz.Samples.Contracts;

public class ListenerJob2 : IJob
{
	private readonly ILogger<ListenerJob2> _logger;

	public ListenerJob2(ILogger<ListenerJob2> logger)
	{
		_logger = logger;
	}

	public Task Execute(IJobExecutionContext context)
	{
		var jobKey = context.JobDetail.Key;
		_logger.LogInformation("ListenerJob2 {JobKey} at {Date}", jobKey, context.FireTimeUtc);
		return Task.CompletedTask;
	}
}