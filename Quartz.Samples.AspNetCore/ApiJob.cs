namespace Quartz.Samples.AspNetCore;

[DisallowConcurrentExecution]
public class ApiJob : IJob
{
	private readonly ILogger<ApiJob> _logger;

	public ApiJob(ILogger<ApiJob> logger)
	{
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		_logger.LogInformation("Executing ApiJob at {Date}", context.FireTimeUtc);

		await Task.Delay(5_000);

		_logger.LogInformation("Executed ApiJob");
	}
}
