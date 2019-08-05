using System;
using System.Collections.Generic;
using System.Text;
using dtSearch.Engine;
using dtSearch.Sample;

namespace ConsoleDemo
    {
    class ConsoleApp : SampleApp
        {
        static string GetInput(string prompt, string defaultVal = null) {
            Console.Write(prompt + ":  ");
            if (!string.IsNullOrWhiteSpace(defaultVal)) {
                Console.Write("[" + defaultVal + "] ");
                }
            string ret = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ret) && !string.IsNullOrWhiteSpace(defaultVal))
                return defaultVal;
            return ret;
            }

        static bool AskYesNo(string prompt, string defaultVal = "N") {
            prompt = prompt + " (Y/N)";
            string answer = GetInput(prompt, defaultVal);
            return (answer.ToUpper().StartsWith('Y'));
            }

        string DocsFolder;
        string Request;


        public void BuildIndex() {
            IndexPath = GetInput("Index Path", IndexPath);
            DocsFolder = GetInput("Docs Folder", DocsFolder);
            using (SampleIndexJob job = new SampleIndexJob()) {
                job.IndexPath = IndexPath;
                job.ActionCreate = true;
                job.ActionAdd = true;
                job.ActionMerge = true;
                job.FoldersToIndex.Add(DocsFolder);
                DateTime startTime = DateTime.Now;
                Console.WriteLine("BuildIndex Start - " + startTime.ToString());
                job.Execute();
                DateTime endTime = DateTime.Now;
                Console.WriteLine("BuildIndex End - " + endTime.ToString());
                Console.WriteLine("Total Execution Time - " + (endTime - startTime).ToString());
                var fileList = job.GetFilesIndexed();
                int logCount = 0;
                foreach (var file in fileList) {
                    Console.WriteLine(file.Filename + " " + file.Detail);
                    logCount++;
                    if (logCount > 25) {
                        if (AskYesNo("See more items?"))
                            logCount = 0;
                        else
                            break;
                        }
                    }
                Console.WriteLine(job.SummarizeIndexResult());
                }
            Console.WriteLine("Indexing complete - " + DateTime.Now.ToString());

            }

        public void SearchIndex() {
            IndexPath = GetInput("Index Path", IndexPath);
            Request = GetInput("Request", Request);
            using (SearchJob searchJob = new SearchJob()) {
                searchJob.Request = Request;
                searchJob.MaxFilesToRetrieve = 50;
                searchJob.IndexesToSearch.Add(IndexPath);
                searchJob.SearchFlags = SearchFlags.dtsSearchDelayDocInfo;
                searchJob.Execute(AllocResults());
                }
            ShowResults();
            }

        public void ShowResults() {
            Console.WriteLine(SearchResults.TotalHitCount + " Hits in " + SearchResults.TotalFileCount + " Files");
            SearchResultsItem item = new SearchResultsItem();
            for (int i = 0; i < SearchResults.Count; ++i) {
                SearchResults.GetNthDoc(i, item);
                Console.WriteLine(item.HitCount + " " + item.Filename);
                }
            }

        public void Run() {
            Console.WriteLine("\n\ndtSearch .NET Core sample application\n");
            VersionInfo versionInfo = new VersionInfo();
            if (!versionInfo.LoadedOK) {
                Console.WriteLine("Error loading dtSearchEngine.dll");
                Console.WriteLine(versionInfo.LoadError);
                return;
                }

            Console.WriteLine("dtSearch Engine version: " + versionInfo.EngineVersion);
            Console.WriteLine(versionInfo.PlatformString);
            Console.WriteLine("-----------------------------------------------\n");
            while (true) {
                string cmd = GetInput("Enter I to build an index, S to search an index, or Q to quit");
                cmd = cmd.ToUpper();
                if (cmd.StartsWith('I'))
                    BuildIndex();
                else if (cmd.StartsWith('S'))
                    SearchIndex();
                else if (cmd.StartsWith('Q'))
                    return;
                }
            }
        }
    }
