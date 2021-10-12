using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.Gelf
{
    public class GelfLogScope
    {
        private readonly string _name;
        private readonly object _state;
        private static readonly AsyncLocal<GelfLogScope?> _value = new();

        internal GelfLogScope(string name, object state)
        {
            _name = name;
            _state = state;
        }

        public GelfLogScope? Parent { get; private set; }

        public static GelfLogScope? Current
        {
            set
            {
                _value.Value = value;
            }
            get
            {
                return _value.Value;
            }
        }

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;

            Current = new GelfLogScope(name, state)
            {
                Parent = temp
            };

            return new DisposableScope();
        }

        public IEnumerable<KeyValuePair<string, object>> GetScopeKeyValuePairs()
        {
            if (_state is IEnumerable<KeyValuePair<string, object>> stateProperties)
            {
                return stateProperties;

            }

            return Enumerable.Empty<KeyValuePair<string, object>>();
        }

        public override string? ToString()
        {
            return _state.ToString();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current?.Parent;
            }
        }
    }
}
