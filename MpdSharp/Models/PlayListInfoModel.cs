namespace MpdSharp.Models;


public class PlayListInfoModel {
	public CurrentSongModel[] Songs { get; set; }
	public PlayListInfoModel(byte[] response) {
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		var currentSongs = new List<CurrentSongModel>();
		var count = parsedResponse.Values("file")
			.Count;
		var stream = new MemoryStream(response);
		using var reader = new StreamReader(stream);
		for (var i = 0; i < count; i++) {
			currentSongs.Add(new CurrentSongModel(reader));
		}
		Songs = currentSongs.ToArray();
	}
}
