using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MpdSharp;
using MpdSharp.Models;


namespace GuiMpc;


public class MpdWrapper {
	private readonly Mpd _mpd;
	private readonly ILogger<MpdWrapper> _logger;
	private StatusModel _status;
	private CurrentSongModel? _currentSong;
	private string[] _playList;

	public MpdWrapper(Mpd mpd, ILogger<MpdWrapper> logger) {
		_mpd = mpd;
		_logger = logger;
	}

	public void Connect() {
		_mpd.Connect();
		_status = _mpd.Status();
		_currentSong = _mpd.CurrentSong();
		GetPlayList();

	}

	public void Close() {
		_mpd.Close();
	}


	public void Play() {
		_mpd.Play();
		_status.PlayState = PlayState.Play;
	}
	public void Stop() {
		_mpd.Stop();
		_status.PlayState = PlayState.Stop;
	}
	public void Pause() {
		_mpd.Pause();
		_status.PlayState = PlayState.Pause;
	}
	public void Next() {
		_mpd.Next();
	}
	public void Prev() {
		_mpd.Previous();
	}

	public PlayState GetPlayButtonState() {
		return _status.PlayState;
	}

	public double GetVol() {
		var vol = _mpd.GetVolume();
		return vol;
	}
	public void SetVol(int vol) {
		_mpd.SetVolume(vol);
	}

	private static int VolumeSliderToMpd(double sliderVal) {
		var mpdVol = (int)Math.Round(Math.Sqrt(sliderVal) * 10.0);
		return mpdVol;
	}
	private static double VolumeMpdToSlider(int mpdVal) {
		var sliderVol = mpdVal * mpdVal / 100.0;
		return sliderVol;
	}


	public double GetSongPos() {
		var pos = _status.Elapsed * 100.0 / _status.Duration;
		return pos;
	}
	public void SetSongPos(double newPos) {
		var oldSliderPos = _status.Elapsed * 100.0 / _status.Duration;
		if (oldSliderPos.CompareTo(newPos) == 0) {
			return;
		}
		var newSongPos = newPos / 100.0 * _status.Duration;
		_mpd.SeekCurrent(newSongPos);
	}

	public byte[] GetSongArt() {
		if (_currentSong == null) {
			return [];
		}
		var imageBytes = _mpd.ReadPicture(_currentSong.File);
		if (imageBytes.Length <= 0) {
			imageBytes = _mpd.AlbumArt(_currentSong.File);
			if (imageBytes.Length <= 0) {
				return [];
			}
		}
		return imageBytes;
	}


	public string[] GetPlayList() {
		var mpdPlaylist = _mpd.PlayListInfo();
		if (mpdPlaylist == null) {
			return [];
		}
		_playList = mpdPlaylist
			.Songs
			.Select(CurrentSongFullText)
			.ToArray();
		return _playList;
	}

	public int GetPlayListSongPos(string songFullTitle) {
		var index = Array.IndexOf(_playList, songFullTitle);
		return index;
	}

	public void PlayNewSong(string songFullTitle) {
		var songPos = GetPlayListSongPos(songFullTitle);
		_mpd.Play(songPos);
		UpdateMpdState();
	}

	public IEnumerable<string> GetPlayQueue() {
		var mpdPlaylist = _mpd.PlayListInfo();
		if (mpdPlaylist == null) {
			return [];
		}
		var songs = mpdPlaylist.Songs.Select(CurrentSongFullText);
		return songs;
	}

	public string? GetCurrentSongFullText() {
		if (_currentSong != null) {
			return CurrentSongFullText(_currentSong);
		}
		return null;
	}



	public void UpdateMpdState() {
		_status = _mpd.Status();
		_currentSong = _mpd.CurrentSong();
	}




	public IEnumerable<FileModel> FileBrowser(string currentUri) {
		var lsInfo = _mpd.LsInfo(currentUri);
		if (lsInfo == null) {
			return [];
		}
		IEnumerable<FileModel> items;
		if (string.IsNullOrEmpty(currentUri)) {
			items = [];
		} else {
			items = [
				new FileModel {
					Name = "..",
					Path = ".."
				}
			];
		}
		var listItemsDirs = lsInfo.Directories.Select(item => new FileModel {
			Name = RemovePartFromPath(currentUri, item.Directory),
			Path = item.Directory
		});
		var listItemsFiles = lsInfo.Files.Select(item => new FileModel {
			Name = RemovePartFromPath(currentUri, item.File),
			Path = item.Title
		});
		items = items.Concat(listItemsDirs.Concat(listItemsFiles));
		return items;
	}


	private string RemovePartFromPath(string removeStart, string path) {
		if (removeStart.Length <= 0) {
			_logger.LogDebug("RemovePartFromPath: {path}", path);
			return path;
		}
		if (path.StartsWith(removeStart)) {
			var pathBit = path[removeStart.Length..]
				.Trim('/');
			_logger.LogDebug("RemovePartFromPath: {path}", pathBit);
			return pathBit;
		}
		if (removeStart.First() == '/' && path.StartsWith(removeStart[1..])) {
			var pathBit = path[(removeStart.Length - 1)..]
				.Trim('/');
			_logger.LogDebug("RemovePartFromPath: {path}", pathBit);
			return pathBit;
		}
		_logger.LogDebug("RemovePartFromPath: {path}", path);
		return path;
	}

	public IEnumerable<string> PlayQueue() {
		var mpdPlaylist = _mpd.PlayListInfo();
		if (mpdPlaylist == null) {
			return [];
		}
		var songs = mpdPlaylist.Songs.Select(CurrentSongFullText);
		return songs;
	}



	private static string CurrentSongFullText(CurrentSongModel currentSong) {
		var bob = new List<string>();
		if (!string.IsNullOrEmpty(currentSong.Artist)) {
			bob.Add(currentSong.Artist);
		}
		if (!string.IsNullOrEmpty(currentSong.Album)) {
			bob.Add(currentSong.Album);
		}
		if (!string.IsNullOrEmpty(currentSong.Title)) {
			bob.Add(currentSong.Title);
		}
		var fullTitle = string.Join(" - ", bob);
		return fullTitle;
	}
}
