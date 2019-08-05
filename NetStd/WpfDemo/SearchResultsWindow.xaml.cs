using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using dtSearch.Engine;
using dtSearch.Sample;

namespace WpfDemo
    {
    /// <summary>
    /// Interaction logic for SearchResultsWindow.xaml
    /// </summary>
    public partial class SearchResultsWindow : Window
        {
        private SampleApp sampleApp;
        public SearchResultsWindow(SampleApp aSampleApp) {
            sampleApp = aSampleApp;

            InitializeComponent();

            ResultsListView.ItemsSource = sampleApp.MakeResultList();
            }

        private async void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ResultsListItem item = ResultsListView.SelectedItem as ResultsListItem;
            if (item == null)
                return;

            string html = await Task.Run<string>(() => ConvertForDisplay(item));

            DocumentViewer.NavigateToString(html);
            }

        private string ConvertForDisplay(ResultsListItem item) {
            return SampleFileConverter.HighlightHits(sampleApp.SearchResults, item.OrdinalInSearchResults, "");
            }

        private void DocumentViewer_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            DocumentViewer.InvokeScript("firstHit");
            }
        private void NextHitButton_Click(object sender, RoutedEventArgs e) {
            DocumentViewer.InvokeScript("nextHit");
            }
        private void PrevHitButton_Click(object sender, RoutedEventArgs e) {
            DocumentViewer.InvokeScript("prevHit");
            }

        }
    }
