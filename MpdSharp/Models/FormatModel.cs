namespace MpdSharp.Models;


public class FormatModel {
	public int SampleRate { get; set; }
	public int Bits { get; set; }
	public int Channels { get; set; }
	public FormatModel(string response = "0:0:0") {
		var split = response.Split(':');
		if (int.TryParse(split[0], out var sampleRate)) SampleRate = sampleRate;
		if (int.TryParse(split[1], out var bits)) Bits = bits;
		if (int.TryParse(split[2], out var channels)) Channels = channels;
	}
	public override string ToString() {
		return $"{SampleRate}:{Bits}:{Channels}";
	}
}
