using MpdSharp;

namespace TestMpd;

public class Program {
	public static void Main() {
		Console.WriteLine("Test Mpd!");
		// var mpd = new Mpd();
		// _ = mpd.Connect();
		// var currentSong = mpd.CurrentSong();
		// mpd.Play();
		// Thread.Sleep(1000);
		// mpd.Pause();
		// Thread.Sleep(1000);
		// mpd.Pause();
		// Thread.Sleep(1000);
		// mpd.Pause();
		// mpd.Close();

		for (int origMpd = 0; origMpd <= 100; origMpd++) {
			var slider = VolumeMpdToSlider(origMpd);
			var newMpd = VolumeSliderToMpd(slider);
			if (origMpd != newMpd) {
				Console.WriteLine($"Mpd: {origMpd} -> {newMpd}");
			}
		}
		Console.WriteLine(CurrentSongFullText("artist", "album", "title"));
	}

	private static string CurrentSongFullText(string? artist, string? album, string? title) {
		var hasArtist = !string.IsNullOrEmpty(artist);
		var hasAlbum = !string.IsNullOrEmpty(album);
		var hasTitle = !string.IsNullOrEmpty(title);
		var bob = new List<string>();
		if (hasArtist) {
			bob.Add(artist);
		}
		if (hasAlbum) {
			bob.Add(album);
		}
		if (hasTitle) {
			bob.Add(title);
		}
		var fullTitle = string.Join(" - ", bob);
		return fullTitle;
	}

	private static int VolumeSliderToMpd(double sliderVal) {
		var mpdVol = (int)Math.Round(Math.Sqrt(sliderVal) * 10.0);
		return mpdVol;
	}
	private static double VolumeMpdToSlider(int mpdVal) {
		var sliderVol = mpdVal * mpdVal / 100.0;
		return sliderVol;
	}
}

