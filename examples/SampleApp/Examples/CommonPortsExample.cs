using SmtpServer;
using System.Security.Cryptography.X509Certificates;

namespace SampleApp.Examples
{
    public static class CommonPortsExample
    {
        public static void Run()
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")

                // Port 25 is primarily used for SMTP relaying where emails are 
                // sent from one mail server to another. Mail clients generally wont
                // use this port and most ISP will likely block it anyway.
                .Endpoint(builder => builder.Port(25).IsSecure(false))

                // For a brief period in time this was a recognized port whereby
                // TLS was enabled by default on the connection. When connecting to
                // port 465 the client will upgrade its connection to SSL before
                // doing anything else. Port 465 is obsolete in favor of using
                // port 587 but it is still available by some mail servers.
                .Endpoint(builder => 
                    builder
                        .Port(465)
                        .IsSecure(true) // indicates that the client will need to upgrade to SSL upon connection
                        .Certificate(new X509Certificate2())) // requires a valid certificate to be configured

                // Port 587 is the default port that should be used by modern mail
                // clients. When a certificate is provided, the server will advertise
                // that is supports the STARTTLS command which allows the client
                // to determine when they want to upgrade the connection to SSL. 
                .Endpoint(builder => 
                    builder
                        .Port(587)
                        .AllowUnsecureAuthentication(false) // using 'false' here means that the user cant authenticate unless the connection is secure
                        .Certificate(new X509Certificate2())) // requires a valid certificate to be configured

                .Build();
        }
    }
}