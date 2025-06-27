using System.Text;


namespace MpdSharp.Models;


public class PlayListModel {
	public List<PlayListItemModel> Items { get; set; }

	public PlayListModel(byte[] response) {
		Items = [];
		var text = Encoding.UTF8.GetString(response);
		var lines = text.Split('\n');
		foreach (var line in lines) {
			var split = line.Split(':');
			var index = int.Parse(split[0]);
			var songFileName = split[2];
			var item = new PlayListItemModel(index, songFileName);
			Items.Add(item);
		}
	}
}

public class PlayListItemModel {
	public int SongIndex { get; set; }
	public string SongFileName { get; set; }
	public PlayListItemModel(int songIndex, string songFileName) {
		SongIndex = songIndex;
		SongFileName = songFileName;
	}
}
