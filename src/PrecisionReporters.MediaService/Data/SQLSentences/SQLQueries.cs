﻿namespace PrecisionReporters.MediaService.Data.SQLSentences
{
    public class SQLQueries
    {
        public static string GetTranscriptions(string depositionId) 
        {
            return $"SELECT * FROM Transcriptions WHERE DepositionId = '{depositionId}'";
        }

        public static string SaveTranscriptions()
        {
            return $"INSERT INTO Transcriptions (Id, CreationDate, Text, UserId, DepositionId, TranscriptDateTime, Confidence, Duration) VALUES (@Id, @CreationDate, @Text, @UserId, @DepositionId, @TranscriptDateTime, @Confidence, @Duration)";
        }
    }
}
