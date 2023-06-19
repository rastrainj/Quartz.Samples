using Microsoft.Extensions.Logging;

namespace Quartz.Samples.Contracts;

public class CronJob : IJob
{
	private readonly ILogger<CronJob> _logger;

	public CronJob(ILogger<CronJob> logger)
	{
		_logger = logger;
	}

	public Task Execute(IJobExecutionContext context)
	{
		_logger.LogInformation("Cron Job at {Date}", context.FireTimeUtc);

		return Task.CompletedTask;
	}
}