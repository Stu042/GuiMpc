using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace MpdSharp;


public class Init {
	public static void Services(IServiceCollection collection) {
		collection
			.AddLogging(builder => {
				builder.AddConsole();
#if DEBUG
				builder.SetMinimumLevel(LogLevel.Debug);
#else
			builder.SetMinimumLevel(LogLevel.Error);
#endif
			})
			.AddSingleton<Mpd>()
			.AddSingleton<Coms>();
	}
}
