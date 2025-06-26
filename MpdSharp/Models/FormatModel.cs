namespace MpdSharp.Models;


public class FormatModel : MustInitialize<string> {
	public int SampleRate { get; set; }
	public int Bits { get; set; }
	public int Channels { get; set; }
	public FormatModel(string response) : base(response) {
		var split = response.Split(':');
		if (int.TryParse(split[0], out var sampleRate)) SampleRate = sampleRate;
		if (int.TryParse(split[0], out var bits)) Bits = bits;
		if (int.TryParse(split[0], out var channels)) Channels = channels;
	}
	public override string ToString() {
		return $"{SampleRate}:{Bits}:{Channels}";
	}
}
