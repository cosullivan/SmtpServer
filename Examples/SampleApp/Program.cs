﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MimeKit;
using SampleApp.Examples;
using SmtpServer.IO;
using SmtpServer.Protocol;
using SmtpServer.Tests;
using SmtpServer.Text;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            //SimpleExample.Run();
            //SimpleServerExample.Run();
            //CustomEndpointListenerExample.Run();
            //ServerCancellingExample.Run();
            //SessionTracingExample.Run();
            //DependencyInjectionExample.Run();
            SecureServerExample.Run();

            //SampleMailClient.Send(user: "user1", password: "password1", useSsl: false, port: 587);
            //SampleMailClient.Send(useSsl: false, port: 587);

            //var files = Directory.GetFiles(@"C:\Temp\enron_mail_20150507.tar", "*.*", SearchOption.AllDirectories).ToList();
            //Console.WriteLine(files.OrderByDescending(file => new FileInfo(file).Length).First());
        }

        static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}