using MpdSharp;

namespace TestMpd;

public class Program {
	public static void Main() {
		Console.WriteLine("Test Mpd!");
		var mpd = new Mpd();
		_ = mpd.Connect();
		var lsinfo = mpd.LsInfo("/");

		// var currentSong = mpd.CurrentSong();
		// mpd.Play();
		// Thread.Sleep(1000);
		// mpd.Pause();
		// Thread.Sleep(1000);
		// mpd.Pause();
		// Thread.Sleep(1000);
		// mpd.Pause();
		// mpd.Close();
	}
}

