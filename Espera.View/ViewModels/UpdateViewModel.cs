﻿using Akavache;
using Espera.Core;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Splat;
using Squirrel;
using Espera.Core.Analytics;
using System.Reactive.Threading.Tasks;

namespace Espera.View.ViewModels
{
    public class UpdateViewModel : ReactiveObject, IDisposable
    {
        private readonly ViewSettings settings;
        private readonly ObservableAsPropertyHelper<bool> shouldRestart;
        private readonly object updateLock;
        private readonly IUpdateManager updateManager;
        private bool updateRun;

        public UpdateViewModel(ViewSettings settings, IUpdateManager updateManager = null)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            this.settings = settings;
            this.updateManager = updateManager ?? new UpdateManager(AppInfo.UpdatePath, "Espera", FrameworkVersion.Net45, AppInfo.AppRootPath);

            this.updateLock = new object();

            this.CheckForUpdate = ReactiveCommand.CreateAsyncTask(_ => this.UpdateSilentlyAsync());

            this.shouldRestart = this.settings.WhenAnyValue(x => x.IsUpdated)
                .ToProperty(this, x => x.ShouldRestart);

            this.Restart = ReactiveCommand.CreateAsyncTask(_ => Task.Run(() => UpdateManager.RestartApp()));

            Observable.Interval(TimeSpan.FromHours(2), RxApp.TaskpoolScheduler)
                .StartWith(0) // Trigger an initial update check
                .InvokeCommand(this.CheckForUpdate);
        }

        /// <summary>
        /// Checks the server if an update is available and applies the update if there is one.
        /// </summary>
        public ReactiveCommand<Unit> CheckForUpdate { get; }

        /// <summary>
        /// Used in the changelog dialog to opt-out of the automatic changelog.
        /// </summary>
        public bool DisableChangelog { get; set; }

        public IEnumerable<ChangelogReleaseEntry> ReleaseEntries
        {
            get
            {
                return BlobCache.LocalMachine.GetObject<Changelog>(BlobCacheKeys.Changelog)
                    .Select(x => x.Releases)
                    .Wait();
            }
        }

        public ReactiveCommand<Unit> Restart { get; }

        public bool ShouldRestart => this.shouldRestart.Value;

        public bool ShowChangelog => this.settings.IsUpdated && this.settings.EnableChangelog;

        public void ChangelogShown()
        {
            this.settings.EnableChangelog = !this.DisableChangelog;
        }

        public void DismissUpdateNotification()
        {
            // We don't want to overwrite the update status if the update manager already downloaded
            // the update
            lock (this.updateLock)
            {
                if (!this.updateRun)
                {
                    this.settings.IsUpdated = false;
                }
            }
        }

        public void Dispose()
        {
            this.updateManager.Dispose();
        }

        private async Task UpdateSilentlyAsync()
        {
            if (ModeDetector.InUnitTestRunner())
                return;

            ReleaseEntry appliedEntry;

            try
            {
                appliedEntry = await this.updateManager.UpdateApp();
            }

            catch (Exception ex)
            {
                this.Log().Error("Failed to update application", ex);
                AnalyticsClient.Instance.RecordNonFatalError(ex);
                return;
            }

            if (appliedEntry != null)
            {
                await ChangelogFetcher.FetchAsync().ToObservable()
                    .Timeout(TimeSpan.FromSeconds(30))
                    .SelectMany(x => BlobCache.LocalMachine.InsertObject(BlobCacheKeys.Changelog, x))
                    .LoggedCatch(this, Observable.Return(Unit.Default), "Could not to fetch changelog")
                    .ToTask();

                lock (this.updateLock)
                {
                    this.updateRun = true;
                    this.settings.IsUpdated = true;
                }

                this.Log().Info($"Updated to version {appliedEntry.Version}");
            }
        }
    }
}