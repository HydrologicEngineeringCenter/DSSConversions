using Hec.Ensemble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_To_DSS
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length != 5)
      {
        Console.WriteLine("Usage: HEFS_To_DSS.exe watershedName startDateTime endDateTime downloadDir dssFileName");
        Console.WriteLine("");
        Console.WriteLine("Example: HEFS_To_DSS.exe RussianNapa \"2021-04-22 12:00\" \"2021-04-22 12:00\" c:\\temp\\downloads output.dss");

        Console.WriteLine("downloads HEFS data and imports to DSS");
      }
      // read from web.
      var watershedName = args[0]; // "RussianNapa";
      var t = DateTime.Parse(args[1]); //new DateTime(2019, 9, 23, 12, 0, 0);
      var t2 = DateTime.Parse(args[2]); //new DateTime(2019, 9, 23, 12, 0, 0);
      var csvDir = args[3]; // @"c:\temp\downloads";
      var dssFileName = args[4];
      HEFS_WebReader.Read(watershedName, t, csvDir);

      // import
      var er = new CsvEnsembleReader(csvDir);
      var ws = er.Read(watershedName, t, t2);
      DssEnsemble.Write(dssFileName, ws);
      

    }
  }
}
