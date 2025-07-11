using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace GuiMpc;


public class App : Application {
	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted() {
		// If you use CommunityToolkit, line below is needed to remove Avalonia data validation.
		// Without this line you will get duplicate validations from both Avalonia and CT
		BindingPlugins.DataValidators.RemoveAt(0);

		// Register all the services needed for the application to run
		var collection = new ServiceCollection();
		collection.AddCommonServices();

		// Creates a ServiceProvider containing services from the provided IServiceCollection
		var services = collection.BuildServiceProvider();
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			var mpdWrapper = services.GetRequiredService<MpdWrapper>();
			var logger = services.GetRequiredService<ILogger<MainWindow>>();
			desktop.MainWindow = new MainWindow(mpdWrapper, logger);
		}
		base.OnFrameworkInitializationCompleted();
	}
}

public class Config {
	public string? HostName { get; set; }
	public int? Port { get; set; }
}

public static class InitEnv {
	public static void AddCommonServices(this IServiceCollection collection) {
		var settings = GetConfig();
		collection
			.AddLogging(builder => {
				builder.AddSimpleConsole(options => {
						options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
						options.SingleLine = true;
					}
				);
#if DEBUG
				builder.SetMinimumLevel(LogLevel.Debug);
#else
			builder.SetMinimumLevel(LogLevel.Error);
#endif
			})
			.AddSingleton<Config>(s => settings)
			.AddTransient<MpdWrapper>();
		MpdSharp.Init.Services(collection);
		collection.BuildServiceProvider();
	}


	private static Config GetConfig() {
		var args = Environment.GetCommandLineArgs();
		var serverStr = Environment.GetEnvironmentVariable("GUIMPC_SERVER");
		var portStr = Environment.GetEnvironmentVariable("GUIMPC_PORT");
		int? port = null;
		if (int.TryParse(portStr, out var tryPort)) {
			port = tryPort;
		}
		var settings = new Config {
			HostName = serverStr,
			Port = port
		};
		if (args.Length <= 1) {
			return settings;
		}
		for (var i = 0; i < args.Length; i++) {
			switch (args[i]
						.ToLower()) {
				case "--server":
				case "-s":
					i++;
					settings.HostName = args[i];
					break;
				case "--port":
				case "-p":
					i++;
					settings.Port = int.Parse(args[i]);
					break;
				default:
					Console.WriteLine($"{args[0]} help:\n --port -p n Where n is the port MPD is using.\n --server -s ip Where ip is the IP address or hostname MPD is running on.\nguimpc is written by Stuart Geddes and is licensed under the GPL.");
					if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
						lifetime.Shutdown();
					}
					break;
			}
		}
		return settings;
	}
}

public static class GlobalImageOptions {
	public static readonly AttachedProperty<BitmapInterpolationMode> InterpolationModeProperty = AvaloniaProperty.RegisterAttached<Image, BitmapInterpolationMode>("InterpolationMode", typeof(GlobalImageOptions), defaultValue: BitmapInterpolationMode.HighQuality);

	// Doesn't appear to be working as expected.
	public static void SetInterpolationMode(Image image, BitmapInterpolationMode value) {
		//image.SetValue(RenderOptions.BitmapInterpolationModeProperty, value);
		RenderOptions.SetBitmapInterpolationMode(image, value);
	}
}
