using System;
using System.Collections.Generic;
using System.Linq;

namespace RazorDoIt.Sandbox
{
    public class DelegateServiceLocator
    {
        private IDictionary<Type, Func<object>> _delegates = new Dictionary<Type, Func<object>>();

        public void Entry<T>(Func<object> functor)
        {
            _delegates[typeof(T)] = functor;
        }

        public T Resolve<T>(Type type) where T : class
        {

            if (!_delegates.ContainsKey(type))
                return null;

            return _delegates[type]() as T;
        }

        public object GetInstance(Type serviceType)
        {
            return Resolve<object>(serviceType);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return Enumerable.Empty<object>();
        }
    }
}