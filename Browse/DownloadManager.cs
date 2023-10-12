using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;

namespace Browse
{
    public class DownloadEvent : EventArgs
    {
        public CoreWebView2DownloadOperation dowloadOperation;
    }

    public class DownloadManager
    {
        CoreWebView2 ww2;

        public event EventHandler<DownloadEvent> DownloadStarting;
        public event EventHandler<DownloadEvent> DownloadUpdated;
        public event EventHandler<DownloadEvent> DownloadFinished;

        public List<CoreWebView2DownloadOperation> Downloads { get; private set; }

        public DownloadManager(CoreWebView2 w)
        {
            Downloads = new();
            ww2 = w;
            ww2.DownloadStarting += Ww2_DownloadStarting;
        }

        private void Ww2_DownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
        {
            Downloads.Add(args.DownloadOperation);
            args.DownloadOperation.BytesReceivedChanged += DownloadOperation_BytesReceivedChanged;
            args.DownloadOperation.StateChanged += DownloadOperation_StateChanged;
            args.DownloadOperation.EstimatedEndTimeChanged += DownloadOperation_EstimatedEndTimeChanged;

            this.DownloadStarting.Invoke(this, new() { dowloadOperation = args.DownloadOperation });
        }

        private void DownloadOperation_EstimatedEndTimeChanged(CoreWebView2DownloadOperation sender, object args)
        {
            this.DownloadUpdated.Invoke(this, new() { dowloadOperation = sender });
        }

        private void DownloadOperation_BytesReceivedChanged(CoreWebView2DownloadOperation sender, object args)
        {
            this.DownloadUpdated.Invoke(this, new() { dowloadOperation = sender });
        }

        private void DownloadOperation_StateChanged(CoreWebView2DownloadOperation sender, object args)
        {
            if (sender.State == CoreWebView2DownloadState.Completed)
            {
                this.DownloadFinished.Invoke(this, new() { dowloadOperation = sender });

                Downloads.Remove(sender);
            }
            else
            {
                this.DownloadUpdated.Invoke(this, new() { dowloadOperation = sender });
            }
        }
    }
}
