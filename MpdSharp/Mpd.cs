using Microsoft.Extensions.Logging;
using MpdSharp.Models;


namespace MpdSharp;


public class Mpd {
	private readonly Coms _coms;
	private readonly ILogger<Mpd> _logger;
	public Mpd(Coms coms, ILogger<Mpd> logger) {
		_coms = coms;
		_logger = logger;
	}

	public bool Connect(string? serverIp = null, int? portNo = null) {
		return _coms.Connect(serverIp, portNo);
	}
	public void Close() {
		_coms.Close();
	}



	public bool Next() {
		_coms.Send("next");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Next returned error: {ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool Pause() {
		_coms.Send("pause");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Pause returned error: {ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool Play(int? position = null) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"play{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Play{pos} returned error: {ErrorMessage}", pos, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool PlayId(int songId) {
		_coms.Send($"playid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Playid {songId} returned error: {ErrorMessage}", songId, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool Previous() {
		_coms.Send("previous");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Previous returned error: {ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}

	public bool Seek(int songPos, double time) {
		_coms.Send($"seek {songPos} {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Seek {songPos} {time} returned error: {ErrorMessage}", songPos, time, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SeekId(int songId, double time) {
		_coms.Send($"seekid {songId} {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Seekid {songId} {time} returned error: {ErrorMessage}", songId, time, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SeekCurrent(double time) {
		_coms.Send($"seekcur {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Seekcur {time} returned error: {ErrorMessage}", time, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool Stop() {
		_coms.Send("stop");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Stop returned error: {ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}


	/// <summary>Add a song to the play queue.</summary>
	/// <param name="uri">File location of the song or directory.</param>
	/// <param name="position">Position to add song to, if null add to end.</param>
	/// <returns>True if successful.</returns>
	public bool QueueAdd(string uri, int? position) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"add \"{uri}\"{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Add {uri}{pos} returned error: {ErrorMessage}", uri, pos, response.ErrorMessage);
			return false;
		}
		return true;
	}

	/// <summary>Add a song to the play queue.</summary>
	/// <param name="uri">File location of the song or directory.</param>
	/// <param name="positionRelative">Relative position to add song to.</param>
	/// <param name="addAfter">Add after relative position.</param>
	/// <returns>True if successful.</returns>
	public bool QueueAddRelative(string uri, uint positionRelative, bool addAfter = true) {
		var pos = addAfter ? $"+{positionRelative}" : $"-{positionRelative}";
		_coms.Send($"add \"{uri}\" {pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Add {uri}{pos} returned error: {ErrorMessage}", uri, pos, response.ErrorMessage);
			return false;
		}
		return true;
	}

	public int QueueAddId(string uri, int? position) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"addid \"{uri}\"{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Add {uri}{pos} returned error: {ErrorMessage}", uri, pos, response.ErrorMessage);
			return -1;
		}
		var songId = new AddIdModel(response.Binary);
		return songId.Id;
	}

	public int QueueAddIdRelative(string uri, uint positionRelative, bool addAfter = true) {
		var pos = addAfter ? '+' : '-' + positionRelative;
		_coms.Send($"addid \"{uri}\" {pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Add {uri}{pos} returned error: {ErrorMessage}", uri, pos, response.ErrorMessage);
			return -1;
		}
		var songId = new AddIdModel(response.Binary);
		return songId.Id;
	}

	public bool QueueClear() {
		_coms.Send("clear");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Clear returned error: {ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}

	public bool QueueDelete(int position) {
		_coms.Send($"delete {position}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Delete {position} returned error: {ErrorMessage}", position, response.ErrorMessage);
			return false;
		}
		return true;
	}

	public bool QueueDelete(int from, int toNotIncluding) {
		_coms.Send($"delete {from}:{toNotIncluding}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Delete {from}:{toNotIncluding} returned error: {ErrorMessage}", from, toNotIncluding, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool QueueDeleteId(int songId) {
		_coms.Send($"deleteid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Deleteid {songId} returned error: {ErrorMessage}", songId, response.ErrorMessage);
			return false;
		}
		return true;
	}

	//
	// move is next to add
	//

	public bool SetConsume(bool state = true) {
		var stateStr = state ? "1" : "0";
		_coms.Send($"consume {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Consume {stateStr} returned error: {ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetCrossfade(int fade = 0) {
		_coms.Send($"crossfade {fade}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Crossfade {fade} returned error: {ErrorMessage}", fade, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetMixRampDb(int decibels = 0) {
		_coms.Send($"mixrampdb {decibels}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Mixrampdb {decibels} returned error: {ErrorMessage}", decibels, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetMixRampDelay(int delay = 0) {
		_coms.Send($"mixrampdelay {delay}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("mixrampdelay {delay} returned error: {ErrorMessage}", delay, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetRandom(bool state = true) {
		var stateStr = state ? "1" : "0";
		_coms.Send($"random {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Random {stateStr} returned error: {ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetRepeat(bool state = true) {
		var stateStr = state ? "1" : "0";
		_coms.Send($"repeat {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Repeat {stateStr} returned error: {ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetVolume(int vol = 0) {
		_coms.Send($"setvol {vol}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Setvol {vol} returned error: {ErrorMessage}", vol, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public int GetVolume() {
		_coms.Send($"getvol");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Getvol returned error: {ErrorMessage}", response.ErrorMessage);
			return -1;
		}
		var volume = new VolumeModel(response.Binary);
		return volume.Volume;
	}
	public bool SetSingle(bool state = true) {
		var stateStr = state ? "1" : "0";
		_coms.Send($"single {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Single {stateStr} returned error: {ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}

	public enum ReplayGainMode {
		Off, Track, Album, Auto
	}

	public bool SetReplayGainMode(ReplayGainMode state) {
		var stateStr = state switch {
			ReplayGainMode.Off => "off",
			ReplayGainMode.Track => "track",
			ReplayGainMode.Album => "album",
			ReplayGainMode.Auto => "auto",
			_ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
		};
		_coms.Send($"replay_gain_mode {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Replay_gain_mode {stateStr} returned error: {ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}

	public string GetReplayGainStatus() {
		_coms.Send("replay_gain_status");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Replay_gain_status returned error: {ErrorMessage}", response.ErrorMessage);
			return string.Empty;
		}
		var volume = new ReplayGainStatusModel(response.Binary);
		return volume.ReplayGainStatus;
	}

	public bool AdjustVolume(int change) {
		_coms.Send($"volume {change}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Volume {change} returned error: {ErrorMessage}", change, response.ErrorMessage);
			return false;
		}
		return true;
	}


	public CurrentSongModel? CurrentSong() {
		_coms.Send("currentsong");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("CurrentSong returned error: {ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var currentSong = new CurrentSongModel(response.Binary);
		return currentSong;
	}

	public byte[] AlbumArt(string uri) {
		var response = _coms.SendReceiveBinary($"albumart \"{uri}\"", 2);
		if (response.IsError) {
			_logger.LogError("Albumart \"{uri}\" 0 returned error: {ErrorMessage}", uri, response.ErrorMessage);
			return [];
		}
		return response.Binary;
	}

	public byte[] ReadPicture(string uri) {
		var response = _coms.SendReceiveBinary($"readpicture \"{uri}\"", 3);
		if (response.IsError) {
			_logger.LogError("ReadPicture \"{uri}\" 0 returned error: {ErrorMessage}", uri, response.ErrorMessage);
			return [];
		}
		return response.Binary;
	}



	public PlayListModel? PlayList() {
		_coms.Send("playlist");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("PlayList returned error: {ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var playListModel = new PlayListModel(response.Binary);
		return playListModel;
	}

	public CurrentSongModel? PlayListId(int songId) {
		_coms.Send($"playlistid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("PlayListId returned error: {ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var currentSongModel = new CurrentSongModel(response.Binary);
		return currentSongModel;
	}

	public PlayListInfoModel? PlayListInfo(int? songId = null) {
		_coms.Send(songId != null ? $"playlistinfo {songId}" : $"playlistinfo");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("PlayListId returned error: {ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var playListInfoModel = new PlayListInfoModel(response.Binary);
		return playListInfoModel;
	}

	public PlayListInfoModel? PlayListInfo(int startSongId, int endSongId) {
		_coms.Send($"playlistinfo {startSongId}:{endSongId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("PlayListId returned error: {ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var playListInfoModel = new PlayListInfoModel(response.Binary);
		return playListInfoModel;
	}

	public ListInfoModel? LsInfo(string uri) {
		_coms.Send($"lsinfo \"{uri}\"");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("LsInfo {uri} returned error: {ErrorMessage}", uri, response.ErrorMessage);
			return null;
		}
		var listInfoModel = new ListInfoModel(response.Binary);
		return listInfoModel;
	}


	public bool QueueShuffle(int? startSongPos = null, int? endSongPos = null) {
		var range = string.Empty;
		if (startSongPos != null && endSongPos != null) {
			range = $" {startSongPos}:{endSongPos}";
		}
		_coms.Send($"shuffle{range}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("QueueShuffle{range} returned error: {ErrorMessage}", range, response.ErrorMessage);
			return false;
		}
		return true;
	}

	public void Idle(SubSystem subsystems) {
		_coms.Send("idle");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("idle{subsystem} returned error: {ErrorMessage}", subsystems, response.ErrorMessage);
			return;
		}
		return;
	}

	[Flags]
	public enum SubSystem {

	}



	public StatusModel? Status() {
		_coms.Send("status");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogError("Status returned error: {ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var status = new StatusModel(response.Binary);
		return status;
	}
}
