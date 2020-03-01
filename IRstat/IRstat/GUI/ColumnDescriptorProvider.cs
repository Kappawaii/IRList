using DataVirtualization;
using IRStat.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace IRStat.GUI
{
    /// <summary>
    /// TODO: doc
    /// </summary>
    public class ColumnDescriptorProvider : IItemsProvider<ColumnDescriptor>
    {
        /// <summary>
        /// TODO:doc
        /// </summary>
        public BindingList<ColumnDescriptor> Columns { get; private set; }

        /// <summary>
        /// TODO: doc
        /// </summary>
        public ColumnDescriptorProvider(BindingList<ColumnDescriptor> columns)
        {
            Columns = columns;
        }

        /// <summary>
        /// TODO: doc
        /// </summary>
        public void AddColumn(ColumnDescriptor column)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }
            else
            {
                Columns.Add(column);
            }
        }

        /// <summary>
        /// TODO: doc
        /// </summary>
        /// <param name="column"></param>
        public void DeleteColumn(ColumnDescriptor column)
        {
            if (column is null)
            {
                Trace.WriteLine("null delete :" + nameof(column));
            }
            else
            {
                Columns.Remove(column);
            }
        }

        /// <summary>
        /// TODO: doc
        /// </summary>
        /// <returns></returns>
        public int FetchCount()
        {
            return Columns.Count;
        }

        /// <summary>
        /// TODO: doc
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IList<ColumnDescriptor> FetchRange(int startIndex, int count)
        {
            return Columns.ToList().GetRange(startIndex, Math.Min(count, Columns.Count - startIndex));
        }
    }
}
