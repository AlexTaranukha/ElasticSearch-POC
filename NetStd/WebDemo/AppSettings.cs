using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dtSearch.Engine;
using System.IO;

namespace WebDemo {
    public class FacetDefinition {
        public string DisplayName { set; get; }
        public string FieldName { set; get; }
        public bool BracketKeys { set; get; }
        }
    public class IndexTableItem {
        public int IndexId { set; get; }
        public string Name { set; get; }
        public string Path { set; get; }

        public bool IsDefault { set; get; }

        public List<FacetDefinition> Facets { set; get; }
        }

    public class IndexTable : List<IndexTableItem> {

        public IndexTableItem FindById(string aId) {
            int v = 0;
            if (int.TryParse(aId, out v))
                return FindById(v);
            return null;
            }
        public IndexTableItem FindById(int aId) {
            foreach (var item in this) {
                if (item.IndexId == aId)
                    return item;
                }
            return null;
            }
        public IndexTableItem FindByPath(string aPath) {
            foreach (var item in this) {
                if (item.Path.Equals(aPath))
                    return item;
                }
            return null;
            }
        public bool IsDefaultIndex(int aId) {
            var index = FindById(aId);
            if (index != null)
                return index.IsDefault;
            return false;
            }
        public string GetPathForId(int aId) {
            IndexTableItem item = FindById(aId);
            if (item != null)
                return item.Path;
            return "";
            }
        public string GetDefaultIndexIds() {
            string ret = "";
            foreach (var item in this) {
                if (item.IsDefault) {
                    if (ret.Length > 0)
                        ret += ",";
                    ret += item.IndexId.ToString();
                    }
                }
            return ret;
            }

        public List<string> GetIndexPaths(string indexIdList) {
            List<string> ret = new List<string>();
            if (string.IsNullOrWhiteSpace(indexIdList))
                return ret;
            string[] ids = indexIdList.Split(",");
            foreach (var id in ids) {
                int idNum = -1;
                if (Int32.TryParse(id, out idNum)) {
                    IndexTableItem item = FindById(idNum);
                    if (item != null) {
                        ret.Add(item.Path);
                        }
                    }
                }
            return ret;
            }
        }

    public class SynopsisSettings {
        public bool GenerateSynopsis { set; get; }
        public int MaxContextBlocks { set; get; }
        public bool IncludeFileStart {set; get;}
        public int WordsOfContext { set; get; }
        public string ContextFooter { set; get; }
        public string ContextHeader { set; get; }
        public string ContextSeparator { set; get; }
        public int MaxWordsToRead { set; get; }
        }

    public class IndexCacheSettings {
        public int MaxIndexCount { set; get; }
        public int AutoCloseTime { set; get; }
        public int AutoReopenTime { set; get; }

        public IndexCacheSettings() {
            MaxIndexCount = 10;
            AutoCloseTime = 60;
            AutoReopenTime = 60;
            }
        }
    public class AppSettings {
        public string IndexPath { set; get; }
        public IndexTable IndexTable { set; get; }
        public int MaxTypeaheadWords { set; get; }
        public SynopsisSettings Synopsis { set; get; }

        public IndexCacheSettings IndexCache { set; get; }

        // Maximum allowed size of a page of search results
        public int MaxAllowedPageSize { set; get; }
        // Maximum allowed number of search results pages
        public int MaxAllowedPageCount { set; get; }
        // Maximum allowed size of search results (product of page size and page count)
        public int MaxAllowedResultsSize { set; get; }

        // The dtSearch Engine can handle search requests up to 70000 characters.
        // This setting allows the application to set a shorter limit.
        public int MaxSearchRequestLength { set; get; }

        public string HomeDir { set; get; }

        public bool ShowWordList { set; get; }
        public bool MultiColorHighlighting { set; get; }
        public void SetEngineOptions() {
            dtSearch.Engine.Options options = new dtSearch.Engine.Options();
            options.HomeDir = HomeDir;
            options.TempFileDir = Path.GetTempPath();
            options.Save();

            }
        }
    }
