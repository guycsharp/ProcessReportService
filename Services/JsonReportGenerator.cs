public class JsonReportGenerator : IReportGenerator
{
    public string Generate()
    {
        return ProcessReporter.GenerateJsonReport();
    }
}
