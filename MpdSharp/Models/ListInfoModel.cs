using System.Text;


namespace MpdSharp.Models;


public class ListInfoModel {
	public List<DirectoryModel> Directories { get; set; }
	public List<FileModel> Files { get; set; }

	public ListInfoModel(byte[] response) {
		Directories = [];
		Files = [];
		var stream = new MemoryStream(response);
		using var reader = new StreamReader(stream);
		var ch = reader.Peek();
		while (!reader.EndOfStream) {
			switch (ch) {
				case 'd':
				case 'D':
					var dir = new DirectoryModel(reader);
					Directories.Add(dir);
					break;
				case 'f':
				case 'F':
					var file = new FileModel(reader);
					Files.Add(file);
					break;
			}
			ch = reader.Peek();
		}
	}

	public class DirectoryModel {
		public string Directory { get; set; }
		public DateTime LastModified { get; set; }
		public DirectoryModel(StreamReader reader) {
			var line = reader.ReadLine();
			while (line != null) {
				var parts = line.Split(':', 2);
				switch (parts[0]
							.ToLowerInvariant()
							.Trim()) {
					case "directory":
						Directory = parts[1]
							.Trim();
						break;
					case "last-modified":
						LastModified = DateTime.Parse(parts[1]);
						return;
				}
				line = reader.ReadLine();
			}
		}
	}

	public class FileModel {
		public string File { get; set; }          // Organised/Ren/Ren - Hi Ren.flac
		public DateTime LastModified { get; set; }// 2025-06-27T22:04:35Z
		public DateTime Added { get; set; }       // 2025-06-27T22:08:39Z
		public FormatModel Format { get; set; }   // 48000:24:2
		public string Title { get; set; }         // Hi Ren
		public string Artist { get; set; }        // Ren
		public string Date { get; set; }             // 2022
		public int Time { get; set; }             // 515
		public double Duration { get; set; }      // 515.000

		public FileModel(StreamReader reader) {
			File = string.Empty;
			Format = new FormatModel();
			Title = string.Empty;
			Artist = string.Empty;
			Date = string.Empty;
			var line = reader.ReadLine();
			while (line != null) {
				var parts = line.Split(':', 2);
				switch (parts[0]
							.ToLowerInvariant()
							.Trim()) {
					case "file":
						File = parts[1]
							.Trim();
						break;
					case "last-modified":
						LastModified = DateTime.Parse(parts[1]);
						break;
					case "added":
						Added = DateTime.Parse(parts[1]);
						break;
					case "format":
						Format = new FormatModel(parts[1]);
						break;
					case "title":
						Title = parts[1]
							.Trim();
						break;
					case "artist":
						Artist = parts[1]
							.Trim();
						break;
					case "date":
						Date = parts[1];
						break;
					case "time":
						Time = int.Parse(parts[1]
							.Trim());
						break;
					case "duration":
						Duration = double.Parse(parts[1]);
						return;
				}
				line = reader.ReadLine();
			}
		}
	}
}
