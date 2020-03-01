using IRStat.Core.Data;
using System.ComponentModel;
using System.Windows.Controls;

namespace IRStat.GUI
{
    class AdvancedGridViewColumn : GridViewColumn
    {
        public long UniqueId { get; set; }

        public void AddHeaderPropertyChanged(ColumnDescriptor columnDesciptor)
        {
            columnDesciptor.PropertyChanged += OnHeaderPropertyChanged;
        }

        private void OnHeaderPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender is ColumnDescriptor columnDescriptor && args.PropertyName == "Nom")
            {
                base.Header = columnDescriptor.Nom;
                base.Width = base.ActualWidth;
                base.Width = double.NaN;
            }
        }
    }
}
