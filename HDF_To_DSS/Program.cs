using H5Assist;
using Hec.Dss;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDF_To_DSS
{
  class Program
  {
    public static string Path(params string[] items)
    {
      return String.Join(H5Reader.PathSeparator, items);
    }


    static void Main(string[] args)
    {
      if(  args.Length < 2 || args.Length > 3)
      {
        Console.WriteLine("Usage:hdf_to_dss.exe input.hdf  output.dss [debug|quiet]");
        return;
      }
      
      string interval = "1Hour";
      DateTime t = DateTime.Parse("1-1-2000 1:00 am");
      string fnDss = args[1];
      string fnHDF = args[0]; //@"C:\project\HDF_To_DSS\HDF_To_DSS\SampleData100Years.wg";
      Stopwatch sW = new Stopwatch();
      sW.Start();
      //add dss 6 example   

      Hec.Dss.DssWriter dss;
      if (args.Length == 3 && args[2] == "debug")
      {
        dss = new Hec.Dss.DssWriter(fnDss, DssReader.MethodID.MESS_METHOD_GENERAL_ID,
                                           DssReader.LevelID.MESS_LEVEL_INTERNAL_DIAG_2);
      }
      else if(args.Length == 3 && args[2] == "quiet")
      {
        dss = new Hec.Dss.DssWriter(fnDss, DssReader.MethodID.MESS_METHOD_GENERAL_ID,
                                      DssReader.LevelID.MESS_LEVEL_NONE);

      }

      dss = new Hec.Dss.DssWriter(fnDss);
      using (dss) {
        using (H5Assist.H5Reader h5 = new H5Reader(fnHDF))
        {
            var realizations = h5.GetGroupNames(H5Reader.Root); // realization level.
            foreach (var realization in realizations)
            {
              int startLifeCycleNumber = (int.Parse(realization.ToLower().Replace("realization", "")) - 1) * 20;
              var binNames = h5.GetGroupNames(Path(realization));
            foreach (var bin in binNames)
            {
              startLifeCycleNumber++;
              var names = h5.GetDatasetNames(Path(realization, bin));
              float[] data = null;
              foreach (var dsn in names)
              {
                String binPath = Path(realization, bin, dsn);
                int ndims = h5.GetDatasetNDims(binPath);
                if (ndims != 1)
                  throw new Exception("Expecting 1D data sets...");
                h5.ReadDataset(binPath, ref data);
                Console.WriteLine(binPath + " : " + data.Length);
                WriteToDss(interval, t, dss, startLifeCycleNumber, data, dsn);

              }
            }
          }
        }
      }
      sW.Stop();
      TimeSpan ts = sW.Elapsed;

        // Format and display the TimeSpan value.
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        Console.WriteLine("RunTime " + elapsedTime);

    }

    private static void WriteToDss(string interval, DateTime t, DssWriter dss, int startLifeCycleNumber, float[] data, string dsn)
    {
      //string dssPath = BuildDssPath(dsn,binPath)
      string F = "C:" + startLifeCycleNumber.ToString().PadLeft(6, '0') + "|swg";
      string parameter = "PRECIP-INC";
      string units = "inches";
      string dataType = "PER-CUM";
      if (dsn.ToLower().Contains("temperature"))
      {
        units = "F";
        dataType = "PER-AVER";
        parameter = "Temperature";
      }
      string dssPath = "/Trinity/" + dsn + "/" + parameter + "//" + interval + "/" + F + "/";
      try{
        WriteToDss(dss, data, dssPath, t, units, dataType);
      }catch(Exception e){
        try{
          WriteToDss(dss, data, dssPath, t, units, dataType);
        }catch(Exception e2){
          await File.WriteAllTextAsync("WriteExceptions.txt","Exception " + e2.Message + " " + dssPath);
        }
      }
      
    }

    private static void WriteToDss(DssWriter dss, float[] data, string dssPath,DateTime startTime, string units, string dataType)
    {
      double[] d = new double[data.Length];
      Array.Copy(data, d, d.Length);
      // insert leap days... copy prev value...
      Hec.Dss.TimeSeries ts = new TimeSeries(dssPath, d, startTime, units, dataType);
      dss.Write(ts, true);
    }

  }
}
