using System.Collections.Generic;
using System.IO;

namespace DS4WinWPF
{
    public class LogWriter
    {
        private readonly string filename;
        private readonly List<LogItem> logCol;

        public LogWriter(string filename, List<LogItem> col)
        {
            this.filename = filename;
            logCol = col;
        }

        public void Process()
        {
            var outputLines = new List<string>();
            foreach (var item in logCol) outputLines.Add($"{item.Datetime}: {item.Message}");

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