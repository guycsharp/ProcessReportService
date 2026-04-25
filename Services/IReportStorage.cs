public interface IReportStorage
{
    void Save(string content);
    string Load();
    bool Exists();
    void Delete();
}
