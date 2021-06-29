namespace PrecisionReporters.MediaService.Data.SQLSentences
{
    public class SQLQueries
    {
        public static string GetTranscriptions(string depositionId) 
        {
            return $"SELECT * FROM Transcriptions WHERE DepositionId = '{depositionId}'";
        }
    }
}
