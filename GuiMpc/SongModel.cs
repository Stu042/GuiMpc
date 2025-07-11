namespace GuiMpc;


public class SongModel {
	public required string FullName;
	public required string File;
	public required int[] Format;
	public string? Title;
	public string? Artist;
	public string? Album;
	public string? Genre;
	public string? Track;
	public string? Time;
	public double Duration;
	public string? Disc;
	public int Pos;
	public int Id;

	public override string ToString() {
		return FullName;
	}
}
