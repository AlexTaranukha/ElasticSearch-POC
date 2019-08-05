using System;
using System.Collections.Generic;
using dtSearch.Engine;
using System.Text;

namespace dtSearch.Sample {

    public class UrlEncodedQuery {
        private StringBuilder data;

        public UrlEncodedQuery() {
            data = new StringBuilder();
            }
        public void Add(string name, int value) {
            Add(name, value.ToString());
            }
        public void Add(string name, string value) {
            if (data.Length > 0)
                data.Append("&");
            data.Append(System.Net.WebUtility.UrlEncode(name));
            data.Append("=");
            data.Append(System.Net.WebUtility.UrlEncode(value));
            }
        public override string ToString() {
            return data.ToString();
            }
        };
    public class UrlDecodedQuery {
        private Dictionary<string, string> data;

        public UrlDecodedQuery(string query) {
            data = new Dictionary<string, string>();

            string[] vars = query.Split("&");
            foreach (var v in vars) {
                string[] pair = v.Split('=');
                if (pair.Length != 2)
                    continue;
                string name = pair[0];
                string value = pair[1];
                data[name] = value;
                }
            }
        public string Get(string name, string defaultValue = "") {
            string value = data[name];
            if (value == null)
                return "";
            return value;
            }
        public int GetInt(string name, int defaultValue = 0) {
            string value = data[name];
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            int v = defaultValue;
            Int32.TryParse(data[name], out v);
            return v;
            }
        };


    public class SearchUrlEncoder {
        private const string strRequest = "SearchRequest";
        private const string strFileConditions = "FileConditions";
        private const string strSearchFlags = "SearchFlags";
        private const string strBooleanConditions = "BooleanConditions";
        private const string strAutoStop = "AutoStop";

        public static void UrlEncode(SearchJob searchJob, UrlEncodedQuery query) {
            query.Add(strRequest, searchJob.Request);
            query.Add(strSearchFlags, (int)searchJob.SearchFlags);
            if (searchJob.AutoStopLimit != 0)
                query.Add(strAutoStop, searchJob.AutoStopLimit);
            if (!string.IsNullOrWhiteSpace(searchJob.FileConditions))
                query.Add(strFileConditions, searchJob.FileConditions);
            if (!string.IsNullOrWhiteSpace(searchJob.BooleanConditions))
                query.Add(strBooleanConditions, searchJob.BooleanConditions);
            }

        public static string UrlEncodeToString(SearchJob searchJob) {
            UrlEncodedQuery query = new UrlEncodedQuery();
            UrlEncode(searchJob, query);
            return query.ToString();
            }

        public static void UrlDecode(SearchJob searchJob, UrlDecodedQuery query) {
            searchJob.Request = query.Get(strRequest);
            searchJob.SearchFlags = (SearchFlags)query.GetInt(strSearchFlags);
            searchJob.AutoStopLimit = query.GetInt(strAutoStop);
            searchJob.FileConditions = query.Get(strFileConditions);
            searchJob.BooleanConditions = query.Get(strBooleanConditions);
            }
        }
    public struct PagerItem {
        public string Url;
        public int pageNum;
        public bool isCurrent;
        }

    public class PagingInfo {
        public int PrevPageNum;
        public string PrevPageUrl;
        public bool HavePrevPageNum;
        public int NextPageNum;
        public bool HaveNextPageNum;
        public string NextPageUrl;
        public int PageSize;
        public int PageNum;
        public int PageCount;
        public int SearchResultsCount;
        public int TotalCount;
        public int PagerSize;
        public List<PagerItem> PagerItems;
        
        private string makeUrlForPage(string baseUrl, int pageNum) {
            string ret = baseUrl;
            if (!ret.EndsWith('&'))
                ret += "&";
            ret = ret + "&PageNum=" + pageNum.ToString();
            ret = ret + "&PageSize=" + PageSize.ToString();
            return ret;
            }
        public PagingInfo(string thisSearchUrl, SearchResults results, int aPageNum, int aPageSize, int pagerControlSize) {
            TotalCount = results.TotalFileCount;
            SearchResultsCount = results.Count;
            PageNum = aPageNum;
            PageSize = aPageSize;
            if (PageSize == 0)
                PageSize = results.Count;
            if ((TotalCount == 0) || (PageSize == 0))
                PageCount = 0;
            else
                PageCount = (TotalCount + PageSize - 1) / PageSize;

            if (PageNum + 1 < PageCount) {
                NextPageNum = PageNum + 1;
                HaveNextPageNum = true;
                NextPageUrl = makeUrlForPage(thisSearchUrl, NextPageNum);
                }
            if (PageNum > 0) {
                PrevPageNum = PageNum - 1;
                HavePrevPageNum = true;
                PrevPageUrl = makeUrlForPage(thisSearchUrl, PrevPageNum);
                }

            // Set up table for pager, which should have pagerSize page links
            int firstPagerLink = PageNum - pagerControlSize / 2;
            if (firstPagerLink < 0)
                firstPagerLink = 0;
            int lastPagerLink = firstPagerLink + pagerControlSize;
            if (lastPagerLink >= PageCount)
                lastPagerLink = PageCount - 1;

            PagerItems = new List<PagerItem>();
            for (int i = firstPagerLink; i <= lastPagerLink; ++i) {
                PagerItem item = new PagerItem();
                item.pageNum = i + 1;
                item.isCurrent = (i == PageNum);
                item.Url = makeUrlForPage(thisSearchUrl, i);
                PagerItems.Add(item);
                }
            }
        public bool HaveItemsToDisplay() {
            if (SearchResultsCount < 1)
                return false;
            if (PageNum * PageSize >= SearchResultsCount)
                return false;
            return true;
            }
        public int GetFirstItemToDisplay() {
            if ((PageNum < 1) || (PageSize < 1))
                return 0;
            return PageNum * PageSize;
            }
        public int GetLastItemToDisplay() {
            if (!HaveItemsToDisplay())
                return -1;
            if (PageSize < 1)
                return SearchResultsCount;
            int iFirst = GetFirstItemToDisplay();
            int iLast = iFirst + PageSize - 1;
            if (iLast >= SearchResultsCount)
                iLast = SearchResultsCount - 1;
            return iLast;
            }

        }
    }

