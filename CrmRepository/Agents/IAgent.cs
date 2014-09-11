using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmRepository.Agents
{
    public interface IAgent
    {
        object GetInstance(object key);
        object GetInstance(object key, IEnumerable<string> columns);
        object GetValue(object key, string propertyName);
        object GetKey(string propertyName, object propertyValue);
    }
}
