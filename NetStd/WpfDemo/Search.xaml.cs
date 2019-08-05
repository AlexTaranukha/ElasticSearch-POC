using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using dtSearch.Sample;
using dtSearch.Engine;

namespace WpfDemo
    {

    public class WordListItem
        {
        public string Word { set; get; }
        public string Detail { set; get; }

        public void MakeFromWordListBuilder(WordListBuilder wlb, int iItem) {
            Word = wlb.GetNthWord(iItem);
            Detail = "        " + wlb.GetNthWordCount(iItem) + " hits in " + wlb.GetNthWordDocCount(iItem) + " documents";
            }

        }
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : Window
        {
        private SampleApp sampleApp;

        private WordListBuilder wordListBuilder;


        private SearchJob searchJob;
        private string errors;

        public Search(SampleApp aSampleApp) {
            sampleApp = aSampleApp;

            InitializeComponent();

            wordListBuilder = new WordListBuilder();
            wordListBuilder.OpenIndex(sampleApp.IndexPath);
            Request.Text = Properties.Settings.Default.SearchRequest;
            StemmingCheckbox.IsChecked = Properties.Settings.Default.Stemming;
            }

        private void Request_TextChanged(object sender, TextChangedEventArgs e) {
            if (wordListBuilder == null)
                return;

            wordListBuilder.ListWords(Request.Text, 5);

            List<WordListItem> words = new List<WordListItem>();
            for (int i = 0; i < wordListBuilder.Count; ++i) {
                WordListItem item = new WordListItem();
                item.MakeFromWordListBuilder(wordListBuilder, i);
                words.Add(item);
                }

            WordList.ItemsSource = words;

            }

        private async void SearchButton_Click(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.SearchRequest = Request.Text;
            Properties.Settings.Default.Stemming = (StemmingCheckbox.IsChecked == true);
            Properties.Settings.Default.Save();

            using (searchJob = new SearchJob()) {
                searchJob.IndexesToSearch.Add(sampleApp.IndexPath);
                searchJob.Request = Request.Text;
                searchJob.MaxFilesToRetrieve = 100;
                searchJob.AutoStopLimit = 100000;
                if (StemmingCheckbox.IsChecked == true)
                    searchJob.SearchFlags |= SearchFlags.dtsSearchStemming;

                bool ok = await ExecuteSearchJobAsync();
                if (ok)
                    ShowResults();
                else
                    Messages.Text = errors;
                }
            }

        private async Task<bool> ExecuteSearchJobAsync() {
            bool ret = await Task.Run<bool>(() => ExecuteSearchJob());
            return ret;
            }

        private bool ExecuteSearchJob() {
            sampleApp.ExecuteSearch(searchJob);

            if (searchJob.Failed()) {
                errors = searchJob.Errors.ToString();
                return false;
                }
            else {
                errors = "";
                return true;
                }

            }

        private void ShowResults() {
            string msg = sampleApp.SearchResults.TotalFileCount + " files with " + sampleApp.SearchResults.TotalHitCount + " hits";
            Messages.Text = msg;
            SearchResultsWindow resultsWindow = new SearchResultsWindow(sampleApp);
            resultsWindow.Show();
            }

        }
    }
