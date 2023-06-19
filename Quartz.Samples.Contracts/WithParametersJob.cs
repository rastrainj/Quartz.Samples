using Microsoft.Extensions.Logging;

namespace Quartz.Samples.Contracts;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class WithParametersJob : IJob
{
	public const string StringParameterName = "string-parameter";
	public const string IntParameterName = "int-parameter";
	public const string CounterName = "counter";

	private readonly ILogger<WithParametersJob> _logger;

	public WithParametersJob(ILogger<WithParametersJob> logger)
	{
		_logger = logger;
	}

	public Task Execute(IJobExecutionContext context)
	{
		var jobKey = context.JobDetail.Key;

		//var data = context.JobDetail.JobDataMap;
		var data = context.MergedJobDataMap;

		var stringParameter = data.GetString(StringParameterName);
		var intParameter = data.GetInt(IntParameterName);

		var counter = data.GetInt(CounterName);
		counter++;
		//data.PutAsString(CounterName, counter);
		context.JobDetail.JobDataMap.PutAsString(CounterName, counter);

		_logger.LogInformation(
			"WithParametersJob '{JobKey}' string={StringParameter} int={IntParameter} & counter={Counter}",
			jobKey,
			stringParameter,
			intParameter,
			counter);

		return Task.CompletedTask;
	}
}