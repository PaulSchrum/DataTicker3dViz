using DataTicker3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTicker3dViz
{
    class TrigFunctionsCsv : ITimeTickerDataProvider
    {
        public String FileName { get; set; }

        public TrigFunctionsCsv(String pathAndFileName)
        {
            FileName = pathAndFileName;
        }

        public SortedDictionary<DateTime, double> getData()
        {
            if(validateFileName() == false)
                return null;

            var returnDict = new SortedDictionary<DateTime, Double>();

            String line; String[] parsedLine; char[] delimChar = { ',' };
            using(System.IO.StreamReader file = new System.IO.StreamReader(FileName))
            {
                while((line = file.ReadLine()) != null)
                {
                    parsedLine = line.Split(delimChar);
                    try
                    {
                        var x = Convert.ToDouble(parsedLine[0]);
                        var y1 = Convert.ToDouble(parsedLine[1]);
                        var y2 = Convert.ToDouble(parsedLine[2]);
                        var newDT = DateTime.FromOADate(x);

                    }
                    catch (FormatException fe)
                    { }
                }

            }

            return returnDict;
        }

        public List<SortedDictionary<DateTime, double>> getDataList()
        {
            if(validateFileName() == false)
                return null;

            var sineDict = new SortedDictionary<DateTime, Double>();
            var cosineDict = new SortedDictionary<DateTime, Double>();

            String line; String[] parsedLine; char[] delimChar = { ',' };
            using(System.IO.StreamReader file = new System.IO.StreamReader(FileName))
            {
                while((line = file.ReadLine()) != null)
                {
                    parsedLine = line.Split(delimChar);
                    try
                    {
                        var x = Convert.ToDouble(parsedLine[0]);
                        var y1 = Convert.ToDouble(parsedLine[1]);
                        var y2 = Convert.ToDouble(parsedLine[2]);
                        var newDT = DateTime.FromOADate(x);

                        sineDict[newDT] = y1;
                        cosineDict[newDT] = y2;
                    }
                    catch(FormatException fe)
                    { }
                }

            }

            var retval = new List<SortedDictionary<DateTime, double>>();
            retval.Add(sineDict);
            retval.Add(cosineDict);
            return retval;
        }

        private bool validateFileName()
        {
            if(FileName == null) return false;
            if(FileName.Length == 0) return false;
            if(File.Exists(FileName) == false) return false;
            return true;
        }
    }
}
