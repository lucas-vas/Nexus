using System;

namespace Nexus.Tests.Common
{
    public class TestServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register(Type type, object instance)
        {
            _services[type] = instance;
        }

        public object? GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var instance);
            return instance;
        }
    }
} 