using System.Threading.Tasks;

namespace ServiceContracts.AI
{
    public interface IGeminiService
    {
        Task<string> AskAsync(string prompt);
        Task<string> AskAsync(string prompt, object? responseSchema);
    }
}
