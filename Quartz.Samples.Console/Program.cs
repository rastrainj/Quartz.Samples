using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Samples.Contracts;

var builder = Host.CreateDefaultBuilder()
	.ConfigureServices((context, services) =>
	{
		services.AddQuartz(quartzConfigurator =>
		{
			quartzConfigurator.UseMicrosoftDependencyInjectionJobFactory();
		});

		services.AddQuartzHostedService(options =>
		{
			options.WaitForJobsToComplete = true;
		});
	})
	.Build();

var schedulerFactory = builder.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();

//var simpleJob = JobBuilder.Create<SimpleJob>()
//	.WithIdentity("simple-job", "console-jobs")
//	.Build();

//var simpleTrigger = TriggerBuilder.Create()
//	.WithIdentity("simple-trigger", "console-jobs")
//	.StartNow()
//	.WithSimpleSchedule(x => x
//		.WithIntervalInSeconds(5)
//		.WithRepeatCount(10))
//	.Build();

//await scheduler.ScheduleJob(simpleJob, simpleTrigger);

//var cronJob = JobBuilder.Create<CronJob>()
//	.WithIdentity("cron-job", "console-jobs")
//	.Build();

//// cada 2 segundos
//var cronTrigger = TriggerBuilder.Create()
//	.WithIdentity("cron-trigger", "console-jobs")
//	.StartNow()
//	.WithCronSchedule("0/2 * * * * ?")
//	.Build();

//await scheduler.ScheduleJob(cronJob, cronTrigger);

//var withParametersJob = JobBuilder.Create<WithParametersJob>()
//	.WithIdentity("withparameters-job", "console-jobs")
//	.Build();

//var withParametersTrigger = TriggerBuilder.Create()
//	.WithIdentity("withparameters-trigger", "console-jobs")
//	.WithSimpleSchedule(x => x
//		.WithIntervalInSeconds(1)
//		.RepeatForever()
//		)
//	.Build();

//withParametersJob.JobDataMap.Put(WithParametersJob.StringParameterName, "My-String-Parameter");
//withParametersJob.JobDataMap.Put(WithParametersJob.IntParameterName, 1);

//await scheduler.ScheduleJob(withParametersJob, withParametersTrigger);

//await scheduler.Start();

var listenerJob1 = JobBuilder.Create<ListenerJob1>()
	.WithIdentity("listener-job-1", "console-jobs")
	.Build();

var listenerTrigger1 = TriggerBuilder.Create()
	.WithIdentity("listener-trigger-1", "console-jobs")
	.StartNow()
	.Build();

var listener = new JobListener(builder.Services.GetRequiredService<ILoggerFactory>());
var matcher = KeyMatcher<JobKey>.KeyEquals(listenerJob1.Key);
scheduler.ListenerManager.AddJobListener(listener, matcher);

await scheduler.ScheduleJob(listenerJob1, listenerTrigger1);

await scheduler.Start();

await builder.RunAsync();