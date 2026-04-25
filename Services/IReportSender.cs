using System.Threading.Tasks;

public interface IReportSender
{
    Task SendAsync(string content);
}
