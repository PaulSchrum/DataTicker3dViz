using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DataTicker3D;

namespace DataTicker3dViz
{
   public class USGS_LakeLevelTxtFileReader : ITimeTickerDataProvider
   {
      public String FileName { get; set; }
      public USGS_LakeLevelTxtFileReader(String pathAndFileName)
      {
         FileName = pathAndFileName;
      }

      public SortedDictionary<DateTime, Double> getData()
      {
         if (validateFileName() == false)
            return null;

         var returnDict = new SortedDictionary<DateTime, Double>();

         String line; String[] parsedLine; char[] tabChar = { '\t' };
         using (System.IO.StreamReader file = new System.IO.StreamReader(FileName))
         {
            while ((line = file.ReadLine()) != null)
            {
               if (line[0] == '#') continue;
               parsedLine = line.Split(tabChar);
               if (parsedLine[0] == "USGS")
               {
                  DateTime date;
                  if (DateTime.TryParse(parsedLine[2], out date) == true)
                  {
                     Double elevation;
                     if (Double.TryParse(parsedLine[3], out elevation) == true)
                     {
                        returnDict.Add(date, elevation);
                     }
                  }
               }
            }
         }
         return returnDict;
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
