namespace MpdSharp.Models;


public class StatusModel {
	public int Volume { get; set; }            // volume: 64
	public bool Repeat { get; set; }           // repeat: 0
	public bool Random { get; set; }           // random: 0
	public bool Single { get; set; }           // single: 0
	public bool Consume { get; set; }          // consume: 0
	public string Partition { get; set; }      // partition: default
	public int Playlist { get; set; }          // playlist: 3
	public int PlaylistLength { get; set; }    // playlistlength: 13
	public double MixRampDb { get; set; }      // mixrampdb: 0
	public string State { get; set; }          // state: pause
	public int LastLoadedPlaylist { get; set; }// lastloadedplaylist:
	public int Song { get; set; }              // song: 1
	public int SongId { get; set; }            // songid: 2
	public double Time { get; set; }           // time: 21:286
	public double Elapsed { get; set; }        // elapsed: 20.872
	public int BitRate { get; set; }           // bitrate: 1128
	public double Duration { get; set; }       // duration: 285.680
	public FormatModel Audio { get; set; }     // audio: 44100:16:2
	public int NextSong { get; set; }          // nextsong: 2
	public int NextSongId { get; set; }        // nextsongid: 3

	public StatusModel(string response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Parse(parsedResponse);
	}
	public StatusModel(byte[] response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Parse(parsedResponse);
	}

	private void Parse(CrazyDict crazyDict) {
		Volume = crazyDict.IntVal("volume") ?? 0;
		Repeat = crazyDict.BoolVal("repeat") ?? false;
		Random = crazyDict.BoolVal("random") ?? false;
		Single = crazyDict.BoolVal("single") ?? false;
		Consume = crazyDict.BoolVal("consume") ?? false;
		Partition = crazyDict.Value("partition");
		Playlist = crazyDict.IntVal("playlist") ?? -1;
		PlaylistLength = crazyDict.IntVal("playlistlength") ?? -1;
		MixRampDb = crazyDict.DoubleVal("mixrampdb") ?? 0;
		State = crazyDict.Value("state");
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
