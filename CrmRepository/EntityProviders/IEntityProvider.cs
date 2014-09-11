using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace CrmRepository.EntityProviders
{
    public interface IEntityProvider
    {
        T GetInstance<T>(object key) where T : class;
        T GetInstance<T>(object key, IEnumerable<string> columns) where T : class;
        TResult GetValue<T, TResult>(object key, string propertyName) where T : class;
        TResult GetKey<T, TProperty, TResult>(string propertyName, TProperty propertyValue) where T : class;
    }
}
