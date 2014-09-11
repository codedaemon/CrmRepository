using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrmRepository.Agents;

namespace CrmRepository.EntityProviders
{
    public class AgentEntityProvider : IEntityProvider
    {
        public T GetInstance<T>(object key) where T : class
        {
            var agent = GetAgent(typeof(T));
            if (agent != null)
            {
                return (T)agent.GetInstance(key);
            }
            return null;
        }

        public T GetInstance<T>(object key, IEnumerable<string> columns) where T : class
        {
            var agent = GetAgent(typeof(T));
            if (agent != null)
            {
                return (T) agent.GetInstance(key, columns);
            }
            return null;
        }

        public TResult GetValue<T, TResult>(object key, string propertyName) where T : class
        {
            var agent = GetAgent(typeof(T));
            if (agent != null)
            {
                return (TResult)agent.GetValue(key, propertyName);
            }
            return default(TResult);
        }

        public TResult GetKey<T, TProperty, TResult>(string propertyName, TProperty propertyValue) where T : class
        {
            var agent = GetAgent(typeof(T));
            if (agent != null)
            {
                return (TResult)agent.GetKey(propertyName, propertyValue);
            }
            return default(TResult);
        }

        private IAgent GetAgent(Type type)
        {
            var assemblyname = Assembly.GetExecutingAssembly().FullName.Split(',')[0];
            var qualifiedAgentName = assemblyname + ".Agents." + type.Name + "Agent";
            var agentType = Type.GetType(qualifiedAgentName);
            if (agentType == null || !agentType.GetInterfaces().Contains(typeof(IAgent)))
            {
                return null;
            }
            return (IAgent)Activator.CreateInstance(agentType);
        }
    }
}