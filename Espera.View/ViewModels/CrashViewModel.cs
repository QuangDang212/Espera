﻿using Espera.Core.Analytics;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace Espera.View.ViewModels
{
    internal class CrashViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool?> sendingSucceeded;

        public CrashViewModel(Exception exception)
        {
            this.ReportContent = exception.ToString();

            this.SubmitCrashReport = ReactiveCommand.CreateAsyncObservable(this.WhenAnyValue(x => x.SendingSucceeded)
                .Select(x => x == null || !x.Value), _ =>
                    Observable.Start(() => AnalyticsClient.Instance.RecordCrash(exception), RxApp.TaskpoolScheduler).Select(__ => true));

            this.sendingSucceeded = this.SubmitCrashReport
                .Select(x => new bool?(x))
                .ToProperty(this, x => x.SendingSucceeded);

            if (this.SendsAutomatically)
            {
                this.SubmitCrashReport.Execute(null);
            }
        }

        public string ReportContent { get; private set; }

        public bool? SendingSucceeded => this.sendingSucceeded.Value;

        public bool SendsAutomatically => AnalyticsClient.Instance.EnableAutomaticReports;

        public ReactiveCommand<bool> SubmitCrashReport { get; }
    }
}