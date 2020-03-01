using System;
using System.Text;
using System.Xml.XPath;

namespace IRStat.Core
{
    static class XPathQueryRunner
    {
        /// <summary>
        /// TODO: doc
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="query"></param>
        /// <exception cref="XPathException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        /// 
        public static string RunQuery(XPathDocument doc, string query) 
        {
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator iterator = nav.Select(query);
            StringBuilder builder = new StringBuilder();
            bool firstLoop = true;

            while (iterator.MoveNext())
            {
                builder.Append(iterator.Current.InnerXml);
                if (!firstLoop)
                {
                    builder.Append(";");
                }
                else
                {
                    firstLoop = false;
                }
            }

            return builder.ToString();
        }

    }
}
