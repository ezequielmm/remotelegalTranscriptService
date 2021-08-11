using FluentResults;
using Microsoft.Extensions.Hosting;
using PrecisionReporters.MediaService.Data.Dapper.Interfaces;
using PrecisionReporters.MediaService.Data.Models;
using PrecisionReporters.MediaService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrecisionReporters.MediaService.Services
{
    public class TranscriptionService : ITranscriptionService
    {
        private readonly IConfigurationData _configurationData;
        private readonly string _filePath;

        public TranscriptionService(IConfigurationData configurationData, IHostEnvironment env)
        {
            _configurationData = configurationData;
            _filePath = env.ContentRootPath;
        }

        public async Task<List<Transcription>> GetTranscriptionsByDepositionId(Guid depositionId)
        {
            var transcripts = await _configurationData.GetTranscriptionsAsync(depositionId);
            return transcripts.ToList();
        }

        public async Task<List<TranscriptionItem>> ParseStream() //Stream xmlStream, Encoding encoding
        {
            var items = new List<TranscriptionItem>();

            //CultureInfo enUS = new CultureInfo("en-US");
            string dateString = "2021-08-05 20:36:38";

            var audioDateTime = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);


            var depositionId = Guid.Parse("54c5e1e4-1d05-4e51-b3e3-dee5fdbcd262");

            using (FileStream xmlStream = File.OpenRead(Path.Combine(_filePath, @"TemporalAudios\20_36_38_Participant1.ttml")))
            {
                XElement xElement = XElement.Load(xmlStream);
                var tt = xElement.GetNamespaceOfPrefix("tt") ?? xElement.GetDefaultNamespace();

                var nodeList = xElement.Descendants(tt + "p").ToList();

                foreach (var node in nodeList)
                {
                    try
                    {
                        var reader = node.CreateReader();
                        reader.MoveToContent();
                        var confidence = Regex.Replace(node.PreviousNode.ToString(), "[^0-9.]", "");
                        var beginString = node.Attribute("begin").Value.Replace("t", ""); // need to sum audio startDate
                        var startTicks = ParseTimecode(beginString);
                        var endString = node.Attribute("end").Value.Replace("t", ""); // need to sum audio startDate
                        var endTicks = ParseTimecode(endString);

                        //TimeSpan ts = TimeSpan.FromTicks(endTicks);
                        var ts = DateTime.ParseExact(endString, "HH:mm:ss.fff", CultureInfo.InvariantCulture).TimeOfDay;

                        var text = reader.ReadInnerXml()
                            .Replace("<tt:", "<")
                            .Replace("</tt:", "</")
                            .Replace(string.Format(@" xmlns:tt=""{0}""", tt), "")
                            .Replace(string.Format(@" xmlns=""{0}""", tt), "");

                        var audioStartTime = await CalculateTimeDifference(depositionId, audioDateTime); //(suma o resta del begin o ending string)

                        var transcript = new Transcription
                        {
                            Id = Guid.NewGuid(),
                            CreationDate = audioStartTime,//DateTime.UtcNow,
                            Text = text,
                            UserId = Guid.Parse("e82e7477-68a8-418d-8650-08d92478cc68"),
                            DepositionId = depositionId,
                            TranscriptDateTime = audioStartTime + ts, // LLAMAR A MI FUNCION 20.30.47 AudioStartTime + endstring 
                            Confidence = double.Parse(confidence),
                            Duration = 1,// int.Parse(endString) - int.Parse(beginString),
                            PostProcessed = true
                        };

                        await SaveTranscriptions(transcript);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception raised when parsing xml node {0}: {1}", node, ex);
                    }
                }

                return items;
            }
        }

        private async Task<int> SaveTranscriptions(Transcription transcript)
        {
            return await _configurationData.SaveTranscriptionsAsync(transcript);
        }

        private long ParseTimecode(string s)
        {
            TimeSpan result;
            if (TimeSpan.TryParse(s, out result))
            {
                return (long)result.TotalMilliseconds;
            }

            long ticks;
            if (long.TryParse(s.TrimEnd('t'), out ticks))
            {
                return ticks / 10000;
            }
            return -1;
        }

        private async Task<DateTime> CalculateTimeDifference(Guid myGuid, DateTime audioDateTime)
        {
            var depoList = await _configurationData.GetDepositionEventsAsync(myGuid);

            List<DepositionEvent> onTheRecordList = depoList.Where(x => x.EventType == Data.Enums.EventType.OnTheRecord).ToList().OrderBy(x => x.CreationDate).ToList();

            try
            {
                for (int i = 0; i < onTheRecordList.Count(); i++)
                {
                    bool biggerThanEarlier = false;
                    if (i == 0 || (i > 0 && onTheRecordList[i - 1].CreationDate < onTheRecordList[i].CreationDate))
                    {
                        biggerThanEarlier = true;
                    }

                    bool minorThanNext = false;
                    if ((i + 1) == onTheRecordList.Count() || (i < onTheRecordList.Count() && onTheRecordList[i + 1].CreationDate > onTheRecordList[i].CreationDate))
                    {
                        minorThanNext = true;
                    }

                    if (biggerThanEarlier && minorThanNext)
                    {
                        // usa lista[i]
                        var differenceResult = audioDateTime - onTheRecordList[i].CreationDate;

                        var result = audioDateTime - differenceResult;

                        //double dResult = result.Ticks;

                        return result; // si la hora del ttml es 20:30:53 y la diferencia de la funcion son 6 deberia devolver 20.30.47
                    }
                }

            }
            catch (FormatException)
            {
                return DateTime.MinValue;
            }

            return DateTime.MinValue;

        }
    }
}
