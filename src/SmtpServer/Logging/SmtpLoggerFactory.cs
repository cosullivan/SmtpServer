using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SmtpServer.ComponentModel;
using SmtpServer.Net;

namespace SmtpServer.Logging
{
    internal static class SmtpLoggerFactory
    {
        internal static ILoggerFactory Resolve(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                return NullLoggerFactory.Instance;
            }

            try
            {
                return serviceProvider.GetServiceOrDefault<ILoggerFactory>(NullLoggerFactory.Instance);
            }
            catch (NotSupportedException)
            {
                return NullLoggerFactory.Instance;
            }
        }

        internal static IReadOnlyList<KeyValuePair<string, object>> CreateSessionScope(SmtpSessionContext context)
        {
            return new SmtpSessionLogScope(context);
        }

        static object TryGetProperty(SmtpSessionContext context, string key)
        {
            return context.Properties.TryGetValue(key, out var value) ? value : null;
        }

        sealed class SmtpSessionLogScope : IReadOnlyList<KeyValuePair<string, object>>
        {
            static readonly string[] Names =
            {
                "SessionId",
                "LocalEndPoint",
                "RemoteEndPoint",
                "EndpointPort",
                "IsSecure",
                "SslProtocol",
                "IsAuthenticated"
            };

            readonly SmtpSessionContext _context;

            public SmtpSessionLogScope(SmtpSessionContext context)
            {
                _context = context;
            }

            public KeyValuePair<string, object> this[int index] => new KeyValuePair<string, object>(Names[index], GetValue(Names[index]));

            public int Count => Names.Length;

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            object GetValue(string name)
            {
                switch (name)
                {
                    case "SessionId":
                        return _context.SessionId;

                    case "LocalEndPoint":
                        return TryGetProperty(_context, EndpointListener.LocalEndPointKey);

                    case "RemoteEndPoint":
                        return TryGetProperty(_context, EndpointListener.RemoteEndPointKey);

                    case "EndpointPort":
                        return _context.EndpointDefinition.Endpoint.Port;

                    case "IsSecure":
                        return _context.Pipe?.IsSecure ?? _context.EndpointDefinition.IsSecure;

                    case "SslProtocol":
                        return _context.Pipe?.SslProtocol.ToString();

                    case "IsAuthenticated":
                        return _context.Authentication.IsAuthenticated;

                    default:
                        return null;
                }
            }
        }
    }
}
