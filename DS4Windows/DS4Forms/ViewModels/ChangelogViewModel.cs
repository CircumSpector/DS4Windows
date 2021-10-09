using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Documents;
using DS4Windows;
using DS4WinWPF.DS4Control.IoC.Services;
using HttpProgress;
using MarkdownEngine = MdXaml.Markdown;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class ChangelogViewModel
    {
        private FlowDocument changelogDocument;

        public ChangelogViewModel()
        {
            BuildTempDocument("Retrieving changelog info.Please wait...");
        }

        public FlowDocument ChangelogDocument
        {
            get => changelogDocument;
            private set
            {
                if (changelogDocument == value) return;
                changelogDocument = value;
                ChangelogDocumentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ChangelogDocumentChanged;

        private void BuildTempDocument(string message)
        {
            var flow = new FlowDocument();
            flow.Blocks.Add(new Paragraph(new Run(message)));
            ChangelogDocument = flow;
        }

        public async void RetrieveChangelogInfo()
        {
            // Sorry other devs, gonna have to find your own server
            var url = new Uri(Constants.ChangelogUri);
            var filename = Path.Combine(Path.GetTempPath(), "Changelog.min.json");
            var readFile = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                var temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                try
                {
                    await temp.ConfigureAwait(true);
                    if (temp.Result.IsSuccessStatusCode) readFile = true;
                }
                catch (HttpRequestException)
                {
                }
            }

            var fileExists = File.Exists(filename);
            if (fileExists && readFile)
            {
                var temp = File.ReadAllText(filename).Trim();
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    options.Converters.Add(new DateTimeJsonConverter.DateTimeConverterUsingDateTimeParse());
                    var tempInfo = JsonSerializer.Deserialize<ChangelogInfo>(temp, options);
                    BuildChangelogDocument(tempInfo);
                }
                catch (JsonException)
                {
                }
            }
            else if (!readFile)
            {
                BuildTempDocument("Failed to retrieve information");
            }

            if (fileExists) File.Delete(filename);
        }

        private void BuildChangelogDocument(ChangelogInfo tempInfo)
        {
            var engine = new MarkdownEngine();
            var flow = new FlowDocument();
            foreach (var versionInfo in tempInfo.Changelog.Versions)
            {
                var tmpLog = versionInfo.ApplicableInfo(AppSettingsService.Instance.Settings.UseLang);
                if (tmpLog != null)
                {
                    var tmpPar = new Paragraph();
                    var tmp = tmpLog.Header;
                    tmpPar.Inlines.Add(new Run(tmp) { Tag = "Header" });
                    flow.Blocks.Add(tmpPar);

                    tmpPar.Inlines.Add(new LineBreak());
                    tmpPar.Inlines.Add(new Run(versionInfo.ReleaseDate.ToUniversalTime().ToString("r"))
                        { Tag = "ReleaseDate" });

                    tmpLog.BuildDisplayText();

                    var tmpDoc = engine.Transform(tmpLog.DisplayLogText);
                    flow.Blocks.AddRange(new List<Block>(tmpDoc.Blocks));

                    tmpPar = new Paragraph();
                    flow.Blocks.Add(tmpPar);
                }
            }

            ChangelogDocument = flow;
        }
    }
}