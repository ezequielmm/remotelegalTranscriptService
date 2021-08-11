namespace PrecisionReporters.MediaService.Data.SQLSentences
{
    public class SQLQueries
    {
        public static string GetTranscriptions(string depositionId) 
        {
            return $"SELECT * FROM Transcriptions WHERE DepositionId = '{depositionId}'";
        }

        public static string GetDepositionEvents(string depositionId)
        {
            return $"SELECT * FROM DepositionEvents WHERE DepositionId = '{depositionId}'";
        }

        public static string SaveTranscriptions()
        {
            return $"INSERT INTO Transcriptions (Id, CreationDate, Text, UserId, DepositionId, TranscriptDateTime, Confidence, Duration, PostProcessed) VALUES (@Id, @CreationDate, @Text, @UserId, @DepositionId, @TranscriptDateTime, @Confidence, @Duration, @PostProcessed)";
        }
    }
}
