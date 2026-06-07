using System.Threading.Tasks;

namespace ServiceContracts.AI
{
    public interface IGeminiService
    {
        Task<string> AskAsync(string prompt);
    }
}
