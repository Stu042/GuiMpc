namespace MpdSharp.Models;


internal class BinaryResponseModel {
	public bool IsError { get; init; }
	public int FooterSize { get; init; }
	public required string ErrorMessage { get; init; }
	public required byte[] Binary { get; set; }
}
