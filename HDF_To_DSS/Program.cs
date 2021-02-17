﻿using H5Assist;
using Hec.Dss;
using System;
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
      if(  args.Length != 2)
      {
        Console.WriteLine("Usage:hdf_to_dss.exe input.hdf  output.dss");
        return;
      }
      
      string interval = "1Hour";
      DateTime t = DateTime.Parse("1-1-2000 1:00 am");
      string fnDss = args[1];
      string fnHDF = args[0]; //@"C:\project\HDF_To_DSS\HDF_To_DSS\SampleData100Years.wg";
      Stopwatch sW = new Stopwatch();
      sW.Start();
      //add dss 6 example   
      using (Hec.Dss.DssWriter dss = new Hec.Dss.DssWriter(fnDss)) {
        using (H5Assist.H5Reader h5 = new H5Reader(fnHDF))
        {
          var realizations = h5.GetGroupNames(H5Reader.Root); // realization level.
          foreach (var realization in realizations)
          {

            int startLifeCycleNumber = (int.Parse(realization.ToLower().Replace("realization", ""))-1) * 20;
            var binNames = h5.GetGroupNames(Path(realization));
            foreach (var bin in binNames)
            {
              startLifeCycleNumber++;
              var names = h5.GetDatasetNames(Path(realization, bin));
              float[] data = null;
              foreach (var dsn in names)
              {
                String binPath = Path(realization, bin, dsn);
                //var tn = h5.GetAttributeNamesAndTypes(binPath);
                int ndims = h5.GetDatasetNDims(binPath);
                if (ndims != 1)
                  throw new Exception("Expecting 1D data sets...");
                h5.ReadDataset(binPath, ref data);

                Console.WriteLine(binPath + " : " + data.Length);

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
                string dssPath = "/Trinity/"+ dsn + "/"+parameter+"//" + interval + "/" + F + "/";
                WriteToDss(dss, data, dssPath,t,units, dataType);
                
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
