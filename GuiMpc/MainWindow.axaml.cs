using System;
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
using MpdSharp.Models;


namespace GuiMpc;


public partial class MainWindow : Window {
	private readonly ILogger<MainWindow> _logger;
	private readonly MpdWrapper _mpdWrapper;
	private readonly DispatcherTimer _viewUpdateTimer;
	private readonly Uri _stopButtonImageUri = new Uri("avares://GuiMpc/Assets/stop2.png");
	private readonly Uri _playButtonImageUri = new Uri("avares://GuiMpc/Assets/play2.png");
	private string _currentUri = string.Empty;


	public MainWindow(MpdWrapper mpdWrapper, ILogger<MainWindow> logger) {
#if DEBUG
		this.AttachDevTools();
#endif
		InitializeComponent();
		_mpdWrapper = mpdWrapper;
		_logger = logger;
		_mpdWrapper.Connect();
		ShowPlayerView();
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
		_logger.LogDebug("Click Queue");
		FileBrowserPopulate();
		PlayQueuePopulate();
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
		PopulatePlayList();
		Timer_Tick(this, EventArgs.Empty);
		UpdateCurrentSong(fullUpdate: true);
		UpdateButtons();
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
		var vol = (int)e.NewValue;
		_mpdWrapper.SetVol(vol);
		Volume.Value = vol;
	}

	private void Volume_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
		if (e.Handled || (e.Delta.Y > 0 && Volume.Value >= 100.0) || (e.Delta.Y < 0 && Volume.Value <= 0.0)) {
			return;
		}
		var vol = _mpdWrapper.GetVol();
		vol += (int)(e.Delta.Y * 2);
		_mpdWrapper.SetVol(vol);
		Volume.Value = vol;
	}

	// UI state update
	private void Timer_Tick(object? sender, EventArgs e) {
		if (PlayerView.IsEnabled) {
			var vol = _mpdWrapper.GetVol();
			Volume.Value = vol;
			var pos = _mpdWrapper.GetSongPos();
			SongPosition.Value = pos;
		}
		if (PlaylistView.IsEnabled) { }
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

	private void PlaylistContainer_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
		if (e.Handled || PlaylistContainer.SelectedItems == null || PlaylistContainer.SelectedItems.Count <= 0) {
			return;
		}
		var selectedItem = PlaylistContainer.SelectedItems[0] as ListBoxItem;
		var selectedText = selectedItem?.Content as string;
		if (string.IsNullOrEmpty(selectedText)) {
			return;
		}
		_mpdWrapper.PlayNewSong(selectedText);
		UpdateCurrentSong();
		SongTitle.Text = selectedText;
	}

	private void UpdateCurrentSong(bool fullUpdate = false) {
		var curSong = _mpdWrapper.GetCurrentSongFullText();
		if (curSong == null) {
			return;
		}
		if (!fullUpdate && string.Equals(SongTitle.Text, curSong)) {// new song?
			return;
		}
		SongTitle.Text = curSong;
		var selectedItem = PlaylistContainer.Items.FirstOrDefault(i => Equals((i as ListBoxItem)?.Content, curSong));
		PlaylistContainer.SelectedItem = selectedItem;
		var imageBytes = _mpdWrapper.GetSongArt();
		if (imageBytes.Length <= 0) {
			AlbumArt.Source = null;
			return;
		}
		using var stream = new MemoryStream(imageBytes);
		var bitmap = new Bitmap(stream);
		AlbumArt.Source = bitmap;
	}

	// clean up when exiting app
	private void Window_OnClosing(object? sender, WindowClosingEventArgs e) {
		_logger.LogDebug("Closing Window nicely...");
		_viewUpdateTimer.Stop();
		_mpdWrapper.Close();
	}



	private void FileBrowserPopulate() {
		var files = _mpdWrapper.FileBrowser(_currentUri);
		if (!files.Any()) {
			_currentUri = string.Empty;
			files = _mpdWrapper.FileBrowser(_currentUri);
		}
		Path.Text = _currentUri;
		FileBrowser.Items.Clear();
		foreach (var file in files) {
			FileBrowser.Items.Add(file);
		}
	}

	private void PlayQueuePopulate() {
		var songs = _mpdWrapper.GetPlayQueue();
		var curSong = _mpdWrapper.GetCurrentSong();
		PlayQueue.Items.Clear();
		foreach (var song in songs) {
			PlayQueue.Items.Add(song);
			if (song.Id == curSong?.Id) {
				PlayQueue.SelectedItem = song;
			}
		}
	}


	private DateTime _fileBrowserLastClick = DateTime.MinValue;
	private void FileBrowser_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
		if (e.Handled || FileBrowser.SelectedItems == null || FileBrowser.SelectedItems.Count <= 0) {
			return;
		}
		var now = DateTime.Now;
		if (_fileBrowserLastClick == DateTime.MinValue || now - _fileBrowserLastClick > TimeSpan.FromSeconds(2)) {
			_fileBrowserLastClick = now;
			e.Handled = true;
			return;
		}
		if (FileBrowser.SelectedItems[0] is not FileModel selectedFileModel) {
			_currentUri = string.Empty;
		} else {
			if (_mpdWrapper.IsFile(_currentUri, selectedFileModel.Name)) {
				QueueAddSongToEnd(selectedFileModel);
				_logger.LogInformation("Add at end..");
			} else if (string.Equals(selectedFileModel.Path, "..")) {
				var lastIndex = _currentUri.LastIndexOf('/');
				_currentUri = lastIndex >= 0 ? _currentUri[..lastIndex] : string.Empty;
			} else {
				_currentUri = selectedFileModel.Path;
			}
		}
		FileBrowserPopulate();
		_fileBrowserLastClick = DateTime.MinValue;
	}
	private void FileAddAfter_OnClick(object? sender, RoutedEventArgs e) {
		if (e.Handled || FileBrowser.SelectedItems == null || FileBrowser.SelectedItems.Count <= 0) {
			return;
		}
		if (FileBrowser.SelectedItems[0] is FileModel selectedFileModel) {
			if (PlayQueue.SelectedItems?.Count >= 1) {
				var songAdded = _mpdWrapper.QueueAddSongAfter(selectedFileModel.Path, 0);
				if (songAdded) {
					PlayQueuePopulate();
				}
				return;
			}
			QueueAddSongToEnd(selectedFileModel);
			PlayQueuePopulate();
		}
	}

	private void QueueAddSongToEnd(FileModel selectedFileModel) {
		var songAdded = _mpdWrapper.QueueAddSongToEnd(selectedFileModel.Path);
		if (songAdded) {
			PlayQueuePopulate();
		}
	}
	private void AddAtEnd_OnClick(object? sender, RoutedEventArgs e) {
		if (e.Handled || FileBrowser.SelectedItems == null || FileBrowser.SelectedItems.Count <= 0) {
			return;
		}
		if (FileBrowser.SelectedItems[0] is FileModel selectedFileModel) {
			QueueAddSongToEnd(selectedFileModel);
		}
	}
	private void FileAddBefore_OnClick(object? sender, RoutedEventArgs e) {
		if (e.Handled || FileBrowser.SelectedItems == null || FileBrowser.SelectedItems.Count <= 0) {
			return;
		}
		if (FileBrowser.SelectedItems[0] is FileModel selectedFileModel) {
			if (PlayQueue.SelectedItems?.Count >= 1) {
				var songAdded = _mpdWrapper.QueueAddSongBefore(selectedFileModel.Path, 0);
				if (songAdded) {
					PlayQueuePopulate();
				}
				return;
			}
			QueueAddSongToEnd(selectedFileModel);
			PlayQueuePopulate();
		}
	}
	private void GoToRoot_OnClick(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		_currentUri = string.Empty;
		FileBrowserPopulate();
	}
	private void ClearQueue_OnClick(object? sender, RoutedEventArgs e) {
		_mpdWrapper.QueueClear();
		PlayQueuePopulate();
	}
	private void Remove_OnClick(object? sender, RoutedEventArgs e) {
		if (e.Handled || PlayQueue.SelectedItems?.Count <= 0) {
			return;
		}
		if (PlayQueue.SelectedItems == null) {
			return;
		}
		foreach (SongModel song in PlayQueue.SelectedItems) {
			_mpdWrapper.QueueRemove(song.Id);
		}
		PlayQueuePopulate();
	}
	private void Shuffle_OnClick(object? sender, RoutedEventArgs e) {
		_mpdWrapper.QueueShuffle();
		PlayQueuePopulate();
	}
}
