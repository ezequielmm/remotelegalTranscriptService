using Dapper;
using FluentResults;
using MySql.Data.MySqlClient;
using PrecisionReporters.MediaService.Data.Dapper.Interfaces;
using PrecisionReporters.MediaService.Data.Models;
using PrecisionReporters.MediaService.Data.SQLSentences;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PrecisionReporters.MediaService.Data.Dapper
{
    public class ConfigurationData : IConfigurationData
    {
        protected readonly IDbConnection _remoteLegalConnection;
        public ConfigurationData(string connectionString)
        {
            _remoteLegalConnection = new MySqlConnection(connectionString);
        }

        public async Task<IEnumerable<Transcription>> GetTranscriptionsAsync(Guid depositionId)
        {
            return await _remoteLegalConnection.QueryAsync<Transcription>(SQLQueries.GetTranscriptions(depositionId.ToString()));
        }

        public async Task<int> SaveTranscriptionsAsync(Transcription transcript)
        {
            return await _remoteLegalConnection.ExecuteAsync(SQLQueries.SaveTranscriptions(), transcript);
        }

    }
}
