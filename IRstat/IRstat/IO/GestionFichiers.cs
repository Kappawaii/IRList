using IRStat.Core.Data;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IRStat.IO
{
    struct SearchForFiles_Args
    {
        public string dirPath;
        public bool recursive;
    }

    class GestionFichiers
    {
        public delegate void OnLoadingFinished(object sender, SearchForFiles_Args args);
        public event OnLoadingFinished LoadFinished;

        public List<InstrumentDeRecherche> IRList { get; private set; }
        public List<InstrumentDeRecherche> NotValidList { get; private set; }

        private string[] xmlMetaNoms;
        public string[] Noms { get; private set; }
        private static ProgressDialog dialog;

        private SearchForFiles_Args search_args;

        private int progress;
        public GestionFichiers()
        {
            IRList = new List<InstrumentDeRecherche>();
        }

        private void SearchForFiles(object sender, DoWorkEventArgs args)
        {
            SearchForFiles_Args search_args = (SearchForFiles_Args)args.Argument;

            SearchOption searchOption = search_args.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            string[] xmlFilenames = Directory.GetFiles(search_args.dirPath, "*.xml", searchOption);

            Noms = (from xmlFilename in xmlFilenames
                    where !xmlFilename.Contains(IRstat_parametres.xmlPlainFilenameDoesNotContains)
                    select xmlFilename).ToArray();
            xmlMetaNoms = (from xmlFilename in xmlFilenames
                           where xmlFilename.EndsWith(IRstat_parametres.xmlMetaFileNameEndsWith)
                           select xmlFilename).ToArray();
        }

        private void LoadFiles(object sender, RunWorkerCompletedEventArgs args)
        {
            //Nettoie la liste
            IRList.Clear();

            dialog = new ProgressDialog
            {
                ShowCancelButton = false,
                Text = "Chargement des fichiers XML en cours"
            };
            dialog.DoWork += LoaderWorker;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Ouvre tous les fichiers xml (suivant les règles inscrites dans IRstat_settings) d'un dossier,
        /// ainsi que leurs métadonnées associées.
        /// </summary>
        /// <param name="owner">fenêtre à bloquer</param>
        /// <param name="dirPath">le chemin du dossier visé</param>
        /// <param name="recursive">inclure les sous dossiers ?</param>
        /// <returns></returns>
        public void OpenAllXmlFilesFromDirectory(Window owner, string dirPath, bool recursive)
        {

            //"packaging" des arguments dans une structure
            search_args = new SearchForFiles_Args
            {
                dirPath = dirPath,
                recursive = recursive
            };

            //création d'un ProgressDialog
            dialog = new ProgressDialog
            {
                ShowCancelButton = false,
                Text = "Recherche des fichiers IR en cours",
            };
            dialog.DoWork += SearchForFiles;
            dialog.RunWorkerCompleted += LoadFiles;
            dialog.ShowDialog(owner, search_args);
        }

        private void LoaderWorker(object sender, DoWorkEventArgs args)
        {
            if (Noms.Length > 0)
            {
                Thread progressReporter = new Thread(ReportProgressworker);
                progressReporter.Start();
                object someListLock = new object();
                Parallel.For(0, Noms.Length,
                   i =>
                   {
                       InstrumentDeRecherche ir = new InstrumentDeRecherche(Noms[i], xmlMetaNoms[i]);
                       ir.Init(IRstat_parametres.XpathIrNom, IRstat_parametres.XpathMetaNom, IRstat_parametres.XpathMetaTitre);
                       lock (someListLock)
                       {
                           IRList.Add(ir);
                           progress++;
                       }
                   });
                Console.WriteLine("Loading Done");
            }
            LoadFinished(this, search_args);
        }

        private void ReportProgressworker()
        {
            do
            {
                int count = Noms.Count();
                Thread.Sleep(5);
                int progressReported = Math.Min(progress * 100 / count, 100);
                try
                {
                    if (progressReported == 100 || dialog.CancellationPending)
                    {
                        return;
                    }
                    dialog.ReportProgress(
                        progressReported,
                        null,
                        string.Format(System.Globalization.CultureInfo.CurrentCulture,
                        "Avancement: {0}% ({1}/{2})",
                        progressReported, progress, count));
                }
                catch (InvalidOperationException)
                {
                    Trace.WriteLine("catched in GestionFichiers");
                    return;
                }

            } while (progress < Noms.Length - 1);
        }
    }
}