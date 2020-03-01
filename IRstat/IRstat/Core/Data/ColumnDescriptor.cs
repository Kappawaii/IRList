using System;
using System.ComponentModel;

namespace IRStat.Core.Data
{
    /// <summary>
    /// TODO: doc
    /// </summary>
    [Serializable]
    public class ColumnDescriptor : INotifyPropertyChanged
    {
        /// <summary>
        /// Id unique
        /// </summary>
        public readonly long uniqueId;

        private string nom;

        /// <summary>
        /// TODO: doc
        /// </summary>
        public XpathQueryParam Cible { get; set; }

        /// <summary>
        /// TODO: doc
        /// </summary>
        public bool IsMeta { get => Cible.Equals(XpathQueryParam.onMeta); }

        /// <summary>
        /// TODO: doc
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Nom de la colonne
        /// </summary>
        public string Nom
        {
            get
            {
                return nom;
            }
            set
            {
                nom = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nom)));
            }
        }
        /// <summary>
        /// Expression XPath à exécuter sur la colonne
        /// </summary>
        public string XPathExpr { get; set; }

        /// <summary>
        /// TODO: doc
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="xPathExpr"></param>
        /// <exception cref="ArgumentException">si un des paramètres null</exception>
        public ColumnDescriptor(string nom, string xPathExpr)
        {
            XPathExpr = xPathExpr ?? throw new ArgumentNullException(nameof(xPathExpr));
            Nom = nom ?? throw new ArgumentNullException(nameof(Nom));
            byte[] buf = new byte[64];
            new Random().NextBytes(buf);
            uniqueId = BitConverter.ToInt64(buf, 0);
            Cible = XpathQueryParam.onMeta;
        }
    }
}

/*
/// <summary>
/// "Triaire" -> null pour pas de tri, true pour afficher les IRs qui remplissent la condition
/// et false pour afficher les IRs qui ne remplissent pas les conditions
/// </summary>
public bool? Filter { get; set; }
/// <summary>
/// Expression régulière pour le tri
/// </summary>
public string FilterExpr { get; set; }
*/
