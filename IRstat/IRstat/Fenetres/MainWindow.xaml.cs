using DataVirtualization;
using IRStat.Core.Data;
using IRStat.GUI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IRStat
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly App app;

        private ColumnDescriptorProvider columns;

        //Order and last clicked column for IrListView sorting
        //Changes every click, so first click will be ascending
        Order lastOrder = Order.Descending;
        string lastColumn = null;

        /// <summary>
        /// Point d'entrée pratique du programme, création de la fenêtre principale
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            app = (App)Application.Current;
            app.Init(this);
            SetElementToFillSpace(ConsoleOutputTextBlock);
            SetElementToFillSpace(IrListView);
            SizeChanged += Window_Resize;
            columns = new ColumnDescriptorProvider(app.ColumnDescriptors);
            columns.AddColumn(new ColumnDescriptor("Résultat XPath", ""));
            UpdateDataContext();
        }

        #region ClickEvents

        private void ExportDataButton_Click(object sender, RoutedEventArgs args)
        {
            app.ExportCurrentDataToCsv();
        }

        private void ExportConsoleButton_Click(object sender, RoutedEventArgs args)
        {
            app.ExportConsoleOutputToFile(ConsoleOutputTextBlock);
        }
        private void NouvelleColonne_Click(object sender, RoutedEventArgs args)
        {
            columns.AddColumn(new ColumnDescriptor("Nouvelle Colonne", ""));
            UpdateDataContext();
        }
        private void SupprimerColonne_Click(object sender, RoutedEventArgs args)
        {
            columns.DeleteColumn(ListBox.SelectedItem as ColumnDescriptor);
            UpdateDataContext();
        }

        private void LvColumnHeader_Click(object sender, RoutedEventArgs args)
        {
            if (sender is ListView && args.OriginalSource is GridViewColumnHeader gvcHeader)
            {
                if(gvcHeader?.Column.Header is string columnString)
                {
                    if (gvcHeader?.Column is AdvancedGridViewColumn advancedColumn)
                    {
                        columnString = "\n"+advancedColumn.UniqueId;
                    }
                    if (!columnString.Equals(lastColumn))
                    {
                        lastColumn = columnString;
                        lastOrder = Order.Ascending;
                    }
                    else
                    {
                        lastOrder = lastOrder.Next();
                    }
                    app.SortLvByColumn(columnString, lastOrder);
                }
            }
        }

        private void SearchXmlButton_Click(object sender, RoutedEventArgs args)
        {
            app.LireDossier((bool)pathRecursiveCheckBox.IsChecked);
        }

        private void XPathAllQueriesButton_Click(object sender, RoutedEventArgs args)
        {
            app.ApplyAllXPathToPool();
        }
        #endregion

        #region WindowEvents
        private void Window_Resize(object sender, RoutedEventArgs args)
        {
            //resizing for IrListView
            Thickness temp = IrListView.Margin;
            temp.Right = -temp.Left - RenderSize.Width + 1900;
            IrListView.Margin = temp;
        }

        /// <summary>
        /// When window is closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
        #endregion

        #region FrameWorkElementEvents
        private void OnListViewItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs args)
        {
            args.Handled = true;
        }

        private void GestionColonnes_Click_Highlight(object sender, RoutedEventArgs args)
        {
            object dataContext = (sender as FrameworkElement).DataContext;
            for (int i = 0; i < ListBox.Items.Count; i++)
            {
                if (dataContext == ListBox.Items[i])
                {
                    ListBox.SelectedIndex = i;
                }
            }
        }
        private void ConsoleOutputTextBlock_AutoScroll(object sender, RoutedEventArgs args)
        {
            ConsoleScrollViewer.ScrollToEnd();
        }
        #endregion

        #region GestionColonnes
        private void UpdateDataContext()
        {
            columns = new ColumnDescriptorProvider(columns.Columns);
            ListBox.DataContext = new AsyncVirtualizingCollection<ColumnDescriptor>(columns);
            Trace.WriteLine(ListBox.ItemsSource);
            foreach (var obj in ListBox.Items)
            {
                Trace.WriteLine(obj);
            }
        }

        private void OnMetaToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            GetColumnDescriptor(sender).Cible = XpathQueryParam.onMeta;
        }

        private void OnMetaToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            GetColumnDescriptor(sender).Cible = XpathQueryParam.onIR;
        }
        private void GestionColonnesXPathQueryButton_Click(object sender, RoutedEventArgs e)
        {
            app.ApplyXPathToPool(GetColumnDescriptor(sender));
        }

        private ColumnDescriptor GetColumnDescriptor(object obj)
        {
            if (obj is Button button)
            {
                return button.DataContext as ColumnDescriptor;
            }
            else if (obj is ToggleSwitch.HorizontalToggleSwitch toggleSwitch)
            {
                return toggleSwitch.DataContext as ColumnDescriptor;
            }
            return null;
        }
        #endregion

        #region HelperFunctions
        private void SetElementToFillSpace(FrameworkElement element)
        {
            //fill all available space
            element.Width = double.NaN;
            element.Height = double.NaN;
        }

        #endregion
    }
}