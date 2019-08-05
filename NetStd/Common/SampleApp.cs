using dtSearch.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;


namespace dtSearch.Sample
    {
    public class EngineLoader {
        public string LoadError { set; get; }
        public bool LoadedOK { set; get; }

        public VersionInfo VersionInfo { set; get; }

        public bool LoadEngine() {
            if (LoadedOK)
                return true;

            string EnginePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dtSearchEngine.dll");
            dtSearch.Engine.Server.SetEnginePath(EnginePath);
            VersionInfo = new dtSearch.Sample.VersionInfo();
            if (VersionInfo.LoadedOK) {
                LoadedOK = true;
                return true;
                }
            return false;
            }
        }
    public class ResultsListItem
        {
        public string Name { set; get; }

        public string Location { set; get; }
        public string FullName { set; get; }
        public string Hits { set; get; }
        public string Date { set; get; }
        public string Detail { set; get; }
        public int OrdinalInSearchResults { set; get; }
        public ResultsListItem(SearchResultsItem item) {
            MakeFromItem(item);
            }
        public void MakeFromItem(SearchResultsItem item) {
            Name = item.ShortName;
            FullName = item.Filename;
            Location = item.Location;
            Date = item.Modified.ToString();
            Hits = item.HitCount.ToString();
            Detail = Hits + " hits " + Date;
            }
        }


    public class SampleApp
        {
        public SearchResults SearchResults;
        public IndexJob IndexJob { set; get; }

        public string IndexPath { set; get; }
        public string HomeDir { set; get; }
        public string TempFileDir { set; get; }
        public string DebugLogName { set; get; }

        public bool DebugLoggingEnabled;

        public SampleApp() {
            HomeDir = "";
            TempFileDir = Path.GetTempPath();

            }


        public SampleApp(SearchResults res) {
            SearchResults = res;
            TempFileDir = Path.GetTempPath();
            }

        public SearchResults AllocResults() {
            if (SearchResults != null)
                SearchResults.Dispose();
            SearchResults = new SearchResults();
            return SearchResults;
            }

        public void ExecuteSearch(SearchJob searchJob) {
            InitOptions();

            searchJob.Execute(AllocResults());
            }

        public void EnableDebugLogging(bool bEnable, string baseName = "") {
            if (DebugLoggingEnabled == bEnable)
                return;
            if (bEnable) {
                if (string.IsNullOrWhiteSpace(baseName))
                    baseName = "debuglog.txt";
                DebugLogName = Path.Combine(TempFileDir, baseName);
                Server.SetDebugLogging(DebugLogName, DebugLogFlags.dtsLogDefault);
                DebugLoggingEnabled = true;
                }
            else {
                Server.SetDebugLogging("", DebugLogFlags.dtsLogDefault);
                DebugLogName = "";
                DebugLoggingEnabled = false;
                }

            }

        public List<ResultsListItem> MakeResultList() {
            List<ResultsListItem> items = new List<ResultsListItem>();
            for (int i = 0; i < SearchResults.Count; ++i) {
                SearchResults.GetNthDoc(i);
                ResultsListItem item = new ResultsListItem(SearchResults.CurrentItem);
                item.OrdinalInSearchResults = i;
                items.Add(item);
                }
            return items;
            }

        // Call before executing a job to make sure option settings are set up correctly.
        // The Options constructor will retrieve the option settings already in effect on this thread.
        // Only call Options.Save() to change settings if something needs to be changed.
        public void InitOptions() {
            // HomeDir is the folder where the dtSearch Engine will look for  
            // configuration and data files such as stemming rules (stemming.dat),
            // the CMAP data files for PDF files, and the noise word list (noise.dat).
            //
            // External file parser DLLs are found in the "viewers" folder under the 
            // HomeDir folder, so the HomeDir will be different for 64-bit and 32-bit 
            // processes.
            //
            // See http://support.dtsearch.com/webhelp/dtsearchCppApi/frames.html?frmname=topic&frmfile=Home_and_Private_Directories.html
            Options options = new Options();
            if (!HomeDir.Equals(options.HomeDir) || !TempFileDir.Equals(options.TempFileDir)) {
                options.HomeDir = HomeDir;
                options.TempFileDir = TempFileDir;
                options.Save();
                }
            }



        }
    }

