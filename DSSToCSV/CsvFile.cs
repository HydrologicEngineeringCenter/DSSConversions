using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSSToCSV
{
  class CsvFile
  {

    /// <summary>
    /// Saves contents of CsvFile to a comma seperated file.
    /// TO DO:  check for internal commas, in string types.
    /// </summary>
    /// <param name="filename"></param>
    public static void Write(DataTable table, string filename)
    {
      StreamWriter sr = new StreamWriter(filename, false);
      int sz = table.Rows.Count;
      int cols = table.Columns.Count;
      bool[] IsStringColumn = new bool[cols];

      int c;
      for (c = 0; c < cols; c++)
      {
        if (c < cols - 1)
          sr.Write(table.Columns[c].ColumnName.Trim() + ",");
        else
          sr.WriteLine(table.Columns[c].ColumnName.Trim()); // no comma on last

        if (table.Columns[c].DataType.ToString() == "System.String")
          IsStringColumn[c] = true;
      }

      

      for (int r = 0; r < sz; r++)
      {
        for (c = 0; c < cols; c++)
        {
          if (IsStringColumn[c])
          {
            sr.Write("\"" + table.Rows[r][c] + "\"");
          }
          else
          {
            sr.Write(table.Rows[r][c]);
          }
          if (c < cols - 1)
            sr.Write(",");
        }
        sr.WriteLine();
      }
      sr.Close();
      Console.WriteLine(" done.");
    }
  }
}
