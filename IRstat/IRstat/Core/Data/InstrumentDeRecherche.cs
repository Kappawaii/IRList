using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Xml.XPath;

namespace IRStat.Core.Data
{
    /// <summary>
    /// TODO: doc
    /// </summary>
    [Serializable]
    class InstrumentDeRecherche : INotifyPropertyChanged
    {

        public string Id { get; private set; }
        public Valide IsValid { get; private set; }

        public string irXmlPath;
        public string metaXmlPath;

        [NonSerialized]
        private XPathDocument _IrXml;

        [NonSerialized]
        private XPathDocument _MetaXml;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Titre { get; set; }
        public string XPath_Resultat { get; set; }


        public string Tooltip => GetTooltipValue(IsValid);
        public Brush MessageColor => GetColor(IsValid);

        public Dictionary<long, string> Columns { get; }

        /// <summary>
        /// indexeur pour Columns
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string this[long uid]
        {
            get
            {
                Columns.TryGetValue(uid, out string value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Columns)));
                if (value == null)
                {
                    return "";
                }
                return value;
            }

            set
            {
                if (!Columns.ContainsKey(uid))
                {
                    Columns.Add(uid, value);
                }
                else
                {
                    Columns[uid] = value;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Columns)));
            }
        }

        public XPathDocument IrXml
        {
            get
            {
                if (_IrXml == null)
                {
                    _IrXml = new XPathDocument(irXmlPath);
                }
                return _IrXml;
            }
            private set
            {
                _IrXml = value;
            }
        }

        public XPathDocument MetaXml
        {
            get
            {
                if (_MetaXml == null)
                {
                    _MetaXml = new XPathDocument(metaXmlPath);
                }
                return _MetaXml;
            }
            private set
            {
                _MetaXml = value;
            }
        }

        /// <summary>
        /// TODO: doc
        /// </summary>
        /// <param name="irXmlPath"></param>
        /// <param name="metaXmlPath"></param>
        /// <exception>NomException</exception>
        public InstrumentDeRecherche(string irXmlPath, string metaXmlPath)
        {
            this.irXmlPath = irXmlPath;
            this.metaXmlPath = metaXmlPath;
            IsValid = Valide.OK;
            Columns = new Dictionary<long, string>();
        }

        /// <summary>
        /// TODO: doc
        /// </summary>
        /// <param name="irNameQuery"></param>
        /// <param name="metaNameQuery"></param>
        /// <param name="metaTitreQuery"></param>
        public void Init(string irNameQuery, string metaNameQuery, string metaTitreQuery)
        {
            XPathDocument metaXmldoc = new XPathDocument(metaXmlPath);
            XPathDocument irXmldoc = null;
            string nomIr = null;
            string nomMeta = null;
            if (metaXmldoc != null)
            {

                XPathNavigator navMeta = metaXmldoc.CreateNavigator();
                XPathNodeIterator iteratorMeta = navMeta.Select(metaNameQuery);
                if (iteratorMeta.MoveNext())
                {
                    nomMeta = iteratorMeta.Current.Value;
                }
                XPathNodeIterator iteratorMeta2 = navMeta.Select(metaTitreQuery);
                if (iteratorMeta2.MoveNext())
                {
                    Titre = iteratorMeta2.Current.Value;
                }
            }
            else
            {
                IsValid = Valide.noMeta;
            }

            if (IsValid == Valide.noMeta && irXmldoc != null)
            {
                irXmldoc = new XPathDocument(irXmlPath);
                XPathNodeIterator iteratorIr = irXmldoc.CreateNavigator().Select(irNameQuery);
                if (iteratorIr.MoveNext())
                {
                    nomIr = iteratorIr.Current.Value;
                }
            }
            else
            {
                if (IsValid.Equals(Valide.noMeta))
                {
                    IsValid = Valide.Invalid;
                }
                if (!string.IsNullOrEmpty(nomMeta))
                {
                    Id = nomMeta;
                    return;
                }
            }

            if (IsValid != Valide.noIR && !string.IsNullOrEmpty(nomIr))
            {
                Id = nomIr;
            }
            else if (IsValid != Valide.noMeta && !string.IsNullOrEmpty(nomMeta))
            {
                Id = nomMeta;
            }
            Id = nomMeta;
        }

        /// <summary>
        /// "Libère" le XPathDocument pour le GC
        /// </summary>
        public void ClearCache()
        {
            _IrXml = null;
            _MetaXml = null;
        }

        private Brush GetColor(Valide isValid)
        {
            switch (isValid)
            {
                case Valide.OK:
                    return Brushes.Green;
                case Valide.noIR:
                    return Brushes.IndianRed;
                case Valide.noMeta:
                    return Brushes.OrangeRed;
                case Valide.Invalid:
                    return Brushes.DarkRed;
                default:
                    Debug.WriteLine("Null Valide value");
                    return null;
            }
        }

        private string GetTooltipValue(Valide valide)
        {
            switch (valide)
            {
                case Valide.OK:
                    return "IR valide";
                case Valide.noIR:
                    return "Méta seul";
                case Valide.noMeta:
                    return "Pas de méta";
                case Valide.Invalid:
                    return "Erreur de lecture";
                default:
                    throw new NotImplementedException("wtf");
            }
        }
    }

}
