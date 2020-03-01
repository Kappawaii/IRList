using DataVirtualization;
using IRStat.Core.Data;
using System;
using System.Collections.Generic;

namespace IRStat.GUI
{
    class IRDataProvider : IItemsProvider<InstrumentDeRecherche>
    {
        readonly List<InstrumentDeRecherche> list;

        public IRDataProvider(IRPool iRPool)
        {
            list = new List<InstrumentDeRecherche>(iRPool?.IRs) ?? throw new ArgumentNullException(nameof(iRPool));
        }

        public IRDataProvider(List<InstrumentDeRecherche> list)
        {
            this.list = list ?? throw new ArgumentNullException(nameof(list));
        }

        public int FetchCount()
        {
            return list.Count;
        }

        public IList<InstrumentDeRecherche> FetchRange(int startIndex, int count)
        {
            return list.GetRange(startIndex, Math.Min(count, list.Count - startIndex));
        }
    }
}