using Microsoft.Extensions.Logging;

namespace Quartz.Samples.Contracts;

public class SimpleJob : IJob
{
	private readonly ILogger<SimpleJob> _logger;

	public SimpleJob(ILogger<SimpleJob> logger)
	{
		_logger = logger;
	}

	public Task Execute(IJobExecutionContext context)
	{
		_logger.LogInformation("Simple Job at {Date}", context.FireTimeUtc);

		return Task.CompletedTask;
	}
}