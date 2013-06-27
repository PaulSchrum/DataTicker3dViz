using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTicker3D
{
   public interface ITimeTickerDataProvider
   {
      SortedDictionary<DateTime, Double> getData();
      List<SortedDictionary<DateTime, Double>> getDataList();
   }
}
