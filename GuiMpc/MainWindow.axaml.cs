using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using MpdSharp;
using MpdSharp.Models;


namespace GuiMpc;


public partial class MainWindow : Window {
	private readonly ILogger<MainWindow> _logger;
	private MpdWrapper _mpdWrapper;
	private readonly DispatcherTimer _viewUpdateTimer;
	private readonly Uri _stopButtonImageUri = new Uri("avares://GuiMpc/Assets/stop2.png");
	private readonly Uri _playButtonImageUri = new Uri("avares://GuiMpc/Assets/play2.png");


	public MainWindow(MpdWrapper mpdWrapper, ILogger<MainWindow> logger) {
#if DEBUG
		this.AttachDevTools();
#endif
		InitializeComponent();
		_mpdWrapper = mpdWrapper;
		_logger = logger;
		PopulatePlayList();
		Timer_Tick(this, EventArgs.Empty);
		UpdateCurrentSong();
		UpdateButtons();
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
	private void UpdateButtons() {
		var playState = _mpdWrapper.GetPlayButtonState();
		switch (playState) {
			case PlayState.Play: {
				var bitmap = new Bitmap(AssetLoader.Open(_stopButtonImageUri));
				PlayImage.Source = bitmap;
				break;
			}
			case PlayState.Pause:
			case PlayState.Stop: {
				var bitmap = new Bitmap(AssetLoader.Open(_playButtonImageUri));
				PlayImage.Source = bitmap;
				break;
			}
			default:
				_logger.LogError("Unknown PlayState: {playState}", playState);
				break;
		}
	}

	private void Play_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		_logger.LogDebug("Click Play");
		var playState = _mpdWrapper.GetPlayButtonState();
		if (playState == PlayState.Stop || playState == PlayState.Pause) {
			_mpdWrapper.Play();
		} else {
			_mpdWrapper.Stop();
			SongPosition.Value = 0;
		}
		UpdateButtons();
	}
	private void Pause_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		_logger.LogDebug("Click Pause");
		_mpdWrapper.Pause();
		UpdateButtons();
	}
	private void Next_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		_logger.LogDebug("Click Next");
		_mpdWrapper.Next();
		UpdateCurrentSong();
	}
	private void Prev_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		_logger.LogDebug("Click Prev");
		_mpdWrapper.Prev();
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




	private void Queue_Clicked(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		Debug.WriteLine("Click Queue");
		ShowPlaylistView();
	}
	private void PlaylistViewClose_OnClick(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		ShowPlayerView();
	}

	private void ShowPlaylistView() {
		PlayerView.IsEnabled = false;
		PlayerView.IsVisible = false;
		PlaylistView.IsEnabled = true;
		PlaylistView.IsVisible = true;
	}

	private void ShowPlayerView() {
		PlayerView.IsEnabled = true;
		PlayerView.IsVisible = true;
		PlaylistView.IsEnabled = false;
		PlaylistView.IsVisible = false;
	}


	// Song Position slider
	private void SongPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
		if (e.Handled) {
			return;
		}
		_mpdWrapper.SetSongPos(e.NewValue);
	}

	// Volume position slider
	private void Volume_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
		_mpdWrapper.SetVol((int)e.NewValue);
	}

	private void Volume_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
		if (e.Handled) {
			return;
		}
		var vol = _mpdWrapper.GetVol();
		vol += e.Delta.Y * 2;
		_mpdWrapper.SetVol((int)vol);
	}

	// Auto app update
	private void Timer_Tick(object? sender, EventArgs e) {
		_mpdWrapper.UpdateMpdState();
		var vol = _mpdWrapper.GetVol();
		Volume.Value = vol;
		var pos = _mpdWrapper.GetSongPos();
		SongPosition.Value = pos;
	}


	// Playlist
	private void PopulatePlayList() {
		var songs = _mpdWrapper.GetPlayList();
		var songsListItems = songs.Select(song => new ListBoxItem {
			Content = song,
		});
		PlaylistContainer.Items.Clear();
		foreach (var li in songsListItems) {
			PlaylistContainer.Items.Add(li);
		}
	}

	private void PlayListContainer_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
		if (e.Handled || e.AddedItems.Count <= 0) {
			return;
		}
		var selectedItem = e.AddedItems[0] as ListBoxItem;
		var selectedText = selectedItem?.Content as string;
		if (string.IsNullOrEmpty(selectedText)) {
			return;
		}
		_mpdWrapper.PlayNewSong(selectedText);
		UpdateCurrentSong();
		SongTitle.Text = selectedText;
	}

	private void UpdateCurrentSong() {
		var curSong = _mpdWrapper.GetCurrentSongFullText();
		if (curSong == null) {
			return;
		}
		if (string.Equals(SongTitle.Text, curSong)) {// new song?
			return;
		}
		SongTitle.Text = curSong;
		var selectedItem = PlaylistContainer.Items.FirstOrDefault(i => string.Equals((i as ListBoxItem)?.Content, curSong));
		PlaylistContainer.SelectedItem = selectedItem;
		var imageBytes = _mpdWrapper.GetSongArt();
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
		_mpdWrapper.Close();
	}
}
