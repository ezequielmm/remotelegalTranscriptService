using FluentResults;
using Microsoft.Extensions.Hosting;
using PrecisionReporters.MediaService.Data.Dapper.Interfaces;
using PrecisionReporters.MediaService.Data.Models;
using PrecisionReporters.MediaService.Services.Interfaces;
using System;
using System.Collections.Generic;
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

            using (FileStream xmlStream = File.OpenRead(Path.Combine(_filePath, @"TemporalAudios\CR.ttml")))
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
                        var text = reader.ReadInnerXml()
                            .Replace("<tt:", "<")
                            .Replace("</tt:", "</")
                            .Replace(string.Format(@" xmlns:tt=""{0}""", tt), "")
                            .Replace(string.Format(@" xmlns=""{0}""", tt), "");


                        var transcript = new Transcription
                        {
                            Id = Guid.NewGuid(),
                            CreationDate = DateTime.UtcNow,
                            Text = text,
                            UserId = Guid.Parse("122a2378-59e6-43a3-835f-68790adf0d2c"),
                            DepositionId = Guid.Parse("d5e061f2-5c8b-4506-82c9-64af66a6dc24"),
                            TranscriptDateTime = DateTime.Now,
                            Confidence = double.Parse(confidence),
                            Duration = 1, //endString - beginString,
                            //PostProcessed = true
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
    }
}
