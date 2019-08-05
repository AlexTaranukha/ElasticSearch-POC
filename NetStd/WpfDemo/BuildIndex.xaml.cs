using dtSearch;
using dtSearch.Engine;
using dtSearch.Sample;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace WpfDemo
    {
    /// <summary>
    /// Interaction logic for BuildIndex.xaml
    /// </summary>
    public partial class BuildIndex : Window
        {

        private SampleIndexJob sampleIndexJob;
        private SampleApp sampleApp;


        public BuildIndex(SampleApp aSampleApp) {
            InitializeComponent();
            sampleApp = aSampleApp;
            sampleIndexJob = new SampleIndexJob(ReportProgress);
            sampleIndexJob.IndexPath = aSampleApp.IndexPath;


            FolderToIndex.Text = Properties.Settings.Default.FolderToIndex;
            IncludeSubfoldersCheckBox.IsChecked = Properties.Settings.Default.IncludeSubfolders;
            IncludeFilters.Text = "*";
            }

        private async void StartIndexingButton_Click(object sender, RoutedEventArgs e) {

            Properties.Settings.Default.FolderToIndex = FolderToIndex.Text;
            Properties.Settings.Default.IncludeSubfolders = (IncludeSubfoldersCheckBox.IsChecked == true);
            Properties.Settings.Default.Save();

            sampleIndexJob.ActionCreate = true;
            sampleIndexJob.ActionAdd = true;
            sampleIndexJob.ActionCompress = (CompressCheckBox.IsChecked == true);
            string folder = FolderToIndex.Text;
            if (IncludeSubfoldersCheckBox.IsChecked == true) {
                // The symbol <+> after a folder tells dtSearch to index
                // subfolders.
                if (!folder.Contains("<+>"))
                    folder += "<+>";
                }

            sampleIndexJob.FoldersToIndex.Add(folder);
            char[] delimiters = { ' ', ',', ';' };
            sampleIndexJob.IncludeFilters.AddRange(IncludeFilters.Text.Split(delimiters));

            StartIndexingButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            bool ok = await ExecuteIndexJobAsync();

            StartIndexingButton.IsEnabled = true;
            CancelButton.IsEnabled = false;


            }
        private async Task<bool> ExecuteIndexJobAsync() {
            bool ret = await Task.Run<bool>(() => ExecuteIndexJob());

            IndexFileList.ItemsSource = sampleIndexJob.GetFilesIndexed();

            MessageBlock.Text = sampleIndexJob.SummarizeIndexResult();

            return ret;
            }

        private bool ExecuteIndexJob() {
            // Set up option settings if needed.
            // Options are maintained per-thread, so this must be done
            // on the thread where the job wil execute.
            sampleApp.InitOptions();

            sampleIndexJob.Execute();

            return sampleIndexJob.Failed() ? false : true;
            }


        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            sampleIndexJob.CancelJob();
            }

        private void ReportProgress(int pct) {
            double prog = pct;
            prog /= 100.0;
            IndexProgressBar.Value = prog;
            }
        }
    }
