using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Quartz.Samples.AspNetCore;
using Quartz.Samples.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("Pokemon", client =>
{
	client.BaseAddress = new Uri("https://pokeapi.co/api/v2/pokemon/ditto");
});

builder.Services.Configure<QuartzOptions>(options =>
{
	options.SchedulerName = "Quartz ASP.NET Core Sample Scheduler";
	options.Scheduling.IgnoreDuplicates = true;
	options.Scheduling.OverWriteExistingData = true;
});

builder.Services.AddQuartz(quartzConfigurator =>
{
	//quartzConfigurator.SchedulerName = "MassTransit-Scheduler";
	//quartzConfigurator.SchedulerId = "AUTO";

	quartzConfigurator.UseMicrosoftDependencyInjectionJobFactory();

	quartzConfigurator.UseDefaultThreadPool(options => options.MaxConcurrency = 10);

	quartzConfigurator.UseTimeZoneConverter();

	quartzConfigurator.UsePersistentStore(store =>
	{
		store.PerformSchemaValidation = true;
		store.UseProperties = true;
		store.RetryInterval = TimeSpan.FromSeconds(5);

		store.UseSqlServer(options =>
		{
			options.ConnectionStringName = "Database";
		});

		store.UseJsonSerializer();
	});

	//var apiJobKey = new JobKey("api-job", "api-jobs");
	//var job = JobBuilder.Create<ApiJob>()
	//	.WithIdentity(apiJobKey)
	//	.Build();

	//quartzConfigurator.AddJob<ApiJob>(apiJobKey, config => config.StoreDurably());

	//var parametersJobKey = new JobKey("withParameters-job", "api-jobs");
	//var withParametersJob = JobBuilder.Create<WithParametersJob>()
	//	.WithIdentity(parametersJobKey)
	//	.Build();

	//quartzConfigurator.AddJob<WithParametersJob>(parametersJobKey, config => config.StoreDurably());

	//var jobKey = new JobKey("simple-job", "worker-jobs");
	//quartzConfigurator.AddJob<SimpleJob>(j =>
	//{
	//	j.StoreDurably();
	//	j.WithIdentity(jobKey);
	//});

	//quartzConfigurator.AddTrigger(t =>
	//{
	//	t.WithIdentity("simple-trigger", "worker-jobs")
	//		.StartNow()
	//		.ForJob(jobKey)
	//		.WithSimpleSchedule(x => x
	//			.WithIntervalInSeconds(5)
	//			.WithRepeatCount(10));
	//});

	var pokemonKey = new JobKey("pokemon-job", "api-jobs");
	quartzConfigurator.AddJob<PokemonJob>(pokemonKey, config => config.StoreDurably());

	quartzConfigurator.AddTrigger(t =>
	{
		t.WithIdentity("pokemon-trigger", "api-jobs")
			.StartNow()
			.ForJob(pokemonKey)
			.WithSimpleSchedule(x => x
				.WithIntervalInSeconds(2)
				.RepeatForever()
				.WithMisfireHandlingInstructionIgnoreMisfires());
	});
});

builder.Services.AddQuartzServer(options =>
{
	options.WaitForJobsToComplete = true;
	options.StartDelay = TimeSpan.FromSeconds(5);
});

builder.Services.Configure<MassTransitHostOptions>(options =>
{
	options.WaitUntilStarted = true;
});

builder.Services.AddMassTransit(x =>
{
	x.AddPublishMessageScheduler();
	x.AddQuartzConsumers();

	x.SetKebabCaseEndpointNameFormatter();

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.UsePublishMessageScheduler();
		cfg.ConfigureEndpoints(context);
	});
});

builder.Services
	.AddOpenTelemetry()
	.ConfigureResource(resourceBuilder =>
	{
		resourceBuilder.AddService(
			"Quartz.Samples",
			serviceVersion: "0.0.1",
			serviceInstanceId: Environment.MachineName);
	})
	.WithTracing(tracingBuilder =>
	{
		tracingBuilder
			.AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
			.AddAspNetCoreInstrumentation()
			.AddQuartzInstrumentation()
			.AddHttpClientInstrumentation()
			.AddJaegerExporter(options =>
			{
				options.AgentHost = "localhost";
				options.AgentPort = 6831;
				options.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
			});
	});

var app = builder.Build();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapPost("/schedule", async (ISchedulerFactory schedulerFactory, CancellationToken cancellationToken) =>
{
	var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

	await scheduler.TriggerJob(new JobKey("api-job", "api-jobs"), cancellationToken);

	return TypedResults.Ok();
});


app.MapPost("/schedule-parameters", async (string name, int number, ISchedulerFactory schedulerFactory, CancellationToken cancellationToken) =>
{
	var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

	var data = new JobDataMap();
	data.Put(WithParametersJob.StringParameterName, name);
	data.PutAsString(WithParametersJob.IntParameterName, number);

	await scheduler.TriggerJob(new JobKey("withParameters-job", "api-jobs"), data, cancellationToken);

	return TypedResults.Ok();
});

app.MapPost("/masstransit", async (string description, IMessageScheduler messageScheduler, CancellationToken cancellationToken) =>
{
	await messageScheduler.SchedulePublish(TimeSpan.FromSeconds(1), new QuartzMessage { Description = description }, cancellationToken);
});

app.MapPut("/pause-pokemon", async (ISchedulerFactory schedulerFactory, CancellationToken cancellationToken) =>
{
	var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

	var jobKey = new JobKey("pokemon-job", "api-jobs");

	//var jobs = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);

	//await scheduler.Interrupt(jobKey);
	await scheduler.PauseJob(jobKey);
	//await scheduler.DeleteJob(jobKey);
});

app.MapPut("/resume-pokemon", async (ISchedulerFactory schedulerFactory, CancellationToken cancellationToken) =>
{
	var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

	var jobKey = new JobKey("pokemon-job", "api-jobs");

	await scheduler.ResumeJob(jobKey);
});

await app.RunAsync();
