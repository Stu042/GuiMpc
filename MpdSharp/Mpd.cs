using MpdSharp.Models;


namespace MpdSharp;


public class Mpd {
	private readonly Coms _coms;


	public Mpd() {
		_coms = new Coms();
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
			Console.WriteLine($"Next returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool Pause() {
		_coms.Send("pause");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Pause returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool Play(int? position = null) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"play{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Play{pos} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool PlayId(int songId) {
		_coms.Send($"playid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Playid {songId} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool Previous() {
		_coms.Send("previous");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Previous returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}

	public bool Seek(int songPos, double time) {
		_coms.Send($"seek {songPos} {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Seek {songPos} {time} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool SeekId(int songId, double time) {
		_coms.Send($"seekid {songId} {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Seekid {songId} {time} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool SeekCurrent(double time) {
		_coms.Send($"seekcur {time}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Seekcur {time} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool Stop() {
		_coms.Send("stop");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Stop returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}


	public bool QueueAdd(string uri, int? position) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"add {uri}{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Add {uri}{pos} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool QueueAddRelative(string uri, uint positionRelative, bool addAfter = true) {
		var pos = addAfter ? '+' : '-' + positionRelative;
		_coms.Send($"add {uri} {pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Add {uri}{pos} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}

	public int QueueAddId(string uri, int? position) {
		var pos = position != null ? $" {position.Value}" : string.Empty;
		_coms.Send($"addid {uri}{pos}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Add {uri}{pos} returned error: {response.ErrorMessage}");
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
			Console.WriteLine($"Add {uri}{pos} returned error: {response.ErrorMessage}");
			return -1;
		}
		var songId = new AddIdModel(response.Binary);
		return songId.Id;
	}

	public bool QueueClear() {
		_coms.Send("clear");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Clear returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}

	public bool QueueDelete(int position) {
		_coms.Send($"delete {position}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Delete {position} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}

	public bool QueueDelete(int from, int toNotIncluding) {
		_coms.Send($"delete {from}:{toNotIncluding}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Delete {from}:{toNotIncluding} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool QueueDeleteId(int songId) {
		_coms.Send($"deleteid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Deleteid {songId} returned error: {response.ErrorMessage}");
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
			Console.WriteLine($"Consume {stateStr} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool SetCrossfade(int fade = 0) {
		_coms.Send($"crossfade {fade}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Crossfade {fade} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool SetMixRampDb(int decibels = 0) {
		_coms.Send($"mixrampdb {decibels}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Mixrampdb {decibels} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool SetMixRampDelay(int delay = 0) {
		_coms.Send($"mixrampdelay {delay}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"mixrampdelay {delay} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool SetRandom(bool state = true) {
		var stateStr = state ? "1" : "0";
		_coms.Send($"random {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Random {stateStr} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool SetRepeat(bool state = true) {
		var stateStr = state ? "1" : "0";
		_coms.Send($"repeat {stateStr}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Repeat {stateStr} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public bool SetVolume(int vol = 0) {
		_coms.Send($"setvol {vol}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Setvol {vol} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}
	public int GetVolume() {
		_coms.Send($"getvol");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Getvol returned error: {response.ErrorMessage}");
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
			Console.WriteLine($"Single {stateStr} returned error: {response.ErrorMessage}");
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
			Console.WriteLine($"Replay_gain_mode {stateStr} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}

	public string GetReplayGainStatus() {
		_coms.Send("replay_gain_status");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Replay_gain_status returned error: {response.ErrorMessage}");
			return string.Empty;
		}
		var volume = new ReplayGainStatusModel(response.Binary);
		return volume.ReplayGainStatus;
	}

	public bool AdjustVolume(int change) {
		_coms.Send($"volume {change}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Volume {change} returned error: {response.ErrorMessage}");
			return false;
		}
		return true;
	}


	public CurrentSongModel? CurrentSong() {
		_coms.Send("currentsong");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"CurrentSong returned error: {response.ErrorMessage}");
			return null;
		}
		var currentSong = new CurrentSongModel(response.Binary);
		return currentSong;
	}

	public byte[] AlbumArt(string uri) {
		var response = _coms.SendReceiveBinary($"albumart \"{uri}\"", 2);
		if (response.IsError) {
			Console.WriteLine($"Albumart \"{uri}\" 0 returned error: {response.ErrorMessage}");
			return [];
		}
		return response.Binary;
	}

	public byte[] ReadPicture(string uri) {
		var response = _coms.SendReceiveBinary($"readpicture \"{uri}\"", 3);
		if (response.IsError) {
			Console.WriteLine($"ReadPicture \"{uri}\" 0 returned error: {response.ErrorMessage}");
			return [];
		}
		return response.Binary;
	}



	public PlayListModel? PlayList() {
		_coms.Send("playlist");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"PlayList returned error: {response.ErrorMessage}");
			return null;
		}
		var playListModel = new PlayListModel(response.Binary);
		return playListModel;
	}

	public CurrentSongModel? PlayListId(int songId) {
		_coms.Send($"playlistid {songId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"PlayListId returned error: {response.ErrorMessage}");
			return null;
		}
		var currentSongModel = new CurrentSongModel(response.Binary);
		return currentSongModel;
	}

	public PlayListInfoModel? PlayListInfo(int? songId = null) {
		_coms.Send(songId != null ? $"playlistinfo {songId}" : $"playlistinfo");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"PlayListId returned error: {response.ErrorMessage}");
			return null;
		}
		var playListInfoModel = new PlayListInfoModel(response.Binary);
		return playListInfoModel;
	}

	public PlayListInfoModel? PlayListInfo(int startSongId, int endSongId) {
		_coms.Send($"playlistinfo {startSongId}:{endSongId}");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"PlayListId returned error: {response.ErrorMessage}");
			return null;
		}
		var playListInfoModel = new PlayListInfoModel(response.Binary);
		return playListInfoModel;
	}

	public ListInfoModel? LsInfo(string uri) {
		_coms.Send($"lsinfo \"{uri}\"");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"LsInfo {uri} returned error: {response.ErrorMessage}");
			return null;
		}
		var listInfoModel = new ListInfoModel(response.Binary);
		return listInfoModel;
	}






	public StatusModel Status() {
		_coms.Send("status");
		var response = _coms.ReceiveRaw();
		if (response.IsError) {
			Console.WriteLine($"Status returned error: {response.ErrorMessage}");
			return null;
		}
		var status = new StatusModel(response.Binary);
		return status;
	}
}
