using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Utilities;
using MpdSharp;
using MpdSharp.Models;


namespace GuiMpc;


public partial class MainWindow : Window {
	private readonly Mpd _mpd;
	private StatusModel? _status;
	private CurrentSongModel? _currentSong;
	private readonly DispatcherTimer _viewUpdateTimer;
	private readonly Uri _stopButtonImageUri = new Uri("avares://GuiMpc/Assets/stop2.png");
	private readonly Uri _playButtonImageUri = new Uri("avares://GuiMpc/Assets/play2.png");
	private PlayListInfoModel _playListInfo;


	public MainWindow() {
#if DEBUG
		this.AttachDevTools();
#endif
		_mpd = new Mpd();
		_mpd.Connect();
		InitializeComponent();
		PopulatePlayList();
		Timer_Tick(this, EventArgs.Empty);
		UpdateCurrentSong();
		_viewUpdateTimer = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(1)
		};
		_viewUpdateTimer.Tick += Timer_Tick;
		_viewUpdateTimer.Start();
	}


	private void App_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (e.Handled) {
			return;
		}
		BeginMoveDrag(e);
	}


	// Media buttons
	private void Play_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		Debug.WriteLine("Click Play");
		if (_status?.State == State.Stop || _status?.State == State.Pause) {
			if (_mpd.Play()) {
				var bitmap = new Bitmap(AssetLoader.Open(_stopButtonImageUri));
				PlayImage.Source = bitmap;
			}
		} else {
			if (_mpd.Stop()) {
				var bitmap = new Bitmap(AssetLoader.Open(_playButtonImageUri));
				PlayImage.Source = bitmap;
				SongPosition.Value = 0;
			}
		}
	}
	private void Pause_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		Debug.WriteLine("Click Pause");
		_mpd.Pause();
	}
	private void Next_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		Debug.WriteLine("Click Next");
		_mpd.Next();
		UpdateCurrentSong();
	}
	private void Prev_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		Debug.WriteLine("Click Prev");
		_mpd.Previous();
		UpdateCurrentSong();
	}

	// exit app on clicking exit icon
	private void Close_OnClick(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
			lifetime.Shutdown();
		}
	}


	// Song Position slider
	private void SongPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
		if (e.Handled || _status == null) {
			return;
		}
		var oldSliderPos = _status.Elapsed * 100.0 / _status.Duration;
		var newSliderPos = e.NewValue;
		if (oldSliderPos.CompareTo(newSliderPos) != 0) {
			var newSongPos = newSliderPos / 100.0 * _status.Duration;
			_mpd.SeekCurrent(newSongPos);
		}
	}

	// Volume position slider
	private void Volume_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
		if (_status != null) {
			var newVol = VolumeSliderToMpd(e.NewValue);
			_mpd.SetVolume(newVol);
			Console.WriteLine($"User volume changed to {newVol}");
		}
	}

	private void Volume_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
		if (e.Handled) {
			return;
		}
		Volume.Value += e.Delta.Y * 2;
		var mpdVol = VolumeSliderToMpd(Volume.Value);
		_mpd.SetVolume(mpdVol);
		e.Handled = true;
	}

	private static int VolumeSliderToMpd(double sliderVal) {
		var mpdVol = (int)Math.Round(Math.Sqrt(sliderVal) * 10.0);
		return mpdVol;
	}
	private static double VolumeMpdToSlider(int mpdVal) {
		var sliderVol = mpdVal * mpdVal / 100.0;
		return sliderVol;
	}


	// Auto app update
	private void Timer_Tick(object? sender, EventArgs e) {
		_status = _mpd.Status();
		SongPosition.Value = _status.Elapsed * 100.0 / _status.Duration;
		var mpdVolume = _mpd.GetVolume();
		var volVal = VolumeMpdToSlider(mpdVolume);
		if (Volume.Value < volVal - 0.5 || Volume.Value > volVal + 0.5) {
			Console.WriteLine($"volVal: {volVal} Volume.Value: {Volume.Value} _status.Volume: {mpdVolume}");
			Volume.Value = volVal;
		}
		UpdateCurrentSong();
	}


	// Playlist
	private void PopulatePlayList() {
		var mpdPlaylist = _mpd.PlayListInfo();
		if (mpdPlaylist == null) {
			return;
		}
		_playListInfo = mpdPlaylist;
		List<ListBoxItem> listBoxItems = [];
		var songsListItems = _playListInfo.CurrentSongs.Select(item => new ListBoxItem {
			Content = CurrentSongFullText(item),
			Margin = new Thickness(5, 0, 5, 0),
			Foreground = new SolidColorBrush(new Color(255, 255, 200, 200)),
			Background = new SolidColorBrush(new Color(44, 34, 0, 0)),
		});
		listBoxItems.AddRange(songsListItems);
		PlaylistContainer.Items.Clear();
		foreach (var li in listBoxItems) {
			PlaylistContainer.Items.Add(li);
		}
	}

	private void PlayListContainer_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
		if (e.Handled) {
			return;
		}
		var selectedItem = e.AddedItems[0] as ListBoxItem;
		var selectedText = selectedItem?.Content as string;
		if (string.IsNullOrEmpty(selectedText)) {
			return;
		}
		foreach (var item in _playListInfo.CurrentSongs) {
			if (string.Equals(selectedText, CurrentSongFullText(item), StringComparison.Ordinal)) {
				if (item.Pos != _currentSong?.Pos) {
					_mpd.Play(item.Pos);
				}
				return;
			}
		}
	}


	private static string CurrentSongFullText(CurrentSongModel currentSong) {
		var hasArtist = !string.IsNullOrEmpty(currentSong.Artist);
		var hasAlbum = !string.IsNullOrEmpty(currentSong.Album);
		var hasTitle = !string.IsNullOrEmpty(currentSong.Title);
		var bob = new List<string>();
		if (hasArtist) {
			bob.Add(currentSong.Artist);
		}
		if (hasAlbum) {
			bob.Add(currentSong.Album);
		}
		if (hasTitle) {
			bob.Add(currentSong.Title);
		}
		var fullTitle = string.Join(" - ", bob);
		return fullTitle;
	}


	private void UpdateCurrentSong() {
		var curSong = _mpd.CurrentSong();
		if (curSong == null) {
			return;
		}
		if (_currentSong?.Id == curSong.Id) {// new song?
			return;
		}
		_currentSong = curSong;
		var songTitle = CurrentSongFullText(curSong);
		SongTitle.Text = songTitle;
		foreach (var item in PlaylistContainer.Items) {
			var i = item as ListBoxItem;
			if (string.Equals(songTitle, i?.Content as string, StringComparison.Ordinal)) {
				PlaylistContainer.SelectedItem = item;
				break;
			}
		}
		var imageBytes = _mpd.ReadPicture(curSong.File);
		if (imageBytes.Length <= 0) {
			imageBytes = _mpd.AlbumArt(curSong.File);
			if (imageBytes.Length <= 0) {
				return;
			}
		}
		using var stream = new MemoryStream(imageBytes);
		var bitmap = new Bitmap(stream);
		AlbumArt.Source = bitmap;
	}



	// clean up when exiting app
	private void Window_OnClosing(object? sender, WindowClosingEventArgs e) {
		Console.WriteLine("Closing Window nicely...");
		_viewUpdateTimer.Stop();
		_mpd.Close();
	}
}
