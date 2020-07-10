using Refit;
using System.Threading.Tasks;

namespace TelegramBot.Bot
{
    public interface IViaCepApi
    {
        [Get("/{cep}/json/")]
        Task<Data> GetAsync(string cep);
    }
}
