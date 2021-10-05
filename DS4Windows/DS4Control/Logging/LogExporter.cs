using System.Collections.Generic;
using System.IO;

namespace DS4WinWPF.DS4Control.Logging
{
    /// <summary>
    ///     Helper to transform the UI log into a text file.
    /// </summary>
    public class LogExporter
    {
        private readonly string filename;
        private readonly List<LogItem> logCol;

        public LogExporter(string filename, List<LogItem> col)
        {
            this.filename = filename;
            logCol = col;
        }

        public void Process()
        {
            var outputLines = new List<string>();
            foreach (var item in logCol) outputLines.Add($"{item.Time}: {item.Message}");

            try
            {
                var stream = new StreamWriter(filename);
                foreach (var line in outputLines) stream.WriteLine(line);
                stream.Close();
            }
            catch
            {
            }
        }
    }
}