using DataVirtualization;
using IRStat.Core;
using IRStat.Core.Data;
using IRStat.GUI;
using IRStat.IO;
using IRStat.IO.Exports;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace IRStat
{
    /// <summary>
    /// TODO: write summary
    /// </summary>

    public partial class App : Application
    {
        private GestionFichiers gestionFichiers;
        IRConsole irConsole;
        private MainWindow mainWindow;
        IRPool iRpool;

        readonly Stopwatch stopWatch = new Stopwatch();

        private TimeSpan lireDossierTimeSpan;

        /// <summary>
        /// initialisation du modèle
        /// </summary>
        /// <param name="mw"></param>
        public void Init(MainWindow mw)
        {
            mainWindow = mw;
            irConsole = new IRConsole(mw.ConsoleOutputTextBlock);
            iRpool = new IRPool(irConsole);
            gestionFichiers = new GestionFichiers(irConsole);
            gestionFichiers.LoadFinished += AfficherIRs_new;
            stopWatch.Start();
        }

        /// <summary>
        /// Lit les Instruments de recherche contenus dans un dossier
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        public void LireDossier(string path, bool recursive)
        {
            //succès
            try
            {
                //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                lireDossierTimeSpan = stopWatch.Elapsed;
                iRpool.Clear();
                gestionFichiers.OpenAllXmlFilesFromDirectory(path, recursive);
            }
            catch (FileNotFoundException e)
            {
                irConsole.Println(e);
            }

        }
        private void AfficherIRs_new(object sender, SearchForFiles_Args args)
        {
            iRpool.LoadIRs(gestionFichiers.IRList);
            mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                UpdateListViewDataContext();
                mainWindow.nombreIRTextBlock.Text = "(" + gestionFichiers.IRList.Count.ToString() + ")";
                //irConsole.Println(iRpool.instrumentsDeRecherche.Count + " IRs chargés en " + TimeToString(stopWatch.Elapsed) + " s");
                AffChargementIRs(iRpool.instrumentsDeRecherche.Count, mainWindow.pathTextBox.Text);
            }, null);
        }

        //AfficherIRs(sender, args);

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

            builder.Append(" depuis " + path + " en " + TimeToString(lireDossierTimeSpan) + "s");
            irConsole.Println(builder.ToString());
        }

        /// <summary>
        /// TODO: doc
        /// </summary>
        /// <param name="query"></param>
        /// <param name="onMeta"></param>
        public void ApplyXpathToPool(string query, bool onMeta)
        {
            TimeSpan start = stopWatch.Elapsed;
            if (iRpool == null)
            {
                irConsole.PrintlnError("Pas d'IRs chargés, veuillez charger des IRs avant d'exécuter des requêtes");
            }
            iRpool?.ApplyXpathToPool(query, (XpathQueryParam)(onMeta ? 1 : 0));
            TimeSpan end = stopWatch.Elapsed;
            irConsole.Println("Requête exécutée en " + TimeToString(end - start) + "s");
            UpdateListViewDataContext();
        }

        private void UpdateListViewDataContext()
        {
            mainWindow.IrListView.DataContext = new AsyncVirtualizingCollection<InstrumentDeRecherche>(
               new IRDataProvider(iRpool),
               IRstat_parametres.pageSize,
               IRstat_parametres.pageTimeout);
        }

        /// <summary>
        /// TODO:doc
        /// </summary>
        public void ExportCurrentDataToCsv()
        {
            CsvIrExport exportData = new CsvIrExport();
            foreach (InstrumentDeRecherche ir in iRpool.instrumentsDeRecherche)
            {
                exportData.AddRow();

                exportData.SetRow("Nom", ir.Nom);
                exportData.SetRow("Titre", ir.Titre);
                exportData.SetRow("Valide", ir.IsValid);
                exportData.SetRow("XPath", ir.XPathResultat);
            }
            try
            {
                TimeSpan start = stopWatch.Elapsed;
                exportData.ExportToFile(mainWindow.exportPathTextBox.Text, true);
                TimeSpan time = stopWatch.Elapsed - start;
                irConsole.Println("Données exportées vers \"" + mainWindow.exportPathTextBox.Text + "\" en " + TimeToString(time) + "s");
            }
            catch (IOException ex)
            {
                irConsole.Println(ex);
            }

        }

        private string TimeToString(TimeSpan t)
        {
            return string.Format("{0:0.0000}", t.TotalSeconds);
        }
    }
}