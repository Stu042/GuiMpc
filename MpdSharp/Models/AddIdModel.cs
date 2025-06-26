namespace MpdSharp.Models;


public class AddIdModel {
	public int Id { get; }
	public AddIdModel(string response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Id = parsedResponse.IntVal("id") ?? -1;
	}
	public AddIdModel(byte[] response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Id = parsedResponse.IntVal("id") ?? -1;
	}
}
