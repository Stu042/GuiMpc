using System.Net.Sockets;
using System.Text;
using MpdSharp.Models;


namespace MpdSharp;


internal class Coms {
	private TcpClient _socket;
	private int _portNo;
	private string _serverIp;
	private string? _version;
	private const string DefaultServerHostName = "127.0.0.1";
	private const int DefaultServerPort = 6600;
	private const int BufferSize = 4096;
	public Coms() {
		_socket = new TcpClient();
	}

	internal bool Connect(string? serverIp, int? portNo) {
		_serverIp = serverIp ?? DefaultServerHostName;
		_portNo = portNo ??  DefaultServerPort;
		_socket = new TcpClient(_serverIp, _portNo);
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
	private void ReConnect() {
		if (_socket.Connected) {
			return;
		}
		_socket.Connect(_serverIp, _portNo);
	}

	internal void Close() {
		_socket.Close();
	}


	internal void Send(string message) {
		ReConnect();
		var messageBytes = Encoding.UTF8.GetBytes(message + "\n");
		try {
			_ = _socket.Client.Send(messageBytes, SocketFlags.None);
		} catch (Exception e) {
			Console.WriteLine(e);
		}
		Console.WriteLine($"Socket client sent: {message}");
	}

	/// <summary>Retrieves a response, checks for OK at the end and returns the raw bytes.</summary>
	internal BinaryResponseModel ReceiveRaw() {
		var bob = new List<byte>();
		try {
			var buffer = new byte[BufferSize];
			int bytesRead;
			do {
				bytesRead = _socket.Client.Receive(buffer, SocketFlags.None);
				bob.AddRange(buffer[..bytesRead]);
			} while (bytesRead == BufferSize);
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


	private class BinaryPartialResponseModel {
		public required BinaryResponseModel BinaryResponse { get; init; }
		public int TotalSize { get; init; }
		public int ChunkSize { get; set; }
	}

	internal BinaryResponseModel SendReceiveBinary(string request, int textLineCount, int curChunkStart = 0) {
		Send($"{request} {curChunkStart}");
		var bytes = new List<byte>();
		var chunk = ReceiveBinaryChunk(textLineCount);
		curChunkStart += chunk.BinaryResponse.Binary.Length;
		if (chunk.BinaryResponse.IsError || curChunkStart >= chunk.TotalSize) {
			return chunk.BinaryResponse;
		}
		bytes.AddRange(chunk.BinaryResponse.Binary);
		while (!chunk.BinaryResponse.IsError && curChunkStart < chunk.TotalSize) {
			Send($"{request} {curChunkStart}");
			chunk = ReceiveBinaryChunk(textLineCount);
			bytes.AddRange(chunk.BinaryResponse.Binary);
			curChunkStart += chunk.BinaryResponse.Binary.Length;
		}
		chunk.BinaryResponse.Binary = bytes.ToArray();
		return chunk.BinaryResponse;
	}
	private BinaryPartialResponseModel ReceiveBinaryChunk(int textLineCount) {
		var initial = ReceiveRaw();
		if (initial.IsError) {
			return new BinaryPartialResponseModel {
				BinaryResponse = initial,
				TotalSize = 0,
				ChunkSize = 0
			};
		}
		var likelyTextEnd = int.Min(initial.Binary.Length, 100);
		var headerDict = ResponseHelper.ResponseToDictionary(initial.Binary[..likelyTextEnd], textLineCount);
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
	private static BinaryResponseModel FormatResponse(byte[] response) {
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
		var index = ErrorStartIndex(response);
		if (index >= 0) {// Found error message, return error along with text error message
			var errorMessBytes = response[index..^1];
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

	private static int ErrorStartIndex(byte[] response) {
		var errorResponse = "ACK "u8.ToArray();
		var index = 0;
		for(var i = 0; i < response.Length; i++) {
			if (response[i] == errorResponse[index]) {
				var startI = i;
				do {
					index++;
					i++;
				} while (index < errorResponse.Length && response[i] == errorResponse[index]);
				if (index >= errorResponse.Length) {
					return i;
				}
				i = startI;
				index = 0;
			}
		}
		return -1;
	}
}


