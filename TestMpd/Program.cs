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

