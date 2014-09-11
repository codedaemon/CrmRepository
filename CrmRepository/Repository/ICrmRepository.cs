using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CrmRepository.Repository
{
    public interface ICrmRepository
    {
        T GetInstance<T>(object key) where T : class;
        T GetInstance<T>(object key, Func<T, object> columns ) where T : class, new();
        TResult GetValue<T, TResult>(object key, Expression<Func<T, TResult>> property) where T : class;
        TResult GetKey<T, TProperty, TResult>(Expression<Func<T, TProperty>> property, TProperty propertyValue) where T : class;
    }
}
