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
			_logger.LogDebug("Next returned error: {response.ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool Pause() {
		_coms.Send("pause");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Pause returned error: {response.ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool Play(int? position = null) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"play{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Play{pos} returned error: {response.ErrorMessage}", pos, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool PlayId(int songId) {
		_coms.Send($"playid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Playid {songId} returned error: {response.ErrorMessage}", songId, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool Previous() {
		_coms.Send("previous");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Previous returned error: {response.ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}

	public bool Seek(int songPos, double time) {
		_coms.Send($"seek {songPos} {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Seek {songPos} {time} returned error: {response.ErrorMessage}", songPos, time, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SeekId(int songId, double time) {
		_coms.Send($"seekid {songId} {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Seekid {songId} {time} returned error: {response.ErrorMessage}", songId, time, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SeekCurrent(double time) {
		_coms.Send($"seekcur {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Seekcur {time} returned error: {response.ErrorMessage}", time, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool Stop() {
		_coms.Send("stop");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Stop returned error: {response.ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}


	public bool QueueAdd(string uri, int? position) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"add {uri}{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Add {uri}{pos} returned error: {response.ErrorMessage}", uri, pos, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool QueueAddRelative(string uri, uint positionRelative, bool addAfter = true) {
		var pos = addAfter ? '+' : '-' + positionRelative;
		_coms.Send($"add {uri} {pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Add {uri}{pos} returned error: {response.ErrorMessage}", uri, pos, response.ErrorMessage);
			return false;
		}
		return true;
	}

	public int QueueAddId(string uri, int? position) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"addid {uri}{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Add {uri}{pos} returned error: {response.ErrorMessage}", uri, pos, response.ErrorMessage);
			return -1;
		}
		var songId = new AddIdModel(response.Binary);
		return songId.Id;
	}

	public int QueueAddIdRelative(string uri, uint positionRelative, bool addAfter = true) {
		var pos = addAfter ? '+' : '-' + positionRelative;
		_coms.Send($"addid {uri} {pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Add {uri}{pos} returned error: {response.ErrorMessage}", uri, pos, response.ErrorMessage);
			return -1;
		}
		var songId = new AddIdModel(response.Binary);
		return songId.Id;
	}

	public bool QueueClear() {
		_coms.Send("clear");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Clear returned error: {response.ErrorMessage}", response.ErrorMessage);
			return false;
		}
		return true;
	}

	public bool QueueDelete(int position) {
		_coms.Send($"delete {position}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Delete {position} returned error: {response.ErrorMessage}", position, response.ErrorMessage);
			return false;
		}
		return true;
	}

	public bool QueueDelete(int from, int toNotIncluding) {
		_coms.Send($"delete {from}:{toNotIncluding}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Delete {from}:{toNotIncluding} returned error: {response.ErrorMessage}", from, toNotIncluding, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool QueueDeleteId(int songId) {
		_coms.Send($"deleteid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Deleteid {songId} returned error: {response.ErrorMessage}", songId, response.ErrorMessage);
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
			_logger.LogDebug("Consume {stateStr} returned error: {response.ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetCrossfade(int fade = 0) {
		_coms.Send($"crossfade {fade}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Crossfade {fade} returned error: {response.ErrorMessage}", fade, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetMixRampDb(int decibels = 0) {
		_coms.Send($"mixrampdb {decibels}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Mixrampdb {decibels} returned error: {response.ErrorMessage}", decibels, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetMixRampDelay(int delay = 0) {
		_coms.Send($"mixrampdelay {delay}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("mixrampdelay {delay} returned error: {response.ErrorMessage}", delay, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetRandom(bool state = true) {
		var stateStr = state ? "1" : "0";
		_coms.Send($"random {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Random {stateStr} returned error: {response.ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetRepeat(bool state = true) {
		var stateStr = state ? "1" : "0";
		_coms.Send($"repeat {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Repeat {stateStr} returned error: {response.ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public bool SetVolume(int vol = 0) {
		_coms.Send($"setvol {vol}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Setvol {vol} returned error: {response.ErrorMessage}", vol, response.ErrorMessage);
			return false;
		}
		return true;
	}
	public int GetVolume() {
		_coms.Send($"getvol");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Getvol returned error: {response.ErrorMessage}", response.ErrorMessage);
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
			_logger.LogDebug("Single {stateStr} returned error: {response.ErrorMessage}", stateStr, response.ErrorMessage);
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
			_logger.LogDebug("Replay_gain_mode {stateStr} returned error: {response.ErrorMessage}", stateStr, response.ErrorMessage);
			return false;
		}
		return true;
	}

	public string GetReplayGainStatus() {
		_coms.Send("replay_gain_status");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Replay_gain_status returned error: {response.ErrorMessage}", response.ErrorMessage);
			return string.Empty;
		}
		var volume = new ReplayGainStatusModel(response.Binary);
		return volume.ReplayGainStatus;
	}

	public bool AdjustVolume(int change) {
		_coms.Send($"volume {change}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Volume {change} returned error: {response.ErrorMessage}", change, response.ErrorMessage);
			return false;
		}
		return true;
	}


	public CurrentSongModel? CurrentSong() {
		_coms.Send("currentsong");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("CurrentSong returned error: {response.ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var currentSong = new CurrentSongModel(response.Binary);
		return currentSong;
	}

	public byte[] AlbumArt(string uri) {
		var response = _coms.SendReceiveBinary($"albumart \"{uri}\"", 2);
		if (response.IsError) {
			_logger.LogDebug("Albumart \"{uri}\" 0 returned error: {response.ErrorMessage}", response.ErrorMessage);
			return [];
		}
		return response.Binary;
	}

	public byte[] ReadPicture(string uri) {
		var response = _coms.SendReceiveBinary($"readpicture \"{uri}\"", 3);
		if (response.IsError) {
			_logger.LogDebug("ReadPicture \"{uri}\" 0 returned error: {response.ErrorMessage}", response.ErrorMessage);
			return [];
		}
		return response.Binary;
	}



	public PlayListModel? PlayList() {
		_coms.Send("playlist");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("PlayList returned error: {response.ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var playListModel = new PlayListModel(response.Binary);
		return playListModel;
	}

	public CurrentSongModel? PlayListId(int songId) {
		_coms.Send($"playlistid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("PlayListId returned error: {response.ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var currentSongModel = new CurrentSongModel(response.Binary);
		return currentSongModel;
	}

	public PlayListInfoModel? PlayListInfo(int? songId = null) {
		_coms.Send(songId != null ? $"playlistinfo {songId}" : $"playlistinfo");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("PlayListId returned error: {response.ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var playListInfoModel = new PlayListInfoModel(response.Binary);
		return playListInfoModel;
	}

	public PlayListInfoModel? PlayListInfo(int startSongId, int endSongId) {
		_coms.Send($"playlistinfo {startSongId}:{endSongId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("PlayListId returned error: {response.ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var playListInfoModel = new PlayListInfoModel(response.Binary);
		return playListInfoModel;
	}

	public ListInfoModel? LsInfo(string uri) {
		_coms.Send($"lsinfo \"{uri}\"");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("LsInfo {uri} returned error: {ErrorMessage}", uri, response.ErrorMessage);
			return null;
		}
		var listInfoModel = new ListInfoModel(response.Binary);
		return listInfoModel;
	}






	public StatusModel Status() {
		_coms.Send("status");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			_logger.LogDebug("Status returned error: {response.ErrorMessage}", response.ErrorMessage);
			return null;
		}
		var status = new StatusModel(response.Binary);
		return status;
	}
}
