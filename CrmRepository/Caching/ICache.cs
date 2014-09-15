using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CrmRepository.Caching
{
    public interface ICache
    {
        bool Save<T>(T instance);
        bool SaveSingleValue<T>(string lookupProperty, object lookupValue, string propertyToCache, object valueToCache);
    }
}
