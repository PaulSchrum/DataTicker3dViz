using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DataTicker3D;

namespace DataTicker3dViz
{
   class TrafficDataFileReader : ITimeTickerDataProvider
   {
      public String FileName { get; set; }
      public String RecordingDeviceID { get; set; }
      public TrafficDataFileReader(String pathAndFileName)
      {
         FileName = pathAndFileName;
      }

      public SortedDictionary<DateTime, Double> getData()
      {
         if (validateFileName() == false)
            return null;

         var returnDict = new SortedDictionary<DateTime, Double>();

         String line; String[] parsedLine; char[] delimChar = { ',' };
         using (System.IO.StreamReader file = new System.IO.StreamReader(FileName))
         {
            while ((line = file.ReadLine()) != null)
            {
               parsedLine = line.Split(delimChar);
               if (parsedLine[0] == RecordingDeviceID)
               {
                  DateTime date;
                  if (DateTime.TryParse(parsedLine[1], out date) == true)
                  {
                     Double numberOfCars;
                     if (Double.TryParse(parsedLine[4], out numberOfCars) == true)
                     {
                        if (returnDict.ContainsKey(date) == false)
                        {
                           returnDict.Add(date, numberOfCars);
                        }
                        else
                        {
                           returnDict[date] += numberOfCars;
                        }
                     }
                  }
               }
            }
         }
         return returnDict;
      }

      public List<SortedDictionary<DateTime, Double>> getDataList()
      {
         var retval = new List<SortedDictionary<DateTime, double>>();
         retval.Add(getData());
         //return retval;
      }

      private bool validateFileName()
      {
         if (FileName == null) return false;
         if (FileName.Length == 0) return false;
         if (File.Exists(FileName) == false) return false;
         return true;
      }
   }
}
