using System.Text;


namespace MpdSharp;


public static class ResponseHelper {
	public static CrazyDict ResponseToDictionary(string response, int? lineCount = null) {
		var cd = new CrazyDict();
		if (string.IsNullOrEmpty(response)) {
			return cd;
		}
		var lines = response
			.Split('\n')
			.Select(x => x.Split(':', 2));
		foreach (var line in lines) {
			if (lineCount-- <= 0) {
				break;
			}
			var key = line[0]
				.ToLowerInvariant();
			cd.Add(key, line[1]
				.Trim());
		}
		return cd;
	}

	public static CrazyDict ResponseToDictionary(byte[] response, int? lineCount = null) {
		var str = Encoding.UTF8.GetString(response);
		var cd = ResponseToDictionary(str, lineCount);
		return cd;
	}
}



public class CrazyDict {
	private Dictionary<string, List<string>> _dict;

	public CrazyDict() {
		_dict = new Dictionary<string, List<string>>();
	}

	public void Add(string key, string value) {
		if (_dict.TryGetValue(key, out var curValue)) {
			curValue.Add(value.Trim());
		} else {
			_dict.Add(key, [value.Trim()]);
		}
	}
	public List<string> Values(string key) {
		return _dict.TryGetValue(key, out var values) ? values : [];
	}


	public string Value(string key) {
		return _dict.TryGetValue(key, out var val) ? val.First() : string.Empty;
	}

	public int? IntVal(string key) {
		var value = Value(key);
		if (int.TryParse(value, out var result)) {
			return result;
		}
		return null;
	}

	public double? DoubleVal(string key) {
		var value = Value(key);
		if (double.TryParse(value, out var result)) {
			return result;
		}
		return null;
	}

	public bool? BoolVal(string key) {
		var value = Value(key);
		if (bool.TryParse(value, out var result)) {
			return result;
		}
		if (int.TryParse(value, out var resultInt)) {
			return resultInt != 0;
		}
		return null;
	}
}
