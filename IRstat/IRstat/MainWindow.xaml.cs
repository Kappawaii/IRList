using Ookii.Dialogs.Wpf;
using System;
using System.Windows;
namespace IRStat
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly App app;
        /// <summary>
        /// Point d'entrée pratique du programme, création de la fenêtre principale
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            app = (App)Application.Current;
            app.Init(this);
            SetElementToFillSpace(ConsoleOutputTextBlock);
            if (VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            {
                Console.WriteLine("Fenêtres de dialogue Vista disponibles !");
            }
            else
            {
                throw new Exception("Fenêtres de dialogue Vista non disponibles");
            }
        }

        private void SearchXmlButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                Description = IRstat_parametres.openFolderDescription
            };
            dialog.ShowDialog();
            if (dialog.SelectedPath != "")
            {
                pathTextBox.Text = dialog.SelectedPath;
                app.LireDossier(dialog.SelectedPath, (bool)pathRecursiveCheckBox.IsChecked);
            }
        }

        private void XPathQueryButton_Click(object sender, RoutedEventArgs e)
        {
            app.ApplyXpathToPool(XPathQueryTextBox.Text, onMetaToggleSwitch.IsChecked);
        }

        private void SetElementToFillSpace(FrameworkElement element)
        {
            element.Width = double.NaN;
            element.Height = double.NaN;
        }

        private void ConsoleOutputTextBlock_AutoScroll(object sender, RoutedEventArgs e)
        {
            ConsoleScrollViewer.ScrollToEnd();
        }

        private void ExportPathButton_Click(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog dialog = new VistaSaveFileDialog
            {
                AddExtension = true,
                CheckFileExists = false,
                DefaultExt = "csv",
                OverwritePrompt = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "Comma-separated values Files (*.csv)|*.csv"



            };
            dialog.ShowDialog();
            exportPathTextBox.Text = dialog.FileName;
        }

        private void ExportDataButton_Click(object sender, RoutedEventArgs e)
        {
            app.ExportCurrentDataToCsv();
        }
    }
}
