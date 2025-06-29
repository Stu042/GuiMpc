namespace MpdSharp.Models;


public enum State {
	Play,
	Stop,
	Pause
}

public class StatusModel {
	/// <summary>(deprecated: -1 if the volume cannot be determined)</summary>
	//public int Volume { get; set; }            // volume: 64
	public bool Repeat { get; set; }           // repeat: 0
	public bool Random { get; set; }           // random: 0
	public bool Single { get; set; }           // single: 0
	public bool Consume { get; set; }          // consume: 0

	/// <summary>The name of the current partition (see Partition commands)</summary>
	public string Partition { get; set; }      // partition: default

	/// <summary>31-bit unsigned integer, the playlist version number</summary>
	public int Playlist { get; set; }          // playlist: 3

	/// <summary>integer, the length of the playlist</summary>
	public int PlaylistLength { get; set; }    // playlistlength: 13

	/// <summary>mixramp threshold in dB</summary>
	public double MixRampDb { get; set; }      // mixrampdb: 0

	/// <summary>play, stop, or pause</summary>
	public State State { get; set; }          // state: pause

	/// <summary>last loaded stored playlist</summary>
	public int LastLoadedPlaylist { get; set; }// lastloadedplaylist:

	/// <summary>playlist song number of the current song stopped on or playing</summary>
	public int Song { get; set; }              // song: 1

	/// <summary>playlist songid of the current song stopped on or playing</summary>
	public int SongId { get; set; }            // songid: 2

	/// <summary>total time elapsed (of current playing/paused song) in seconds (deprecated, use elapsed instead)</summary>
	public double Time { get; set; }           // time: 21:286

	/// <summary>Total time elapsed within the current song in seconds, but with higher resolution.</summary>
	public double Elapsed { get; set; }        // elapsed: 20.872

	/// <summary>instantaneous bitrate in kbps</summary>
	public int BitRate { get; set; }           // bitrate: 1128

	/// <summary>Duration of the current song in seconds.</summary>
	public double Duration { get; set; }       // duration: 285.680

	/// <summary>The format emitted by the decoder plugin during playback, format: samplerate:bits:channels.</summary>
	public FormatModel Audio { get; set; }     // audio: 44100:16:2

	/// <summary>playlist song number of the next song to be played</summary>
	public int NextSong { get; set; }          // nextsong: 2

	/// <summary>playlist songid of the next song to be played</summary>
	public int NextSongId { get; set; }        // nextsongid: 3

	//xfade: crossfade in seconds (see Cross-Fading)
	//mixrampdelay: mixrampdelay in seconds

	public StatusModel(string response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Parse(parsedResponse);
	}
	public StatusModel(byte[] response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Parse(parsedResponse);
	}

	private void Parse(CrazyDict crazyDict) {
		//Volume = crazyDict.IntVal("volume") ?? 0;
		Repeat = crazyDict.BoolVal("repeat") ?? false;
		Random = crazyDict.BoolVal("random") ?? false;
		Single = crazyDict.BoolVal("single") ?? false;
		Consume = crazyDict.BoolVal("consume") ?? false;
		Partition = crazyDict.Value("partition");
		Playlist = crazyDict.IntVal("playlist") ?? -1;
		PlaylistLength = crazyDict.IntVal("playlistlength") ?? -1;
		MixRampDb = crazyDict.DoubleVal("mixrampdb") ?? 0;
		State = Enum.Parse<State>(crazyDict.Value("state"), ignoreCase: true);
		LastLoadedPlaylist = crazyDict.IntVal("lastloadedplaylist") ?? -1;
		Song = crazyDict.IntVal("song") ?? -1;
		SongId = crazyDict.IntVal("songid") ?? -1;
		var time = crazyDict
			.Value("time")
			.Replace(':', '.');
		if (!string.IsNullOrEmpty(time)) {
			Time = double.Parse(time);
		} else {
			Time = 0;
		}
		Elapsed = crazyDict.DoubleVal("elapsed") ?? 0;
		BitRate = crazyDict.IntVal("bitrate") ?? 0;
		Duration = crazyDict.DoubleVal("duration") ?? 0;
		Audio = new FormatModel(crazyDict.Value("audio"));
		NextSong = crazyDict.IntVal("nextsong") ?? -1;
		NextSongId = crazyDict.IntVal("nextsongid") ?? -1;
	}
}
