namespace GuiMpc;


public class FileModel {
	public required string Name { get; set; }
	public required string Path { get; set; }
	public override string ToString() {
		return Name;
	}
}
