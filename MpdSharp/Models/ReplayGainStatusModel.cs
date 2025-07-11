namespace MpdSharp.Models;


public class ReplayGainStatusModel {
	public string ReplayGainStatus { get; set; }

	public ReplayGainStatusModel(byte[] response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		ReplayGainStatus = parsedResponse.Value("replay_gain_status");
	}
}
