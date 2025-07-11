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

	public MpdWrapper(Mpd mpd, ILogger<MpdWrapper> logger) {
		_mpd = mpd;
		_logger = logger;
	}

	public void Connect() {
		_mpd.Connect();
		GetPlayList();

	}

	public void Close() {
		_mpd.Close();
	}


	public void Play() {
		_mpd.Play();
	}
	public void Stop() {
		_mpd.Stop();
	}
	public void Pause() {
		_mpd.Pause();
	}
	public void Next() {
		_mpd.Next();
	}
	public void Prev() {
		_mpd.Previous();
	}

	public PlayState GetPlayButtonState() {
		var status = _mpd.Status();
		return status?.PlayState ?? PlayState.Stop;
	}

	public int GetVol() {
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
		var status = _mpd.Status();
		if (status == null) {
			return 0.0;
		}
		var pos = status.Elapsed * 100.0 / status.Duration;
		return pos;
	}
	public void SetSongPos(double newPos) {
		var status = _mpd.Status();
		if (status == null) {
			return;
		}
		var oldSliderPos = status.Elapsed * 100.0 / status.Duration;
		if (oldSliderPos.CompareTo(newPos) == 0) {
			return;
		}
		var newSongPos = newPos / 100.0 * status.Duration;
		_mpd.SeekCurrent(newSongPos);
	}

	public byte[] GetSongArt() {
		var currentSong = _mpd.CurrentSong();
		if (currentSong == null) {
			return [];
		}
		var imageBytes = _mpd.ReadPicture(currentSong.File);
		if (imageBytes.Length <= 0) {
			imageBytes = _mpd.AlbumArt(currentSong.File);
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
		var playList = mpdPlaylist
			.Songs
			.Select(CurrentSongFullText)
			.ToArray();
		return playList;
	}

	public int GetPlayListSongPos(string songFullTitle) {
		var playList = GetPlayList();
		var index = Array.IndexOf(playList, songFullTitle);
		return index;
	}

	public void PlayNewSong(string songFullTitle) {
		var songPos = GetPlayListSongPos(songFullTitle);
		_mpd.Play(songPos);
	}



	public IEnumerable<SongModel> GetPlayQueue() {
		var mpdPlaylist = _mpd.PlayListInfo();
		if (mpdPlaylist == null) {
			return [];
		}
		var songs = mpdPlaylist.Songs.Select(s => new SongModel {
			FullName = CurrentSongFullText(s),
			File = s.File,
			Format = [s.Format.SampleRate, s.Format.Bits, s.Format.Channels],
			Title = s.Title,
			Artist = s.Artist,
			Album = s.Album,
			Genre = s.Genre,
			Track = s.Track,
			Time = s.Time,
			Duration = s.Duration,
			Disc = s.Disc,
			Pos = s.Pos,
			Id = s.Id
		});
		return songs;
	}

	public SongModel? GetCurrentSong() {
		var currentSong = _mpd.CurrentSong();
		if (currentSong != null) {
			return new SongModel {
				FullName = CurrentSongFullText(currentSong),
				File = currentSong.File,
				Format = [currentSong.Format.SampleRate, currentSong.Format.Bits, currentSong.Format.Channels],
				Title = currentSong.Title,
				Artist = currentSong.Artist,
				Album = currentSong.Album,
				Genre = currentSong.Genre,
				Track = currentSong.Track,
				Time = currentSong.Time,
				Duration = currentSong.Duration,
				Disc = currentSong.Disc,
				Pos = currentSong.Pos,
				Id = currentSong.Id
			};
		}
		return null;
	}

	public string? GetCurrentSongFullText() {
		var currentSong = _mpd.CurrentSong();
		if (currentSong != null) {
			return CurrentSongFullText(currentSong);
		}
		return null;
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
			Path = item.File
		});
		items = items.Concat(listItemsDirs.Concat(listItemsFiles));
		return items;
	}

	public bool IsFile(string currentUri, string filenamePart) {
		var lsInfo = _mpd.LsInfo(currentUri);
		var listItemsDirs = lsInfo?.Directories.FirstOrDefault(item => string.Equals(filenamePart, RemovePartFromPath(currentUri, item.Directory)));
		return listItemsDirs == null;
	}


	private string RemovePartFromPath(string removeStart, string path) {
		if (removeStart.Length <= 0) {
			_logger.LogDebug("RemovePartFromPath({removeStart}, {path}) {path}", removeStart, path, path);
			return path;
		}
		if (path.StartsWith(removeStart)) {
			var pathBit = path[removeStart.Length..]
				.Trim('/');
			_logger.LogDebug("RemovePartFromPath({removeStart}, {path}) {path}", removeStart, path, path);
			return pathBit;
		}
		if (removeStart.First() == '/' && path.StartsWith(removeStart[1..])) {
			var pathBit = path[(removeStart.Length - 1)..]
				.Trim('/');
			_logger.LogDebug("RemovePartFromPath({removeStart}, {path}) {path}", removeStart, path, path);
			return pathBit;
		}
		_logger.LogDebug("RemovePartFromPath({removeStart}, {path}) {path}", removeStart, path, path);
		return path;
	}

	// public IEnumerable<string> PlayQueue() {
	// 	var mpdPlaylist = _mpd.PlayListInfo();
	// 	if (mpdPlaylist == null) {
	// 		return [];
	// 	}
	// 	var songs = mpdPlaylist.Songs.Select(CurrentSongFullText);
	// 	return songs;
	// }

	public bool QueueAddSongToEnd(string uri) {
		var result = _mpd.QueueAdd(uri, null);
		return result;
	}

	public bool QueueAddSongBefore(string uri, uint pos) {
		var result = _mpd.QueueAddRelative(uri, pos, addAfter: false);
		return result;
	}
	public bool QueueAddSongAfter(string uri, uint pos) {
		var result = _mpd.QueueAddRelative(uri, pos, addAfter: true);
		return result;
	}

	public bool QueueClear() {
		var result = _mpd.QueueClear();
		return result;
	}

	public bool QueueRemove(int id) {
		var result = _mpd.QueueDeleteId(id);
		return result;
	}

	public bool QueueShuffle() {
		var result = _mpd.QueueShuffle();
		return result;
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
