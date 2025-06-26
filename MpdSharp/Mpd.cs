using System.Net.Sockets;
using System.Text;
using MpdSharp.Models;


namespace MpdSharp;


public class Mpd {
	private readonly Coms _coms;

	public Mpd(string serverIp = "127.0.0.1", int portNo = 6600) {
		_coms = new Coms(serverIp, portNo);
	}

	public bool Connect() {
		return _coms.Connect();
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

		var response = _coms.SendReceiveBinary($"albumart \"{uri}\"");
		if (response.IsError) {
			Console.WriteLine($"Albumart \"{uri}\" 0 returned error: {response.ErrorMessage}");
			return [];
		}
		return response.Binary;
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


internal class Coms {
	private readonly TcpClient _socket;
	private readonly int _portNo;
	private readonly string _serverIp;
	private string? _version;
	public Coms(string serverIp, int portNo) {
		_serverIp = serverIp;
		_portNo = portNo;
		_socket = new TcpClient(_serverIp, _portNo);
	}

	internal bool Connect() {
		if (!_socket.Connected) {
			Console.WriteLine("Connection failed");
			return false;
		}
		var okResponseBytes = "OK MPD"u8.ToArray();
		var buffer = new byte[1024];
		_ = _socket.Client.Receive(buffer, SocketFlags.None);
		var isOkay = buffer[^okResponseBytes.Length..]
			.SequenceEqual(okResponseBytes);
		var response = Encoding.UTF8.GetString(buffer);
		if (!isOkay) {
			Console.WriteLine($"Connection failed, invalid response, {response}");
			return false;
		}
		_version = response[7..^1];//"OK MPD 0.24.0"
		Console.WriteLine($"Connected to MPD version {_version}");
		return true;
	}
	public void Close() {
		_socket.Close();
	}


	internal void Send(string message) {
		if (!_socket.Connected) {
			_socket.Connect(_serverIp, _portNo);
		}
		var messageBytes = Encoding.UTF8.GetBytes(message + "\n");
		try {
			_ = _socket.Client.Send(messageBytes, SocketFlags.None);
		} catch (Exception e) {
			Console.WriteLine(e);
		}
		Console.WriteLine($"Socket client sent: {message}");
	}

	internal BinaryResponseModel ReceiveRaw() {
		const int bufferSize = 4096;
		var bob = new List<byte>();
		try {
			var buffer = new byte[bufferSize];
			int bytesRead;
			do {
				bytesRead = _socket.Client.Receive(buffer, SocketFlags.None);
				bob.AddRange(buffer[..bytesRead]);
			} while (bytesRead == bufferSize);
		} catch (Exception e) {
			Console.WriteLine(e);
			return new BinaryResponseModel {// response was not okay but unable to find error message, return unknown error
				IsError = true,
				Binary = bob.ToArray(),
				FooterSize = 0,
				ErrorMessage = e.Message
			};
		}
		var response = FormatResponse(bob.ToArray());
		return response;
	}


	internal class BinaryPartialResponseModel {
		public required BinaryResponseModel BinaryResponse { get; init; }
		public int TotalSize { get; init; }
		public int ChunkSize { get; set; }
	}

	internal BinaryResponseModel SendReceiveBinary(string request, int curChunkStart = 0) {
		Send($"{request} {curChunkStart}");
		var bytes = new List<byte>();
		var chunk = FormatBinaryChunk();
		curChunkStart += chunk.BinaryResponse.Binary.Length;
		if (chunk.BinaryResponse.IsError || curChunkStart >= chunk.TotalSize) {
			return chunk.BinaryResponse;
		}
		bytes.AddRange(chunk.BinaryResponse.Binary);
		while (!chunk.BinaryResponse.IsError && curChunkStart < chunk.TotalSize) {
			Send($"{request} {curChunkStart}");
			chunk = FormatBinaryChunk();
			bytes.AddRange(chunk.BinaryResponse.Binary);
			curChunkStart += chunk.BinaryResponse.Binary.Length;
		}
		chunk.BinaryResponse.Binary = bytes.ToArray();
		return chunk.BinaryResponse;
	}
	private BinaryPartialResponseModel FormatBinaryChunk() {
		var initial = ReceiveRaw();
		if (initial.IsError) {
			return new BinaryPartialResponseModel {
				BinaryResponse = initial,
				TotalSize = 0,
				ChunkSize = 0
			};
		}
		var headerDict = ResponseHelper.ResponseToDictionary(initial.Binary[..100], 2);
		var totalSize = headerDict.IntVal("size");
		var chunkSize = headerDict.IntVal("binary");
		if (totalSize == null || chunkSize == null) {
			return new BinaryPartialResponseModel {
				BinaryResponse = initial,
				TotalSize = 0,
				ChunkSize = 0
			};
		}
		var result = new BinaryPartialResponseModel {
			BinaryResponse = initial,
			TotalSize = totalSize.Value,
			ChunkSize = chunkSize.Value
		};
		var start = initial.Binary.Length - chunkSize.Value;
		result.BinaryResponse.Binary = initial.Binary[start..];
		return result;
	}


	/// <summary>Check for an error, returns error message and length of text at end of response.</summary>
	private BinaryResponseModel FormatResponse(byte[] response) {
		var okResponseBytes = "OK\n"u8.ToArray();
		var isOkay = response[^okResponseBytes.Length..]
			.SequenceEqual(okResponseBytes);
		if (isOkay) {
			var footerSize = int.Min(okResponseBytes.Length + 1, response.Length);
			return new BinaryResponseModel {
				IsError = false,
				Binary = response[..^footerSize],
				FooterSize = footerSize,
				ErrorMessage = "OK"
			};
		}
		var errorResponse = "ACK "u8.ToArray();
		var index = Array.IndexOf(response, errorResponse);
		if (index >= 0) {// Found error message, return error along with text error message
			var errorMessBytes = response[(index + errorResponse.Length)..^1];
			return new BinaryResponseModel {
				IsError = true,
				Binary = [],
				FooterSize = errorMessBytes.Length + errorResponse.Length + 2,
				ErrorMessage = Encoding.UTF8.GetString(errorMessBytes),
			};
		}
		return new BinaryResponseModel {// response was not okay but unable to find error message, return unknown error
			IsError = true,
			Binary = [],
			FooterSize = 0,
			ErrorMessage = "Unknown Error"
		};
	}
}

internal class BinaryResponseModel {
	public bool IsError { get; init; }
	public int FooterSize { get; init; }
	public required string ErrorMessage { get; init; }
	public required byte[] Binary { get; set; }
}
