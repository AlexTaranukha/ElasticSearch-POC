using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using dtSearch.Engine;
using dtSearch.Sample;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebDemo {
    [Route("api/[controller]")]
    public class WordsController : Controller {
        private int maxWordCount;
        private IndexCache indexCache;
        private AppSettings appSettings;

        public struct WordListItem {
            public string word;
            public int count;
            };

        public WordsController(IOptions<AppSettings> settings, WebDemoIndexCache aCache) {

            maxWordCount = settings.Value.MaxTypeaheadWords;
            appSettings = settings.Value;
            indexCache = aCache;
            }

        //
        // http://localhost:<port>/api/words/list?ixid=1&type=0&ct=5&req=sample
        // ixid: Identifies the index to list from, which the appSettings.IndexTable maps to a path
        // type: Type of word list.  0=ListMatchingWords, 1=ListWords
        // ct:   Number of words to return
        // req:  Search request
        //
        // Returns a simple array of words
        [HttpGet("List")]
        public IEnumerable<string> Get(int ixid, int type, int ct, string req) {
            // The control on the client may pass the entire search request
            // (not just the last word) so tokenize the input and take only the last word
            // as the input for WordListBuilder
            var wordList = new List<string>();

            string indexPath = appSettings.IndexTable.GetPathForId(ixid);
            if (!string.IsNullOrWhiteSpace(indexPath) && !string.IsNullOrWhiteSpace(req)) {
                string word = req.Split(' ').Last();
                if ((ct > maxWordCount) || (ct <= 0))
                    ct = maxWordCount;
                using (WordListBuilder wordListBuilder = new WordListBuilder()) {
                    wordListBuilder.OpenIndex(indexPath, indexCache);
                    int numWords = 0;
                    switch (type) {
                        case 0:
                            numWords = wordListBuilder.ListMatchingWords(word + "*", ct, 0, 0);
                            break;
                        case 1:
                            wordListBuilder.Flags = WordListBuilderFlags.dtsWordListPadTopOfList;
                            numWords = wordListBuilder.ListWords(word, ct);
                            break;
                        default:
                            break;
                        }

                    for (int i = 0; i < numWords; i++) {
                        wordList.Add(wordListBuilder.GetNthWord(i));
                        }
                    }
                }
            return wordList;
            }

        //
        // http://localhost:<port>/api/words/info?ixid=1&type=0&ct=5&req=sample
        // ixid: Identifies the index to list from, which the appSettings.IndexTable maps to a path
        // type: Type of word list.  0=ListMatchingWords, 1=ListWords
        // ct:   Number of words to return
        // req:  Search request
        [HttpGet("Info")]
        public IEnumerable<WordListItem> GetWordsWithCount(int ixid, int type, int ct, string req) {
            // The control on the client may pass the entire search request
            // (not just the last word) so tokenize the input and take only the last word
            // as the input for WordListBuilder
            var wordList = new List<WordListItem>();
            string indexPath = appSettings.IndexTable.GetPathForId(ixid);
            if (!string.IsNullOrWhiteSpace(indexPath) && !string.IsNullOrWhiteSpace(req)) {

                string word = req.Split(' ').Last();
                if ((ct > maxWordCount) || (ct <= 0))
                    ct = maxWordCount;
                using (WordListBuilder wordListBuilder = new WordListBuilder()) {
                    wordListBuilder.OpenIndex(indexPath, indexCache);
                    int numWords = 0;
                    switch (type) {
                        case 0:
                            numWords = wordListBuilder.ListMatchingWords(word + "*", ct, 0, 0);
                            break;
                        case 1:
                            wordListBuilder.Flags = WordListBuilderFlags.dtsWordListPadTopOfList;
                            numWords = wordListBuilder.ListWords(word, ct);
                            break;
                        default:
                            break;
                        }

                    for (int i = 0; i < numWords; i++) {
                        WordListItem item = new WordListItem();
                        item.word = wordListBuilder.GetNthWord(i);
                        item.count = wordListBuilder.GetNthWordCount(i);
                        wordList.Add(item);
                        }
                    }
                }

            return wordList;
            }


        }
    }
