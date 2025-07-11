namespace MpdSharp.Models;


public class CurrentSongModel {
	public string File { get; set; }          // Organised/Pink Floyd/Discovery/1209-pink_floyd-young_lust.flac
	public DateTime LastModified { get; set; }// 2025-05-29T21:17:37Z
	public DateTime Added { get; set; }       // 2025-05-29T21:17:37Z
	public FormatModel Format { get; set; }   // 44100:16:2
	public string? Title { get; set; }                 // Young Lust
	public string? Artist { get; set; }                // Discovery Artist: Pink Floyd
	public string? Album { get; set; }                 // Discovery Artist: The Wall
	public string? Date { get; set; }                  // 2011
	public string? Genre { get; set; }                 // Symphonic Rock
	public string? Track { get; set; }                 // 6
	public string? Time { get; set; }                  // 210
	public double Duration { get; set; }               // 209.973
	public string? Disc { get; set; }                  // 9
	public int Pos { get; set; }                       // 289
	public int Id { get; set; }                        // 290

	public CurrentSongModel(byte[] response) {
		File = string.Empty;
		Format = new FormatModel();
		var parsedResponse = ResponseHelper.ResponseToDictionary(response);
		Parse(parsedResponse);
	}
	public CurrentSongModel(StreamReader reader) {
		File = string.Empty;
		LastModified = DateTime.MinValue;
		Added = DateTime.MinValue;
		Format = new FormatModel();
		while (reader.ReadLine() is { } line) {
			var parts = line.Split(':', 2);
			switch (parts[0]
						.ToLowerInvariant()
						.Trim()) {
				case "file":
					File = parts[1].Trim();
					break;
				case "lastmodified":
					LastModified = DateTime.Parse(parts[1]);
					break;
				case "added":
					Added = DateTime.Parse(parts[1]);
					break;
				case "format":
					Format = new FormatModel(parts[1]);
					break;
				case "artist":
					Artist = parts[1].Trim();
					break;
				case "album":
					Album = parts[1].Trim();
					break;
				case "title":
					Title = parts[1].Trim();
					break;
				case "genre":
					Genre = parts[1].Trim();
					break;
				case "date":
					Date = parts[1].Trim();
					break;
				case "track":
					Track = parts[1].Trim();
					break;
				case "disc":
					Disc = parts[1].Trim();
					break;
				case "time":
					Time = parts[1].Trim();
					break;
				case "duration":
					Duration = double.Parse(parts[1]);
					break;
				case "pos":
					Pos = int.Parse(parts[1]);
					break;
				case "id":
					Id = int.Parse(parts[1]);
					return;
			}
		}
	}

	private void Parse(CrazyDict crazyDict) {
		File = crazyDict.Value("file");
		if (File == string.Empty) {
			File = "Error";
			Format = new FormatModel();
			return;
		}
		if (DateTime.TryParse(crazyDict.Value("last-modified"), out var lastModified)) {
			LastModified = lastModified;
		}
		if (DateTime.TryParse(crazyDict.Value("added"), out var added)) {
			Added = added;
		}
		Format = new FormatModel(crazyDict.Value("format"));
		Artist = crazyDict.Value("artist");
		Album = crazyDict.Value("album");
		Title = crazyDict.Value("title");
		Genre = crazyDict.Value("genre");
		Date = crazyDict.Value("date");
		Track = crazyDict.Value("track");
		Disc = crazyDict.Value("disc");
		Time = crazyDict.Value("time");
		Duration = crazyDict.DoubleVal("duration") ?? 0;
		Pos = crazyDict.IntVal("pos") ?? 0;
		Id = crazyDict.IntVal("id") ?? -1;
	}
}
