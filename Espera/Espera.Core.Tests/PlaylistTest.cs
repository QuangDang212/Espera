﻿using Espera.Core.Management;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Espera.Core.Tests
{
    [TestFixture]
    public sealed class PlaylistTest
    {
        [Test]
        public void AddSongs_AddTwoSong_IndexesAreCorrect()
        {
            Song[] songs = Helpers.SetupSongMocks(2);

            var playlist = new Playlist("Playlist");

            playlist.AddSongs(songs);

            Assert.AreEqual(0, playlist[0].Index);
            Assert.AreEqual(1, playlist[1].Index);
        }

        [Test]
        public void AddSongs_ArgumentIsNull_ThrowsArgumentNullException()
        {
            var playlist = new Playlist("Playlist");

            Assert.Throws<ArgumentNullException>(() => playlist.AddSongs(null));
        }

        [Test]
        public void AddSongs_PlaylistContainsSongs()
        {
            Song[] songs = Helpers.SetupSongMocks(4);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            Assert.AreEqual(4, playlist.Count());
            Assert.AreEqual(songs[0], playlist[0].Song);
            Assert.AreEqual(songs[1], playlist[1].Song);
            Assert.AreEqual(songs[2], playlist[2].Song);
            Assert.AreEqual(songs[3], playlist[3].Song);
        }

        [Test]
        public void CanPlayNextSong_CurrentSongIndexIsLastSong_ReturnsFalse()
        {
            Song[] songs = Helpers.SetupSongMocks(4);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.CurrentSongIndex.Value = 3;

            Assert.IsFalse(playlist.CanPlayNextSong.FirstAsync().Wait());
        }

        [Test]
        public void CanPlayNextSong_CurrentSongIndexIsNull_ReturnsFalse()
        {
            Song[] songs = Helpers.SetupSongMocks(4);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.CurrentSongIndex.Value = null;

            Assert.IsFalse(playlist.CanPlayNextSong.FirstAsync().Wait());
        }

        [Test]
        public void CanPlayNextSong_CurrentSongIndexIsZero_ReturnsTrue()
        {
            Song[] songs = Helpers.SetupSongMocks(4);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.CurrentSongIndex.Value = 0;

            Assert.IsTrue(playlist.CanPlayNextSong.FirstAsync().Wait());
        }

        [Test]
        public void CanPlayNextSong_PlaylistIsEmpty_ReturnsFalse()
        {
            var playlist = new Playlist("Playlist");

            Assert.IsFalse(playlist.CanPlayNextSong.FirstAsync().Wait());
        }

        [Test]
        public void CanPlayPreviousSong_CurrentSongIndexIsLastSong_ReturnsTrue()
        {
            Song[] songs = Helpers.SetupSongMocks(4);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.CurrentSongIndex.Value = 3;

            Assert.IsTrue(playlist.CanPlayPreviousSong.FirstAsync().Wait());
        }

        [Test]
        public void CanPlayPreviousSong_CurrentSongIndexIsNull_ReturnsFalse()
        {
            Song[] songs = Helpers.SetupSongMocks(4);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.CurrentSongIndex.Value = null;

            Assert.IsFalse(playlist.CanPlayPreviousSong.FirstAsync().Wait());
        }

        [Test]
        public void CanPlayPreviousSong_CurrentSongIndexIsZero_ReturnsFalse()
        {
            Song[] songs = Helpers.SetupSongMocks(4);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.CurrentSongIndex.Value = 0;

            Assert.IsFalse(playlist.CanPlayPreviousSong.FirstAsync().Wait());
        }

        [Test]
        public void CanPlayPreviousSong_PlaylistIsEmpty_ReturnsFalse()
        {
            var playlist = new Playlist("Playlist");

            Assert.IsFalse(playlist.CanPlayPreviousSong.FirstAsync().Wait());
        }

        [Test]
        public void CurrentSongIndexSetter_PlaylistIsEmptyAndSetToNull_Passes()
        {
            new Playlist("Playlist").CurrentSongIndex.Value = null;
        }

        [Test]
        public void CurrentSongIndexSetter_PlaylistIsEmptyAndSetToZero_ThrowsArgumentOutOfRangeException()
        {
            var playlist = new Playlist("Playlist");

            Assert.Throws<ArgumentOutOfRangeException>(() => playlist.CurrentSongIndex.Value = 0);
        }

        [Test]
        public void CurrentSongIndexSetter_ValueIsNotInPlaylistRange_ThrowsArgumentOutOfRangeException()
        {
            Song[] songs = Helpers.SetupSongMocks(3);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            Assert.Throws<ArgumentOutOfRangeException>(() => playlist.CurrentSongIndex.Value = 3);
        }

        [Test]
        public void GetIndexes_MultipleSongs_ReturnsCorrectIndexes()
        {
            Song[] songs = Helpers.SetupSongMocks(3, true);

            Playlist playlist = Helpers.SetupPlaylist(songs);

            IEnumerable<int> indexes = playlist.GetIndexes(songs);

            Assert.IsTrue(indexes.SequenceEqual(new[] { 0, 1, 2 }));
        }

        [Test]
        public void GetIndexes_OneSong_ReturnsCorrectIndexes()
        {
            Song song = Helpers.SetupSongMock("Song", true);

            Playlist playlist = Helpers.SetupPlaylist(song);

            int index = playlist.GetIndexes(new[] { song }).Single();

            Assert.AreEqual(0, index);
        }

        [Test]
        public void GetIndexes_OneSongWithMultipleReferences_ReturnsCorrectIndexes()
        {
            Song song = Helpers.SetupSongMock("Song", true);

            Playlist playlist = Helpers.SetupPlaylist(Enumerable.Repeat(song, 3));

            IEnumerable<int> indexes = playlist.GetIndexes(new[] { song });

            Assert.IsTrue(indexes.SequenceEqual(new[] { 0, 1, 2 }));
        }

        [Test]
        public void GetIndexes_PassSongsThatAreNotInPlaylist_ReturnsNoIndexes()
        {
            Song[] songs = Helpers.SetupSongMocks(4, true);

            Playlist playlist = Helpers.SetupPlaylist(songs.Take(2));

            IEnumerable<int> indexes = playlist.GetIndexes(songs.Skip(2));

            Assert.IsEmpty(indexes);
        }

        [Test]
        public void Indexer_LessThanZero_ThrowsArgumentOutOfRangeException()
        {
            var playlist = new Playlist("Playlist");

            PlaylistEntry temp;

            Assert.Throws<ArgumentOutOfRangeException>(() => temp = playlist[-1]);
        }

        [Test]
        public void Indexer_MoreThanZero_ThrowsArgumentOutOfRangeException()
        {
            Song song = Helpers.SetupSongMock();
            var playlist = Helpers.SetupPlaylist(song);

            PlaylistEntry temp;

            Assert.Throws<ArgumentOutOfRangeException>(() => temp = playlist[1]);
        }

        [Test]
        public void InsertMove_FromIndexIsLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            var playlist = new Playlist("Playlist");

            Assert.Throws<ArgumentOutOfRangeException>(() => playlist.InsertMove(-1, 0));
        }

        [Test]
        public void InsertMove_InsertSongToPlaylist_OrderIsCorrent()
        {
            Song[] songs = Helpers.SetupSongMocks(5);

            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.InsertMove(3, 1);

            var expected = new[] { songs[0], songs[3], songs[1], songs[2], songs[4] };

            Assert.IsTrue(playlist.Select(entry => entry.Song).SequenceEqual(expected));
        }

        [Test]
        public void InsertMove_ToIndexIsBiggerThanFromIndex_ThrowsArgumentException()
        {
            var playlist = new Playlist("Playlist");

            Assert.Throws<ArgumentException>(() => playlist.InsertMove(0, 1));
        }

        [Test]
        public void InsertMove_ToIndexIsEqualFromIndex_ThrowsArgumentException()
        {
            var playlist = new Playlist("Playlist");

            Assert.Throws<ArgumentException>(() => playlist.InsertMove(0, 0));
        }

        [Test]
        public void InsertMove_ToIndexIsLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            var playlist = new Playlist("Playlist");

            Assert.Throws<ArgumentOutOfRangeException>(() => playlist.InsertMove(0, -1));
        }

        [Test]
        public void NameSetter_IsTemporaryIsTrue_ThrowsInvalidOperationException()
        {
            var playlist = new Playlist("Playlist", true);

            Assert.Throws<InvalidOperationException>(() => playlist.Name = "Test");
        }

        [Test]
        public void RemoveSongs_ArgumentIsNull_ThrowsArgumentNullException()
        {
            var playlist = new Playlist("Playlist");

            Assert.Throws<ArgumentNullException>(() => playlist.RemoveSongs(null));
        }

        [Test]
        public void RemoveSongs_RemoveMultipleSongs_OrderIsCorrect()
        {
            Song[] songs = Helpers.SetupSongMocks(7);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.RemoveSongs(new[] { 1, 3, 4 });

            Assert.AreEqual(4, playlist.Count());
            Assert.AreEqual(songs[0], playlist[0].Song);
            Assert.AreEqual(songs[2], playlist[1].Song);
            Assert.AreEqual(songs[5], playlist[2].Song);
            Assert.AreEqual(songs[6], playlist[3].Song);
        }

        [Test]
        public void RemoveSongs_RemoveOneSong_OrderIsCorrect()
        {
            Song[] songs = Helpers.SetupSongMocks(4);
            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.RemoveSongs(new[] { 1 });

            Assert.AreEqual(3, playlist.Count());
            Assert.AreEqual(songs[0], playlist[0].Song);
            Assert.AreEqual(songs[2], playlist[1].Song);
            Assert.AreEqual(songs[3], playlist[2].Song);
        }

        [Test]
        public void RemoveSongsCorrectsCurrentSongIndex()
        {
            Song[] songs = Helpers.SetupSongMocks(2);

            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.CurrentSongIndex.Value = 1;

            playlist.RemoveSongs(new[] { 0 });

            Assert.AreEqual(0, playlist.CurrentSongIndex.Value);
        }

        [Test]
        public void ShuffleMigratesCurrentSongIndex()
        {
            Song[] songs = Helpers.SetupSongMocks(100, true);

            Playlist playlist = Helpers.SetupPlaylist(songs);

            playlist.CurrentSongIndex.Value = 0;

            playlist.Shuffle();

            int newIndex = playlist.GetIndexes(new[] { songs[0] }).First();

            Assert.AreEqual(newIndex, playlist.CurrentSongIndex.Value);
        }
    }
}