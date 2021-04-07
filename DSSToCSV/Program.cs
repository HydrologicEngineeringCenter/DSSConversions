using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hec.Dss;

namespace DSSToCSV
{
  class Program
  {
    static void Main(string[] args)
    {
      using (Hec.Dss.DssReader r = new DssReader(@"C:\project\DSSConversions\DSSToCSV\data\1PcAEP_360min_5145.dss"))
      {
        var s = r.GetTimeSeries(new DssPath("//Subbasin-1C2/FLOW//1MIN/RUN:1PcAEP_360min_5145/"));
        var table = s.ToDataTable();
        CsvFile.Write(table, "1PcAEP_360min_5145.csv");
      }

    }

  }
}
