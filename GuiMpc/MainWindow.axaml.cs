using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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
	}


	private void App_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		BeginMoveDrag(e);
	}


	// Media buttons
	private void ButtonPlay(object? sender, RoutedEventArgs e) {
		Debug.WriteLine("Click Play");
		_mpd.Play();
	}
	private void ButtonPause(object? sender, RoutedEventArgs e) {
		Debug.WriteLine("Click Pause");
		_mpd.Pause();
	}
	private void ButtonStop(object? sender, RoutedEventArgs e) {
		Debug.WriteLine("Click Stop");
		_mpd.Stop();
	}
	private void ButtonNext(object? sender, RoutedEventArgs e) {
		Debug.WriteLine("Click Next");
		_mpd.Next();
		UpdateCurrentSong();
	}
	private void ButtonPrev(object? sender, RoutedEventArgs e) {
		Debug.WriteLine("Click Prev");
		_mpd.Previous();
		UpdateCurrentSong();
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
			var newVol = (int)(Math.Sqrt(e.NewValue) * 10.0);
			_mpd.SetVolume(newVol);
			_status.Volume = newVol;
			Console.WriteLine($"User volume changed to {newVol}");
		}
	}


	// Auto app update
	private void Timer_Tick(object? sender, EventArgs e) {
		_status = _mpd.Status();
		SongPosition.Value = _status.Elapsed * 100.0 / _status.Duration;
		var volVal = _status.Volume * _status.Volume / 100;
		if (Volume.Value.CompareTo(volVal) != 0) {
			Volume.Value = volVal;
			Console.WriteLine($"volVal: {volVal} Volume.Value: {Volume.Value} _status.Volume: {_status.Volume}");
		}
		UpdateCurrentSong();
	}



	private void UpdateCurrentSong() {
		var curSong = _mpd.CurrentSong();
		if (curSong == null) {
			return;
		}
		if (_currentSong?.Id == curSong.Id) {// new song
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



	private void Window_OnClosing(object? sender, WindowClosingEventArgs e) {
		_viewUpdateTimer.Stop();
		_mpd.Close();
	}
}
