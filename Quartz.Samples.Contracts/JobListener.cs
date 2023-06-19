using Microsoft.Extensions.Logging;

namespace Quartz.Samples.Contracts;

public class JobListener : IJobListener
{
	private readonly ILogger<JobListener> _logger;

	public JobListener(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<JobListener>();
	}

	public string Name => "job1-->job2";

	public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("JobListener execution vetoed at {Date}", context.FireTimeUtc);
		return Task.CompletedTask;
	}

	public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("JobListener to be executed at {Date}", context.FireTimeUtc);
		return Task.CompletedTask;
	}

	public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("JobListener was executed at {Date}", context.FireTimeUtc);

		var job2 = JobBuilder.Create<ListenerJob2>()
			.WithIdentity("listener-job-2", "console-jobs")
			.Build();

		var trigger2 = TriggerBuilder.Create()
			.WithIdentity("listener-trigger-2", "console-jobs")
			.StartNow()
			.Build();

		try
		{
			// schedule the job to run!
			await context.Scheduler.ScheduleJob(job2, trigger2, cancellationToken);
		}
		catch (SchedulerException ex)
		{
			_logger.LogError(ex, "Unable to schedule job2!");
		}
	}
}