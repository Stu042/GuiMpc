

namespace MpdSharp.Models;



public class BinaryModel  {
	public bool IsError { get; init; }
	public string ResponseMessage { get; init; }
	public byte[] Bytes { get; init; }
	public BinaryModel(byte[] bytes, string responseMessage, bool isError) {
		IsError = isError;
		Bytes = bytes;
		ResponseMessage = responseMessage;
	}
}
