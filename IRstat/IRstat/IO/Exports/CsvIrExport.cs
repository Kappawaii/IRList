using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;

namespace IRStat.IO.Exports
{
    /// <summary>
    /// Classe D'export d'IRs vers un fichier csv
    /// </summary>
    public class CsvIrExport
    {
        /// <summary>
        /// To keep the ordered list of column names
        /// </summary>
        readonly List<string> _fields = new List<string>();

        /// <summary>
        /// The list of rows
        /// </summary>
        readonly List<Dictionary<string, object>> _rows = new List<Dictionary<string, object>>();

        /// <summary>
        /// The current row
        /// </summary>
        Dictionary<string, object> CurrentRow { get { return _rows.Last(); } }

        /// <summary>
        /// The string used to separate columns in the output
        /// </summary>
        private readonly string _columnSeparator;

        /// <summary>
        /// Whether to include the preamble that declares which column separator is used in the output
        /// </summary>
        private readonly bool _includeColumnSeparatorDefinitionPreamble;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvIrExport"/> class.
        /// </summary>
        /// <param name="columnSeparator">
        /// The string used to separate columns in the output.
        /// By default this is a comma so that the generated output is a CSV file.
        /// </param>
        /// <param name="includeColumnSeparatorDefinitionPreamble">
        /// Whether to include the preamble that declares which column separator is used in the output.
        /// By default this is <c>true</c> so that Excel can open the generated CSV
        /// without asking the user to specify the delimiter used in the file.
        /// </param>
        public CsvIrExport(string columnSeparator = ",", bool includeColumnSeparatorDefinitionPreamble = true)
        {
            _columnSeparator = columnSeparator;
            _includeColumnSeparatorDefinitionPreamble = includeColumnSeparatorDefinitionPreamble;
        }

        /// <summary>
        /// Set a value on this column
        /// </summary>
        public void SetCellOnCurrentRow(string field, object value)
        {
            // Keep track of the field names, because the dictionary loses the ordering
            if (!_fields.Contains(field))
            {
                _fields.Add(field);
            }
            CurrentRow[field] = value;
        }

        /// <summary>
        /// Call this before setting any fields on a row
        /// </summary>
        public void AddRow()
        {
            _rows.Add(new Dictionary<string, object>());
        }

        /// <summary>
        /// Add a list of typed objects, maps object properties to CsvFields
        /// </summary>
        public void AddRows<T>(IEnumerable<T> list)
        {
            foreach (var obj in list)
            {
                AddRow();
                foreach (var value in obj.GetType().GetProperties())
                {
                    SetCellOnCurrentRow(value.Name, value.GetValue(obj, null));
                }
            }
        }

        /// <summary>
        /// Converts a value to how it should output in a csv file
        /// If it has a comma, it needs surrounding with double quotes
        /// Eg Sydney, Australia -> "Sydney, Australia"
        /// Also if it contains any double quotes ("), then they need to be replaced with quad quotes[sic] ("")
        /// Eg "Dangerous Dan" McGrew -> """Dangerous Dan"" McGrew"
        /// </summary>
        /// <param name="value">
        /// The string used to separate columns in the output.
        /// By default this is a comma so that the generated output is a CSV document.
        /// </param>
        public static string MakeValueCsvFriendly(object value)
        {
            if (value == null || value is INullable valueNullable && valueNullable.IsNull)
            {
                return "";
            }
            if (value is DateTime dt)
            {
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            string output = value.ToString().Trim();

            if (output.Length > 30000) //cropping value for stupid Excel
                output = output.Substring(0, 30000);

            return '"' + output.Replace("\"", "\"\"") + '"';
        }

        /// <summary>
        /// Outputs all rows as a CSV, returning one string at a time
        /// </summary>
        private IEnumerable<string> ExportToLines(bool includeHeader = false)
        {
            if (_includeColumnSeparatorDefinitionPreamble) yield return "sep=" + _columnSeparator;

            // The header
            if (includeHeader)
            {
                yield return string.Join(_columnSeparator, _fields.Select(f => MakeValueCsvFriendly(f)));
            }

            // The rows
            foreach (Dictionary<string, object> row in _rows)
            {
                foreach (string k in _fields.Where(f => !row.ContainsKey(f)))
                {
                    row[k] = null;
                }
                yield return string.Join(_columnSeparator, _fields.Select(field => MakeValueCsvFriendly(row[field])));
            }
        }

        /// <summary>
        /// Output all rows as a CSV returning a string
        /// </summary>
        public string Export(bool includeHeader = false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in ExportToLines(includeHeader))
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Exports to a file
        /// </summary>
        public void ExportToFile(string path, bool includeHeader = false)
        {
            IEnumerable<string> strings = ExportToLines(includeHeader);
            File.WriteAllLines(path, strings, Encoding.Default);
        }

        /// <summary>
        /// Exports as raw UTF8 bytes
        /// </summary>
        public byte[] ExportToBytes(bool includeHeader = false)
        {
            var data = Encoding.UTF8.GetBytes(Export(includeHeader));
            return Encoding.UTF8.GetPreamble().Concat(data).ToArray();
        }
    }
}