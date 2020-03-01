using DataVirtualization;
using IRStat.Core;
using IRStat.Core.Data;
using IRStat.GUI;
using IRStat.IO;
using IRStat.IO.Exports;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;


namespace IRStat
{
    /// <summary>
    /// ViewModel, Contrôleur et fonctions générales de l'application
    /// </summary>
    public partial class App : Application
    {
        private GestionFichiers gestionFichiers;
        IRConsole irConsole;
        private MainWindow mainWindow;

        private IRPool IrPool { get; set; }

        /// <summary>
        /// TODO: doc
        /// </summary>
        public BindingList<ColumnDescriptor> ColumnDescriptors { get => IrPool.Columns; }

        private readonly Stopwatch stopWatch = new Stopwatch();

        private TimeSpan lireDossierTimeSpan;
        private TimeSpan XpathTimeSpan;

        /// <summary>
        /// initialisation du viewmodel
        /// </summary>
        /// <param name="mw"></param>
        public void Init(MainWindow mw)
        {
            mainWindow = mw;
            irConsole = new IRConsole(mw.ConsoleOutputTextBlock);
            IrPool = new IRPool(irConsole);
            gestionFichiers = new GestionFichiers();
            gestionFichiers.LoadFinished += AfficherIRs_new;
            stopWatch.Start();
            if (VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            {
                irConsole.Println("Fenêtres de dialogue Vista disponibles !");
            }
            else
            {
                throw new Exception("Fenêtres de dialogue Vista non disponibles");
            }
            irConsole.Println("Init OK");
            IrPool.NewColumnEvent += NewColumnEvent_Handler;
            IrPool.DeletedColumnEvent += DeletedColumnEvent_Handler;
        }

        /// <summary>
        /// Lit les Instruments de recherche contenus dans un dossier
        /// </summary>
        /// <param name="recursive"></param>
        public void LireDossier(bool recursive)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                Description = IRstat_parametres.openFolderDescription
            };
            dialog.ShowDialog();
            if (dialog.SelectedPath != "")
            {
                //succès
                try
                {
                    lireDossierTimeSpan = stopWatch.Elapsed;
                    IrPool.Clear();
                    gestionFichiers.OpenAllXmlFilesFromDirectory(mainWindow, dialog.SelectedPath, recursive);
                }
                catch (FileNotFoundException e)
                {
                    irConsole.Println(e);
                }
            }
        }

        private void AfficherIRs_new(object sender, SearchForFiles_Args args)
        {
            IrPool.LoadIRs(gestionFichiers.IRList);
            mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                UpdateListViewDataContext();
                mainWindow.nombreIRTextBlock.Text = "( " + gestionFichiers.IRList.Count.ToString() + " chargés )";
                AffChargementIRs(IrPool.IRs.Count, args.dirPath);
            }, null);
        }

        internal void ExportConsoleOutputToFile(TextBlock textBlock)
        {
            VistaSaveFileDialog dialog = new VistaSaveFileDialog
            {
                AddExtension = true,
                CheckFileExists = false,
                DefaultExt = "txt",
                OverwritePrompt = true,
                ValidateNames = true,
                RestoreDirectory = true,
                Filter = "Text File (*.txt)|*.txt"
            };

            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                StringBuilder builder = new StringBuilder();
                foreach (Run run in textBlock.Inlines)
                {
                    builder.Append(run.Text + Environment.NewLine);
                }
                File.WriteAllText(dialog.FileName, builder.ToString());
                irConsole.Println("Logs écrits vers " + dialog.FileName);
            }
        }

        #region XPath

        /// <summary>
        /// Exécute la requête XPath choisie pour tous les IRs chargés
        /// </summary>
        /// <param name="column"></param>
        public void ApplyXPathToPool(ColumnDescriptor column)
        {
            ProgressDialog dialog = new ProgressDialog
            {
                ShowCancelButton = false,
                Text = "Exécution de la requête XPath en cours",
            };
            XpathTimeSpan = stopWatch.Elapsed;
            dialog.DoWork += IrPool.ApplyXpathToPool;
            dialog.RunWorkerCompleted += ApplyXpathToPool_OnComplete;
            dialog.ShowDialog(mainWindow,
                new Tuple<string, XpathQueryParam, Action<int, string>, ColumnDescriptor>(
                    column.XPathExpr,
                    column.Cible,
                    (x, y) => { dialog.ReportProgress(x, "Exécution de la requête XPath en cours", y); },
                    column
                ));
        }

        /// <summary>
        /// Exécute toutes les requêtes XPath entrées pour tous les IRs chargés
        /// </summary>
        public void ApplyAllXPathToPool()
        {
            ProgressDialog dialog = new ProgressDialog
            {
                ShowCancelButton = false,
                Text = "Exécution des requêtes XPath en cours",
            };
            XpathTimeSpan = stopWatch.Elapsed;
            dialog.DoWork += IrPool.ApplyAllXpathToPool;
            dialog.RunWorkerCompleted += ApplyXpathToPool_OnComplete;
            /*dialog.ShowDialog(mainWindow,
                new Tuple<string, XpathQueryParam, Action<int, string>>(
                    query,
                    (XpathQueryParam)(onMeta ? 1 : 0),
                    (x, y) => { dialog.ReportProgress(x, "Exécution des requêtes XPath en cours", y); }
                ));*/
            dialog.ShowDialog(mainWindow,
            new Tuple<Action<int, string>>(
                (x, y) => { dialog.ReportProgress(x, "Exécution des requêtes XPath en cours", y); }
            ));
        }

        /// <summary>
        /// Output et databinding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ApplyXpathToPool_OnComplete(object sender, RunWorkerCompletedEventArgs args)
        {

            if (IrPool == null || IrPool.IRs.Count == 0)
            {
                irConsole.PrintlnError("Pas d'IRs chargés, veuillez charger des IRs avant d'exécuter des requêtes");
                return;
            }
            irConsole.Println("Requête exécutée en " + TimeToString(stopWatch.Elapsed - XpathTimeSpan) + "s");
            UpdateListViewDataContext();
        }
        #endregion

        #region IrListViewSort
        /// <summary>
        /// Trie IrListView par colonne et ordre
        /// </summary>
        /// <param name="column"></param>
        /// <param name="order"></param>
        public void SortLvByColumn(string column, Order order)
        {
            ProgressDialog dialog = new ProgressDialog
            {
                ShowCancelButton = false,
                Text = "Tri de la colonne en cours"
            };
            dialog.DoWork += SortWorker;
            dialog.RunWorkerCompleted += SortDone;
            dialog.ShowDialog(mainWindow, new Tuple<string, Order>(column, order));
        }

        private void SortWorker(object sender, DoWorkEventArgs args)
        {
            Tuple<string, Order> tuple = args.Argument as Tuple<string, Order>;
            Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IrPool.SortList(new IRSorter(tuple.Item1, tuple.Item2));
            }, null)
            .Wait();
        }

        private void SortDone(object sender, RunWorkerCompletedEventArgs args)
        {
            mainWindow.IrListView.DataContext = new AsyncVirtualizingCollection<InstrumentDeRecherche>(
               new IRDataProvider(IrPool),
               IRstat_parametres.pageSize,
               IRstat_parametres.pageTimeout);
        }
        #endregion

        #region GestionColonnes
        private void NewColumnEvent_Handler(ColumnDescriptor columnDescriptor)
        {
            AdvancedGridViewColumn gvc = new AdvancedGridViewColumn
            {
                Header = columnDescriptor.Nom,
                DisplayMemberBinding = new Binding("[" + columnDescriptor.uniqueId + "]")
                {
                    Mode = BindingMode.OneWay
                },
                UniqueId = columnDescriptor.uniqueId
            };
            gvc.AddHeaderPropertyChanged(columnDescriptor);
            mainWindow.lvGridView.Columns.Add(gvc);
            UpdateListViewDataContext();
        }

        private void DeletedColumnEvent_Handler(ColumnDescriptor column)
        {
            mainWindow.lvGridView.Columns.Remove(FindGVCById(column.uniqueId));
            IrPool.Columns.Remove(column);
        }
        #endregion

        #region HelperFunctions
        private GridViewColumn FindGVCById(long id)
        {
            foreach (GridViewColumn column in mainWindow.lvGridView.Columns)
            {
                if (column is AdvancedGridViewColumn advGvc && advGvc.UniqueId == id)
                {
                    return column;
                }
            }
            return null;
        }
        private string TimeToString(TimeSpan t)
        {
            return string.Format("{0:0.0000}", t.TotalSeconds);
        }

        private void UpdateListViewDataContext()
        {
            mainWindow.IrListView.DataContext = new AsyncVirtualizingCollection<InstrumentDeRecherche>(
               new IRDataProvider(IrPool),
               IRstat_parametres.pageSize,
               IRstat_parametres.pageTimeout);
        }

        /// <summary>
        /// Exporte les données contenues dans IrListView vers
        /// </summary>
        public void ExportCurrentDataToCsv()
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
            if (dialog.FileName != "")
            {
                CsvIrExport exportData = new CsvIrExport();
                Dictionary<long, string> ColumnDictionnary = new Dictionary<long, string>();
                foreach (ColumnDescriptor columnDescriptor in ColumnDescriptors)
                {
                    ColumnDictionnary.Add(columnDescriptor.uniqueId, columnDescriptor.Nom);
                }
                foreach (InstrumentDeRecherche ir in IrPool.IRs)
                {
                    exportData.AddRow();
                    exportData.SetCellOnCurrentRow("Nom", ir.Id);
                    exportData.SetCellOnCurrentRow("Titre", ir.Titre);
                    foreach (KeyValuePair<long, string> column in ir.Columns)
                    {
                        exportData.SetCellOnCurrentRow(ColumnDictionnary[column.Key], column.Value);
                    }
                }
                try
                {
                    TimeSpan start = stopWatch.Elapsed;
                    try
                    {
                        exportData.ExportToFile(dialog.FileName, true);
                        TimeSpan time = stopWatch.Elapsed - start;
                        irConsole.Println("Données exportées vers \"" + dialog.FileName + "\" en " + TimeToString(time) + "s");
                    }
                    catch (ArgumentException)
                    {
                        irConsole.PrintlnError("Mauvais chemin de fichier d'export entré");
                    }
                }
                catch (IOException ex)
                {
                    irConsole.Println(ex);
                }
            }
        }

        private void AffChargementIRs(int nbrIRscharges, string path)
        {

            StringBuilder builder = new StringBuilder();
            switch (nbrIRscharges)
            {
                case 0:
                    builder.Append("Aucun IR chargé");
                    break;
                case 1:
                    builder.Append(nbrIRscharges.ToString() + " IR chargé");
                    break;
                default:
                    builder.Append(nbrIRscharges.ToString() + " IRs chargés");
                    break;
            }

            builder.Append(" depuis " + path + " en " + TimeToString(stopWatch.Elapsed - lireDossierTimeSpan) + "s");
            irConsole.Println(builder.ToString());
        }
        #endregion
    }
}