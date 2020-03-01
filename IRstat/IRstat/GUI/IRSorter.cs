using IRStat.Core.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace IRStat.GUI
{
    /// <summary>
    /// TODO: doc
    /// </summary>
    public enum Order
    {
        /// <summary>
        /// TODO: doc
        /// </summary>
        Ascending,
        /// <summary>
        /// TODO: doc
        /// </summary>
        Descending
    }

    static class OrderExtensions
    {
        public static Order Next(this Order current)
        {
            switch (current)
            {
                case Order.Ascending:
                    return Order.Descending;
                case Order.Descending:
                    return Order.Ascending;
                default:
                    throw new Exception("wtf");
            }

        }
    }

    class IRSorter : IComparer<InstrumentDeRecherche>
    {

        public string Column { get; }

        public Order Order { get; }

        public IRSorter(string column, Order order)
        {
            Column = column;
            Order = order;
        }

        public int Compare(InstrumentDeRecherche x, InstrumentDeRecherche y)
        {
            string yValue;
            string xValue;
            if (Column.StartsWith("\n"))
            {
                xValue = x[long.Parse(Column.Substring(1))];
                yValue = y[long.Parse(Column.Substring(1))];
                {
                    long id;
                }
            }
            else
            {
                try
                {
                    PropertyInfo property = typeof(InstrumentDeRecherche).GetProperty(Column);

                    xValue = property.GetValue(x)?.ToString();
                    yValue = property.GetValue(y)?.ToString();
                }
                catch (NullReferenceException e)
                {
                    Trace.WriteLine(e);
                    throw new ArgumentException("Bad Column Name");
                }
            }
            if (xValue != null && yValue == null)
            {
                return -1;
            }
            else if (xValue == null)
            {
                return yValue != null ? 1 : 0;
            }

            int preresult = xValue.CompareTo(yValue);

            if (Order == Order.Descending)
            {
                preresult *= -1;
            }
            return preresult;
        }
    }
}
