using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SmtpServer.Tests.Mocks
{
    internal sealed class TestLoggerFactory : ILoggerFactory
    {
        readonly List<TestLogEntry> _entries = new List<TestLogEntry>();
        readonly AsyncLocal<Scope> _currentScope = new AsyncLocal<Scope>();

        public IReadOnlyList<TestLogEntry> Entries
        {
            get
            {
                lock (_entries)
                {
                    return _entries.ToList();
                }
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(this, categoryName);
        }

        public void AddProvider(ILoggerProvider provider) { }

        public void Dispose() { }

        internal IDisposable PushScope(object state)
        {
            var scope = new Scope(this, state, _currentScope.Value);
            _currentScope.Value = scope;
            return scope;
        }

        internal IReadOnlyList<object> GetScopes()
        {
            var scopes = new List<object>();

            for (var scope = _currentScope.Value; scope != null; scope = scope.Parent)
            {
                scopes.Add(scope.State);
            }

            scopes.Reverse();
            return scopes;
        }

        internal void Add(TestLogEntry entry)
        {
            lock (_entries)
            {
                _entries.Add(entry);
            }
        }

        sealed class Scope : IDisposable
        {
            readonly TestLoggerFactory _factory;

            public Scope(TestLoggerFactory factory, object state, Scope parent)
            {
                _factory = factory;
                State = state;
                Parent = parent;
            }

            public object State { get; }

            public Scope Parent { get; }

            public void Dispose()
            {
                if (_factory._currentScope.Value == this)
                {
                    _factory._currentScope.Value = Parent;
                }
            }
        }

        sealed class TestLogger : ILogger
        {
            readonly TestLoggerFactory _factory;
            readonly string _categoryName;

            public TestLogger(TestLoggerFactory factory, string categoryName)
            {
                _factory = factory;
                _categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return _factory.PushScope(state);
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _factory.Add(new TestLogEntry(_categoryName, logLevel, eventId, state, formatter(state, exception), exception, _factory.GetScopes()));
            }
        }
    }

    internal sealed class TestLogEntry
    {
        public TestLogEntry(string categoryName, LogLevel logLevel, EventId eventId, object state, string message, Exception exception, IReadOnlyList<object> scopes)
        {
            CategoryName = categoryName;
            LogLevel = logLevel;
            EventId = eventId;
            State = state;
            Message = message;
            Exception = exception;
            Scopes = scopes;
        }

        public string CategoryName { get; }

        public LogLevel LogLevel { get; }

        public EventId EventId { get; }

        public object State { get; }

        public string Message { get; }

        public Exception Exception { get; }

        public IReadOnlyList<object> Scopes { get; }

        public bool TryGetStateValue(string name, out object value)
        {
            return TryGetValue(State, name, out value);
        }

        public bool TryGetScopeValue(string name, out object value)
        {
            foreach (var scope in Scopes)
            {
                if (TryGetValue(scope, name, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }

        static bool TryGetValue(object state, string name, out object value)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> properties)
            {
                foreach (var property in properties)
                {
                    if (property.Key == name)
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }
    }
}
