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
using MpdSharp;


namespace GuiMpc;


public partial class App : Application {
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
				builder.AddConsole();
#if DEBUG
				builder.SetMinimumLevel(LogLevel.Debug);
#else
			builder.SetMinimumLevel(LogLevel.Error);
#endif
			})
			.AddSingleton<Mpd>(s => new Mpd())
			.AddSingleton<Config>(s => settings)
			.AddTransient<MpdWrapper>();
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
		if (args.Length <= 1)
			return settings;
		switch (args[1]
					.ToLower()) {
			case "--server":
			case "-s":
				settings.HostName = args[2];
				break;
			case "--port":
			case "-p":
				settings.Port = int.Parse(args[2]);
				break;
			default:
				Console.WriteLine("Run GuiMpc ");
				if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
					lifetime.Shutdown();
				}
				break;
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
