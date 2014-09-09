using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CrmRepository.Repository
{
    public interface ICrmRepository
    {
        T GetInstance<T>(object key) where T : class;
    }
}
