using System.Text.Json;

namespace Quartz.Samples.AspNetCore;

public class Ability
{
	//public Ability Ability { get; set; }
	public bool Is_hidden { get; set; }
	public int Slot { get; set; }
}

public class Pokemon
{
	public Ability[] Abilities { get; set; }
}

public class PokemonJob : IJob
{
	private static System.Text.Json.JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<PokemonJob> _logger;

	public PokemonJob(IHttpClientFactory httpClientFactory, ILogger<PokemonJob> logger)
	{
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		using var httpClient = _httpClientFactory.CreateClient("Pokemon");

		var pokemon = await httpClient.GetFromJsonAsync<Pokemon>("", _options);

		_logger.LogInformation("Pokemon Job at {Date} {Pokemon}", context.FireTimeUtc, JsonSerializer.Serialize(pokemon, _options));

		if (context.CancellationToken.IsCancellationRequested)
		{
			return;
		}
	}
}
