using System;
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