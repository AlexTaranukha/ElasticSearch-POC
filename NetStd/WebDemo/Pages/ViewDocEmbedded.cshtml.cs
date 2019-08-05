using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using dtSearch.Engine;
using dtSearch.Sample;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;

namespace WebDemo.Pages {
    public class ViewDocModel : PageModel {
        public string HighlightedDocument;
        public SearchResultsItem Item;
        private AppSettings appSettings;
        private IHostingEnvironment _hostingEnvironment;

        [BindNever]
        public string ErrorMessage { set; get; }
        [BindNever]
        public bool WasError { set; get; }

        public string BaseHRef { set; get; }

        private readonly ILogger _log;


        public ViewDocModel(IHostingEnvironment hostingEnvironment, IOptions<AppSettings> settings, ILogger<ViewDocModel> logger) {
            _log = logger;
            _hostingEnvironment = hostingEnvironment;
            appSettings = settings.Value;
            }

        public IActionResult OnGet() {
            string urlEncodedItem = HttpContext.Request.QueryString.HasValue ? HttpContext.Request.QueryString.Value : "";
            var query = QueryHelpers.ParseQuery(urlEncodedItem);
            string indexId = query["ixid"];
            string indexPath = "";
            if (!string.IsNullOrWhiteSpace(indexId)) {
                indexPath = appSettings.IndexTable.GetPathForId(Int32.Parse(indexId));
                }
            HighlightedDocument = HighlightHits(urlEncodedItem, indexPath);
            if (WasError)
                _log.LogError(EventId.DocAccessError, "Error accessing document \"{filename}\".  The error was: \"{message}\"", Item.Filename, ErrorMessage);
            else
                _log.LogInformation(EventId.DocAccess, "Opened document \"{filename}\".", Item.Filename);
            return Page();
            }
        private string HighlightHits(string urlEncodedItem, string indexPath) {
            appSettings.SetEngineOptions();
            Item = new SearchResultsItem();
            using (SearchResults results = new SearchResults()) {
                if (!string.IsNullOrWhiteSpace(indexPath))
                    results.UrlDecodeItemWithIndex(urlEncodedItem, indexPath);
                else {
                    results.UrlDecodeItem(urlEncodedItem);
                    }
                if (results.Count == 0) {
                    WasError = true;
                    ErrorMessage = "Unable to get document information from index " + indexPath;
                    return null;
                    }
                results.GetNthDoc(0, Item);
                using (FileConverter fc = new FileConverter()) {
                    if (SampleFileConverter.SetupFileConverter(fc, results, 0, _hostingEnvironment.WebRootPath)) {
                        if (appSettings.MultiColorHighlighting)
                            SampleFileConverter.SetupMulticolorHighlighting(fc);
                        fc.Execute();
                        if (!fc.Failed()) {
                            BaseHRef = fc.BaseHRef;
                            return fc.OutputString;
                            }
                        else
                            ErrorMessage = fc.Errors.ToString();
                        }
                    else
                        ErrorMessage = "Error accessing document information in SearchResults";
                    }
                WasError = true;
                return null;
                }

            }
        }
    }