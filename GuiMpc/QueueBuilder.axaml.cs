using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MpdSharp;
using MpdSharp.Models;


namespace GuiMpc;


public partial class QueueBuilder : Window {
	private readonly Mpd _mpd;
	private string _currentUri;
	private ListInfoModel? _lsInfo;
	private PlayListInfoModel? _playListInfo;
	public QueueBuilder() {
		_mpd = new Mpd();
		_ = _mpd.Connect();
		InitializeComponent();
		_currentUri = "/Organised/Ren";
		UpdatePlayQueue();
		UpdateBrowser();
	}

	private void UpdateBrowser() {
		var lsInfo = _mpd.LsInfo(_currentUri);
		if (lsInfo == null) {
			return;
		}
		_lsInfo = lsInfo;
		Path.Text = _currentUri;
		var startTextIndex = _currentUri.Length + 1;
		var listItemsDirs = _lsInfo.Directories.Select(item => new ListBoxItem {
			Content = item.Directory[startTextIndex..]
		});
		var listItemsFiles = _lsInfo.Files.Select(item => new ListBoxItem {
			Content = item.File[startTextIndex..],
		});
		Browser.Items.Clear();
		ListBoxAddItems(Browser, [new ListBoxItem { Content = ".." }, ..listItemsDirs]);
		ListBoxAddItems(Browser, listItemsFiles);
	}

	private void UpdatePlayQueue() {
		var mpdPlaylist = _mpd.PlayListInfo();
		if (mpdPlaylist == null) {
			return;
		}
		_playListInfo = mpdPlaylist;
		var songs = _playListInfo.Songs.Select(s => new ListBoxItem {
			Content = CurrentSongFullText(s),
		});
		PlayQueue.Items.Clear();
		ListBoxAddItems(PlayQueue, songs);
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

	private static void ListBoxAddItems(ListBox listBox, IEnumerable<ListBoxItem> listItems) {
		foreach (var li in listItems) {
			listBox.Items.Add(li);
		}
	}


	private void App_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
		if (e.Handled) {
			return;
		}
		BeginMoveDrag(e);
	}

	private void Path_OnTextChanged(object? sender, TextChangedEventArgs e) {
		var s = sender as TextBox;
		_currentUri = s?.Text ?? "/";
		UpdateBrowser();
	}


	private void Close_OnClick(object? sender, RoutedEventArgs e) {
		if (e.Handled) {
			return;
		}
		Close();
	}
}
