using IRStat.GUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.XPath;

namespace IRStat.Core.Data
{
    /// <summary>
    /// TODO: doc
    /// </summary>
    public enum XpathQueryParam
    {
        /// <summary>
        /// TODO: doc
        /// </summary>
        onIR,
        /// <summary>
        /// TODO: doc
        /// </summary>
        onMeta
    }

    [Serializable]
    class IRPool
    {

        public BindingList<InstrumentDeRecherche> IRs { get; private set; }

        [NonSerialized]
        public readonly IRConsole irConsole;
        public BindingList<ColumnDescriptor> Columns { get; private set; }
        public BindingList<ColumnDescriptor> ColumnsCopy { get; private set; }

        public delegate void NewColumnEventHandler(ColumnDescriptor column);
        public event NewColumnEventHandler NewColumnEvent;

        public delegate void DeletedColumnEventHandler(ColumnDescriptor column);
        public event DeletedColumnEventHandler DeletedColumnEvent;

        public IRPool(IRConsole irConsole)
        {
            this.irConsole = irConsole;
            IRs = new BindingList<InstrumentDeRecherche>();
            Columns = new BindingList<ColumnDescriptor>();
            ColumnsCopy = new BindingList<ColumnDescriptor>();
            Columns.ListChanged += AddingNew_Event;
        }

        private void AddingNew_Event(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                ColumnDescriptor newElement = Columns[e.NewIndex];
                NewColumnEvent?.Invoke(newElement);
                ColumnsCopy.Add(newElement);
            }
            else if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                DeletedColumnEvent?.Invoke(ColumnsCopy[e.NewIndex]);
                ColumnsCopy.RemoveAt(e.NewIndex);
            }
        }

        public void Clear()
        {
            IRs.Clear();
        }

        public void LoadIRs(List<InstrumentDeRecherche> instrumentsDeRecherche)
        {
            foreach (InstrumentDeRecherche instrumentDeRecherche in instrumentsDeRecherche)
            {
                IRs.Add(instrumentDeRecherche);
            }
        }
        public void ApplyXpathToPool(object sender, DoWorkEventArgs args)
        {
            Tuple<string, XpathQueryParam, Action<int, string>, ColumnDescriptor> tuple = args.Argument as Tuple<string, XpathQueryParam, Action<int, string>, ColumnDescriptor>;
            string xPathQuery = tuple.Item1;
            XpathQueryParam param = tuple.Item2;
            object sync = new object();
            try
            {
                int total = IRs.Count;
                int current = 0;

                ColumnDescriptor column = tuple.Item4;

                Parallel.ForEach(IRs, ir =>
                {
                    ir.Columns[column.uniqueId] = XPathQueryRunner.RunQuery(GetDestination(ir, column.Cible), column.XPathExpr);

                    if (param == XpathQueryParam.onIR)
                    {
                        ir.ClearCache();
                    }
                    lock (sync)
                    {
                        int avancement = ++current * 100 / total;
                        tuple.Item3(avancement, "Avancement: " + avancement + "%");
                    }
                });
            }
            catch (AggregateException e)
            {
                e.Handle((x) =>
                {
                    if (x is XPathException ex)
                    {
                        App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            irConsole.PrintlnError("Erreur XPath à " + DateTime.Now.ToString("h:mm:ss") + " : " + ex.Message);
                        }, null).Wait();
                    }
                    return false;
                });
                if (e.Message == "L'expression doit être évaluée pour donner une collection de noeuds.")
                {
                    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        irConsole.PrintlnError("erreur de syntaxe XPath, vérifiez la requête (" + e.Message + ")");
                    }, null).Wait();
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void ApplyAllXpathToPool(object sender, DoWorkEventArgs args)
        {
            Action<int, string> callback = args.Argument as Action<int, string>;
            object sync = new object();
            try
            {
                int total = IRs.Count;
                int current = 0;
                Parallel.ForEach(IRs, ir =>
                {
                    bool clear = false;
                    //obsolète
                    //ir.XPath_Resultat = XPathQueryRunner.RunQueryMulti(GetDestination(ir, param), xPathQuery);
                    foreach (ColumnDescriptor column in Columns)
                    {
                        ir.Columns[column.uniqueId] = XPathQueryRunner.RunQuery(GetDestination(ir, column.Cible), column.XPathExpr);
                        Trace.WriteLine(ir.Columns[column.uniqueId]);
                        if (column.Cible == XpathQueryParam.onIR)
                        {
                            clear = true;
                        }
                    }
                    if (clear)
                    {
                        ir.ClearCache();
                    }
                    lock (sync)
                    {
                        int avancement = ++current * 100 / total;
                        callback(avancement, "Avancement: " + avancement + "%");
                    }
                });
            }
            catch (AggregateException e)
            {
                e.Handle((x) =>
                {
                    if (x is XPathException ex)
                    {
                        App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            irConsole.PrintlnError("Erreur XPath à " + DateTime.Now.ToString("h:mm:ss") + " : " + ex.Message);
                        }, null).Wait();
                    }
                    return false;
                });
                if (e.Message == "L'expression doit être évaluée pour donner une collection de noeuds.")
                {
                    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        irConsole.PrintlnError("erreur de syntaxe XPath, vérifiez la requête (" + e.Message + ")");
                    }, null).Wait();
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private XPathDocument GetDestination(InstrumentDeRecherche ir, XpathQueryParam param)
        {
            switch (param)
            {
                case XpathQueryParam.onIR:
                    return ir.IrXml;
                case XpathQueryParam.onMeta:
                    return ir.MetaXml;
                default:
                    return null;
            }
        }

        public void SortList(IRSorter sorter)
        {
            try
            {
                IRs = new BindingList<InstrumentDeRecherche>(IRs.OrderBy(x => x, sorter).ToList());
            }
            catch (ArgumentException e)
            {
                irConsole.PrintlnError("Erreur du comparateur : " + e);
            }
        }
    }
}
