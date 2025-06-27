using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MpdSharp;
using MpdSharp.Models;


namespace GuiMpc;


public partial class MainWindow : Window {
	private readonly Mpd _mpd;
	private StatusModel? _status;
	private CurrentSongModel? _currentSong;

	private DispatcherTimer _viewUpdateTimer;
	public MainWindow() {
		_mpd = new Mpd();
		_mpd.Connect();
		InitializeComponent();
		UpdateCurrentSong();
		Timer_Tick(this, EventArgs.Empty);
		_viewUpdateTimer = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(1)
		};
		_viewUpdateTimer.Tick += Timer_Tick;
		_viewUpdateTimer.Start();
		PopulatePlayList();
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
		_mpd.Play();

	}
	private void Pause_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		Debug.WriteLine("Click Pause");
		_mpd.Pause();
	}
	private void Stop_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		Debug.WriteLine("Click Stop");
		_mpd.Stop();
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
			_status.Volume = newVol;
			_mpd.SetVolume(newVol);
			Console.WriteLine($"User volume changed to {newVol}");
		}
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
		var volVal = VolumeMpdToSlider(_status.Volume);
		if (Volume.Value < volVal - 0.5 || Volume.Value > volVal + 0.5) {
			Console.WriteLine($"volVal: {volVal} Volume.Value: {Volume.Value} _status.Volume: {_status.Volume}");
			Volume.Value = volVal;
		}
		UpdateCurrentSong();
	}


	// Playlist
	private PlayListInfoModel _playListInfo;

	private void PopulatePlayList() {
		var mpdPlaylist = _mpd.PlayListInfo();
		if (mpdPlaylist == null) {
			return;
		}
		_playListInfo = mpdPlaylist;
		List<ListBoxItem> listBoxItems = [];
		foreach (var item in _playListInfo.CurrentSongs) {
			var listItem = new ListBoxItem {
				Content = CurrentSongFullText(item),
				Margin = new Thickness(5, 0, 5, 0),
				Foreground = new SolidColorBrush(new Color(255, 34, 0, 0)),
				Background = new SolidColorBrush(new Color(255, 34, 255, 0)),
			};
			listBoxItems.Add(listItem);
		}
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
			if (string.Equals(selectedText, CurrentSongFullText(item), StringComparison.OrdinalIgnoreCase)) {
				_mpd.Play(item.Pos);
				break;
			}
		}
	}


	private static string CurrentSongFullText(CurrentSongModel currentSong) {
		return $"{currentSong.Artist} - {currentSong.Album} - {currentSong.Title}";
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
		SongTitle.Text = $"{curSong.Artist} - {curSong.Album} - {curSong.Title}";
		var imageBytes = _mpd.AlbumArt(curSong.File);
		if (imageBytes.Length <= 0) {
			return;
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
