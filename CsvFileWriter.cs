using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Serial_Oscilloscope
{
    class CsvFileWriter
    {
        /// <summary>
        /// File path of CSV file.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Internal flag used to disable writes during file close.
        /// </summary>
        private bool writesEnabled;

        /// <summary>
        /// Stream Writer to write to file.
        /// </summary>
        private StreamWriter streamWriter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath"></param>
        /// 
        public CsvFileWriter(string filePath, string startTime, string fileFormat)
        {
            FilePath = filePath;
            writesEnabled = true;
            streamWriter = null;

            if (writesEnabled)
            {
                // Open file
                if (streamWriter == null)
                {
                    streamWriter = new System.IO.StreamWriter(FilePath, false);
                }
                streamWriter.WriteLine(fileFormat);
                //streamWriter.WriteLine(startTime);

            }
        }
        public CsvFileWriter(string filePath)
        {
            FilePath = filePath;
            writesEnabled = true;
            streamWriter = null;
        }

        /// <summary>
        /// Close CSV file.
        /// </summary>
        public void CloseFile()
        {
            List<string> fileNames = new List<string>();
            writesEnabled = false;
            streamWriter.Close();
        }

        /// <summary>
        /// Write array of values as line of CSVs in file.
        /// </summary>
        /// <param name="values"></param>
        public void WriteCSVline(float[] values)
        {
            if (writesEnabled)
            {
                // Open file
                if (streamWriter == null)
                {
                    streamWriter = new System.IO.StreamWriter(FilePath, false);
                }

                // Write line
                string csvLine = "";
                for(int i = 0; i <values.Length; i++){
                    csvLine += values[i].ToString(CultureInfo.InvariantCulture);
                    if (i < values.Length - 1)
                    {
                        csvLine += ",";
                    }
                }
                streamWriter.WriteLine(csvLine);
            }
        }
        public void WriteCSVline(List<string> listStringData)
        {
            if (writesEnabled)
            {
                // Open file
                if (streamWriter == null)
                {
                    streamWriter = new System.IO.StreamWriter(FilePath, false);
                }
                // Write line
                //string csvLine = "";
                string[] data = listStringData.ToArray();
                foreach (string csvLine in data)
                {
                    streamWriter.WriteLine(csvLine);
                }
            }
        }
    }
}
