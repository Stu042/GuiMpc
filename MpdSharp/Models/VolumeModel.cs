namespace MpdSharp.Models;


public class VolumeModel {
	public int Volume { get; }
	public VolumeModel(string response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Volume = parsedResponse.IntVal("volume") ?? -1;
	}
	public VolumeModel(byte[] response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Volume = parsedResponse.IntVal("volume") ?? -1;	}
}
