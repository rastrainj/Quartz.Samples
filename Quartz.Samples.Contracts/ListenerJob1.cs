using Microsoft.Extensions.Logging;

namespace Quartz.Samples.Contracts;

public class ListenerJob1 : IJob
{
	private readonly ILogger<ListenerJob1> _logger;

	public ListenerJob1(ILogger<ListenerJob1> logger)
	{
		_logger = logger;
	}

	public Task Execute(IJobExecutionContext context)
	{
		var jobKey = context.JobDetail.Key;
		_logger.LogInformation("ListenerJob1 {JobKey} at {Date}", jobKey, context.FireTimeUtc);
		return Task.CompletedTask;
	}
}