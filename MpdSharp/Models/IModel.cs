namespace MpdSharp.Models;


public interface IModel {
	public void Init(List<byte> response);
	public void Init(List<byte[]> responses);
}
