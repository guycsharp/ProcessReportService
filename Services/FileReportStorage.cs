using System.IO;

public class FileReportStorage : IReportStorage
{
    private readonly string _path;

    public FileReportStorage(string path)
    {
        _path = path;
    }

    public void Save(string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path));
        File.WriteAllText(_path, content);
    }

    public string Load() => File.ReadAllText(_path);

    public bool Exists() => File.Exists(_path);

    public void Delete() => File.Delete(_path);
}
