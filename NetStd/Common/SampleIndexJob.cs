using dtSearch.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace dtSearch.Sample
    {
    public class IndexResultItem
        {
        public string Filename { set; get; }
        public string Location { set; get; }
        public int WordCount { set; get; }
        public string Error { set; get; }
        public bool Success { set; get; }
        public bool Encrypted { set; get; }
        public string Detail { set; get; }

        public IndexResultItem(IndexFileInfo info, MessageCode updateType) {
            Filename = info.Name;
            Location = info.Location;

            if (updateType == MessageCode.dtsnIndexFileOpenFail) {
                Error = info.OpenFailMessage;
                Detail = "Not indexed: " + Error;
                }
            else if (updateType == MessageCode.dtsnIndexFileDone) {
                Success = true;
                WordCount = info.WordCount;
                Detail = info.Type + " " + info.WordCount + " words";
                }
            else if (updateType == MessageCode.dtsnIndexFileEncrypted) {
                Encrypted = true;
                Detail = "Encrypted";
                }

            }
        }
    public class SampleIndexStatusHandler : IIndexStatusHandler
        {
        public List<IndexResultItem> FileList = new List<IndexResultItem>();
        public IProgress<int> ProgressReporter { set; get; }

        public int MaxListSize { set; get; }

        public SampleIndexStatusHandler() {
            MaxListSize = 10000;
            }

        public void BeforeExecute() {
            Cancelled = false;
            Stopped = false;
            FileList.Clear();
            }

        public IndexProgressInfo Result;
        public void OnProgressUpdate(IndexProgressInfo info) {
            Server.AddToLog("Index progress: " + info.PercentDone + "%");
            if ((ProgressReporter != null) && !Cancelled)
                ProgressReporter.Report(info.PercentDone);

            // Use these OnProgressUpdate message types to keep track of which files
            // were successfully indexed.
            // The list is limited by MaxListSize to prevent problems populating the ListView
            // control after the update is done.  In a production application this type of information
            // should be logged to a file because the number of files indexed may be in the millions.
            switch (info.UpdateType) {
                case MessageCode.dtsnIndexFileDone:
                case MessageCode.dtsnIndexFileOpenFail:
                case MessageCode.dtsnIndexFileEncrypted:
                    if (FileList.Count < MaxListSize)
                        FileList.Add(new IndexResultItem(info.File, info.UpdateType));
                    break;
                case MessageCode.dtsnIndexDone:
                    Result = info;
                    break;
                default:
                    break;
                }
            }
        public AbortValue CheckForAbort() {
            if (Cancelled)
                // Cancel the job and revert the index to its state before the
                // update started
                return AbortValue.CancelImmediately;
            else if (Stopped)
                // Stop indexing new documents and save data indexed so far to the index
                return AbortValue.Cancel;
            else
                return AbortValue.Continue;
            }

        public bool Cancelled { set; get; }
        public bool Stopped { set; get; }
        }

    public class SampleIndexJob : IndexJob
        {
        private SampleIndexStatusHandler IndexStatusHandler { set; get; }

        public SampleIndexJob(Action<int> aProgressHandler = null) {
            IndexStatusHandler = new SampleIndexStatusHandler();
            if (aProgressHandler != null)
                IndexStatusHandler.ProgressReporter = new Progress<int>(aProgressHandler);

            StatusHandler = IndexStatusHandler;
            }

        public override bool Execute() {
            IndexStatusHandler.BeforeExecute();

            return base.Execute();
            }

        public List<IndexResultItem> GetFilesIndexed() {
            return IndexStatusHandler.FileList;
            }

        public void CancelJob() {
            IndexStatusHandler.Cancelled = true;
            }

        public string SummarizeIndexResult() {
            if (Failed())
                return Errors.ToString();
            else if (ActionVerify)
                return "Index verified without errors";
            string ret = "";
            ret += IndexStatusHandler.Result.FilesRead + " files indexed\r\n";
            ret += IndexStatusHandler.Result.EncryptedCount + " encrypted files skipped\r\n";
            ret += IndexStatusHandler.Result.OpenFailures + " other files could not be opened\r\n";
            ret += IndexStatusHandler.Result.WordsInIndex + " words in index\r\n";
            return ret;
            }

        }


    }

