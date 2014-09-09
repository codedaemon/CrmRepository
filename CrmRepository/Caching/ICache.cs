using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CrmRepository.Caching
{
    public interface ICache
    {
        bool Save<T>(T instance);
    }
}
