using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using dtSearch.Sample;
using dtSearch.Engine;


namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static SampleApp TheSampleApp { set; get; }

        public MainWindow()
        {
            InitializeComponent();

            VersionInfo vi = new VersionInfo();
            VersionInfoLabel.Text = "dtSearch Engine " + vi.EngineVersion + "\r\n" + vi.PlatformString;


            TheSampleApp = new SampleApp();
            TheSampleApp.IndexPath = Path.Combine(Path.GetTempPath(), "index");

            IndexPathEdit.Text = TheSampleApp.IndexPath;

            // HomeDir is the folder where the dtSearch Engine will look for  
            // configuration and data files such as stemming rules (stemming.dat),
            // the CMAP data files for PDF files, and the noise word list (noise.dat).
            //
            // External file parser DLLs are found in the "viewers" folder under the 
            // HomeDir folder, so the HomeDir will be different for 64-bit and 32-bit 
            // processes.
            //
            // See http://support.dtsearch.com/webhelp/dtsearchCppApi/frames.html?frmname=topic&frmfile=Home_and_Private_Directories.html
            TheSampleApp.HomeDir = @"c:\Program Files (x86)\dtSearch Developer\bin";
            if (Environment.Is64BitProcess)
                TheSampleApp.HomeDir = @"c:\Program Files (x86)\dtSearch Developer\bin64";
            TheSampleApp.TempFileDir = Path.GetTempPath();

        }

        private void BuildIndexButton_Click(object sender, RoutedEventArgs e)
        {
            BuildIndex bi = new BuildIndex(TheSampleApp);
            bi.ShowDialog();
        }

        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            Search search = new Search(TheSampleApp);

            search.ShowDialog();
        }

        private void DebugLoggingCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            TheSampleApp.EnableDebugLogging(DebugLoggingCheckBox.IsChecked == true);
            DebugLogNameEdit.Text = TheSampleApp.DebugLogName;
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            VerifyIndex dlg = new VerifyIndex(TheSampleApp);
            dlg.ShowDialog();
        }
    }
}
