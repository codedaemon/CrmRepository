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
    }
}
