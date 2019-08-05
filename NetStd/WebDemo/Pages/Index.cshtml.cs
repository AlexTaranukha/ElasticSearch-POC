using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using dtSearch.Engine;
using dtSearch.Sample;
using static dtSearch.Engine.SearchFlags;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using static System.Net.WebUtility;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;

namespace WebDemo.Pages
{
    public enum SearchType
    {
        NoValue,
        AllWords,
        AnyWords,
        Boolean
    }
    public class SearchModel : PageModel
    {

        [BindNever]
        public SearchResults SearchResults { set; get; }

        [BindNever]
        public SearchFilter ResultsAsFilter { set; get; }

        [BindNever]
        public SearchFacets FacetValuesFromSearch { set; get; }

        [BindNever]
        public AppSettings Settings { set; get; }

        [BindNever]
        public PagingInfo Pager { set; get; }
        [BindNever]
        public string ErrorMessage { set; get; }

        [BindNever]
        public bool WasError { set; get; }

        [BindProperty(SupportsGet = true)]
        public string SearchRequest { set; get; }

        [BindProperty(SupportsGet = true)]
        public int PageNum { set; get; }
        [BindProperty(SupportsGet = true)]
        public int PageSize { set; get; }
        [BindProperty(SupportsGet = true)]
        public bool Fuzzy { set; get; }
        [BindProperty(SupportsGet = true)]
        public int Fuzziness { set; get; }
        [BindProperty(SupportsGet = true)]
        public bool Stemming { set; get; }
        [BindProperty(SupportsGet = true)]
        public bool PhonicSearching { set; get; }
        [BindProperty(SupportsGet = true)]
        public SearchType SearchType { set; get; }

        [BindProperty(SupportsGet = true)]
        public int SearchFlags { set; get; }

        [BindProperty(SupportsGet = true)]
        public bool NoFrames { set; get; }

        [BindProperty(SupportsGet = true)]
        public bool EnableDateSearch { set; get; }

        [DataType(DataType.Date)]
        [BindProperty(SupportsGet = true)]
        [Required(AllowEmptyStrings = true)]
        public DateTime? StartDate { set; get; }

        [DataType(DataType.Date)]
        [BindProperty(SupportsGet = true)]
        [Required(AllowEmptyStrings = true)]
        public DateTime? EndDate { set; get; }

        [BindProperty(SupportsGet = true)]
        public string FileConditions { set; get; }

        [BindProperty(SupportsGet = true)]
        public string BooleanConditions { set; get; }

        [BindProperty(SupportsGet = true)]
        public string Facets { set; get; }

        // This is one or more IndexId values (integers), which
        // map to index paths in appSettings.IndexTable
        [BindProperty(SupportsGet = true)]
        public List<string> IxId { set; get; }

        // This is the values from IxId, broken into separate strings for easier access
        [BindNever]
        public string[] IndexIds { set; get; }

        private readonly ILogger _log;

        private List<string> IndexesToSearch;

        private IndexCache indexCache;

        public bool IsSelected(int indexId)
        {
            if (IndexIds == null)
            {
                return Settings.IndexTable.IsDefaultIndex(indexId);
            }

            return IndexIds.Contains(indexId.ToString());
        }


        public SearchModel(IOptions<AppSettings> settings, ILogger<SearchModel> log, WebDemoIndexCache aCache)
        {
            indexCache = aCache;
            _log = log;
            Settings = settings.Value;
            SearchRequest = "";
        }


        public bool HaveItemsToDisplay()
        {
            if (Pager == null)
                return false;
            return Pager.HaveItemsToDisplay();
        }

        public bool HaveFacets()
        {
            return (FacetValuesFromSearch != null) && FacetValuesFromSearch.HaveFacets();
        }
        public bool HaveFacetBreadcrumbs()
        {
            return (FacetValuesFromSearch != null) && FacetValuesFromSearch.HaveBreadcrumbs();
        }

        public SearchResultsItem GetSearchResultsItem(int iItem, out string highlightUrl)
        {
            SearchResultsItem CurrentItem = new SearchResultsItem();
            SearchResults.GetNthDoc(iItem, CurrentItem);
            string indexId = "";
            if ((CurrentItem.WhichIndex >= 0) && (CurrentItem.WhichIndex < IndexIds.Length))
                indexId = IndexIds[CurrentItem.WhichIndex];
            string urlEncodedItem;
            if (Settings.MultiColorHighlighting)
                urlEncodedItem = SearchResults.UrlEncodeItemWithIndexId(
                        iItem: iItem, 
                        indexId: indexId, 
                        additionalSearchFlags: dtsSearchWantHitsByWord | dtsSearchWantHitsArray | dtsSearchWantHitsByWordOrdinals,
                        asSearch: true);
            else
                urlEncodedItem = SearchResults.UrlEncodeItemWithIndexId(
                        iItem: iItem, 
                        indexId: indexId);
            string aspPage = "ViewDocEmbedded";
            if (NoFrames)
            {
                aspPage = "ViewDoc";
            }
            highlightUrl = "/" + aspPage + "?" + urlEncodedItem;
            return CurrentItem;
        }
        private SearchResults AllocResults()
        {
            SearchResults = new SearchResults();
            ResultsAsFilter = new SearchFilter();
            HttpContext.Response.RegisterForDispose(SearchResults);
            HttpContext.Response.RegisterForDispose(ResultsAsFilter);
            return SearchResults;
        }
        public string GetModelErrors()
        {
            string ret = "";
            var errors = ModelState.Where(state => state.Value.Errors.Any())
                                   .Select(x => new { x.Key, x.Value.Errors });

            foreach (var error in errors)
            {
                if (ret.Length > 0)
                    ret = ret + "\n";
                ret = ret + error.Key + ": ";
                foreach (var modelError in error.Errors)
                {
                    ret = ret + modelError.ErrorMessage + " ";
                }
            }
            return ret;
        }
        public IActionResult OnPostSearch()
        {
            if (!ModelState.IsValid)
            {
                return ShowError(GetModelErrors());
            }
            return DoSearch();
        }
        public IActionResult OnGetSearch()
        {
            if (!ModelState.IsValid)
            {
                return ShowError(GetModelErrors());
            }
            return DoSearch();
        }

        public string MakeFileConditions()
        {
            if (!string.IsNullOrEmpty(FileConditions))
                return FileConditions;
            if (!EnableDateSearch)
                return "";
            if (!StartDate.HasValue && !EndDate.HasValue)
                return "";
            string req = "xfilter(date \"" + formatDate(StartDate.Value) + "~~" + formatDate(EndDate.Value) + "\")";
            return req;
        }
        private string formatDate(DateTime date)
        {
            if (date == null)
                return "";
            return "Y" + date.Year + "/M" + date.Month + "/D" + date.Day;
        }
        public IActionResult DoSearch()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            VersionInfo vi = new dtSearch.Sample.VersionInfo();
            if (!vi.LoadedOK)
                return ShowError(vi.LoadError);

            if (PageSize < 1)
                PageSize = 25;
            if (PageNum < 0)
                PageNum = 0;
            if (PageNum > Settings.MaxAllowedPageCount)
                return ShowError("Invalid results page number " + PageNum);
            if (PageSize > Settings.MaxAllowedPageSize)
                return ShowError("Invalid results page size " + PageSize);

            int MaxFilesToRetrieve = (PageNum + 1) * PageSize;
            if (MaxFilesToRetrieve > Settings.MaxAllowedResultsSize)
                return ShowError("Search results size too large");
            if (string.IsNullOrWhiteSpace(SearchRequest))
                return ShowError("Please enter a search request");
            if (SearchRequest.Length > Settings.MaxSearchRequestLength)
                return ShowError("Search request is too long.  Maximum allowed length is: " + Settings.MaxSearchRequestLength);

            // Called call values for IxId into one comma-delimited string
            string IxIdString = "";
            foreach (var id in IxId)
            {
                if (IxIdString.Length > 0)
                    IxIdString += ",";
                IxIdString = IxIdString + id;
            }
            if (string.IsNullOrWhiteSpace(IxIdString))
                IxIdString = Settings.IndexTable.GetDefaultIndexIds();
            IndexIds = IxIdString.Split(",");
            IndexesToSearch = Settings.IndexTable.GetIndexPaths(IxIdString);

            string thisSearchUrl;
            string thisSearchUrlWithoutFacets;

            using (SearchJob searchJob = new SearchJob())
            {
                searchJob.IndexCache = indexCache;
                searchJob.MaxFilesToRetrieve = MaxFilesToRetrieve;
                searchJob.IndexesToSearch = IndexesToSearch;
                searchJob.Request = SearchRequest;
                searchJob.FileConditions = MakeFileConditions();
                searchJob.BooleanConditions = BooleanConditions;

                searchJob.SearchFlags = dtsSearchDelayDocInfo;
                if (SearchType == SearchType.AllWords)
                    searchJob.SearchFlags |= dtsSearchTypeAllWords;
                else if (SearchType == SearchType.AnyWords)
                    searchJob.SearchFlags |= dtsSearchTypeAnyWords;
                if (Stemming)
                    searchJob.SearchFlags |= dtsSearchStemming;
                if (PhonicSearching)
                    searchJob.SearchFlags |= dtsSearchPhonic;
                if (Fuzzy && (Fuzziness > 0))
                {
                    searchJob.SearchFlags |= dtsSearchFuzzy;
                    searchJob.Fuzziness = Fuzziness;
                }

                searchJob.SearchFlags |= (SearchFlags)SearchFlags;

                // thisSearchUrl is used to build links to use to repeat the search, which is needed for
                // two purposes:  (1) The SearchPager uses thisSearchUrl to generate new URLs representing
                // each page of search results.  (2) The SearchFacets class uses thisSearchUrl as the base
                // to make links implementing new searches for each facet value. 
                thisSearchUrl =
                         HttpContext.Request.Path +
                         "?handler=Search" +
                         "&ixid=" + UrlEncode(IxIdString) +
                         "&" + SearchUrlEncoder.UrlEncodeToString(searchJob);
                if (NoFrames)
                    thisSearchUrl += "&NoFrames=1";


                thisSearchUrlWithoutFacets = thisSearchUrl;
                if (!string.IsNullOrWhiteSpace(Facets))
                {
                    // Add the facets to the SearchJob after encoding it so they are
                    // not included both in BooleanConditions and in Facets
                    thisSearchUrl = thisSearchUrl + "&Facets=" + UrlEncode(Facets);
                    FacetsInQuery f = new FacetsInQuery(Facets);

                    // The facets in the search request will be implemented as BooleanConditions to add to the 
                    // SearchJob.  The search request may or may not use the boolean query syntax depending on the
                    // search type selected in the UI.  For example, the user may have selected an "all words"
                    // or an "any words" search.  SearchJob.BooleanConditions provides a way to add query items that
                    // must be expressed using the boolean query syntax.
                    searchJob.BooleanConditions = SearchFacets.BooleanCombine(searchJob.BooleanConditions, f.AsBooleanConditions());
                }

                bool ok = ExecuteSearch(searchJob);
                if (!ok)
                {
                    string message = searchJob.Errors.ToString();
                    _log.LogInformation(EventId.Search, "Search Error: \"{SearchRequest}\" Time: {SearchTime} Message: {ErrorMessage}", SearchRequest, stopwatch.ElapsedMilliseconds, message);
                    return ShowError(message);
                }
            }
            Pager = new PagingInfo(thisSearchUrl, SearchResults, PageNum, PageSize, 5);

            FacetValuesFromSearch = new SearchFacets(thisSearchUrlWithoutFacets, Facets);
            FacetValuesFromSearch.BuildForSearch(ResultsAsFilter, Settings.IndexTable, IndexIds);

            stopwatch.Stop();
            long millis = stopwatch.ElapsedMilliseconds;

            _log.LogInformation(EventId.Search, "Search: \"{SearchRequest}\" Time: {SearchTime} Results count: {Count}", SearchRequest, stopwatch.ElapsedMilliseconds, SearchResults.Count);

            if (Settings.Synopsis.GenerateSynopsis)
                GenerateSynopsisForThisPage();
            return Page();
        }

        bool ExecuteSearch(SearchJob searchJob)
        {
            Settings.SetEngineOptions();
            AllocResults();
            return searchJob.Execute(SearchResults, ResultsAsFilter);
        }
        public IActionResult ShowError(string msg)
        {
            WasError = true;
            ErrorMessage = msg;
            return Page();
        }

        // Generate a brief hits-in-context snippet to include in search results.
        //
        // If the index was not built with caching of text enabled, this will be MUCH slower.
        //
        // This will populate the SearchResults object with a synopsis for each item in the
        // selected range.
        private void GenerateSynopsisForThisPage()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (SearchReportJob reportJob = new SearchReportJob())
            {
                reportJob.SetResults(SearchResults);
                reportJob.OutputFormat = OutputFormat.itUnformattedHTML;
                reportJob.BeforeHit = "<b>";
                reportJob.AfterHit = "</b>";

                reportJob.WordsOfContextExact = Settings.Synopsis.WordsOfContext;
                reportJob.ContextFooter = Settings.Synopsis.ContextFooter;
                reportJob.ContextHeader = Settings.Synopsis.ContextHeader;
                reportJob.ContextSeparator = Settings.Synopsis.ContextSeparator;
                reportJob.MaxContextBlocks = Settings.Synopsis.MaxContextBlocks;
                reportJob.MaxWordsToRead = Settings.Synopsis.MaxWordsToRead;
                reportJob.SelectItems(PageSize * PageNum, PageSize * (PageNum + 1) - 1);
                reportJob.Flags = ReportFlags.dtsReportGetFromCache | ReportFlags.dtsReportLimitContiguousContext |
                        ReportFlags.dtsReportStoreInResults;
                if (Settings.Synopsis.IncludeFileStart)
                    reportJob.Flags |= ReportFlags.dtsReportIncludeFileStart;
                reportJob.Execute();
            }
            _log.LogInformation(EventId.SearchReport, "SearchReport: \"{SearchRequest}\" Time: {SearchTime} Results count: {Count}", SearchRequest, stopwatch.ElapsedMilliseconds, SearchResults.Count);
        }

    }
    public class EventId
    {
        public const int Search = 1001;
        public const int SearchError = 1002;
        public const int SearchReport = 1100;
        public const int DocAccess = 1200;
        public const int DocAccessError = 1201;
        public const int WordRequest = 1300;
        public const int ApplicationStart = 9001;
        public const int ApplicationExit = 9002;

    }
}