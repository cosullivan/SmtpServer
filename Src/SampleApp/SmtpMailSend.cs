// Smtp Server

using System;
using System.Net.Mail;
using System.Net;

namespace SMTP
{
    class Program
    {
        static void Main(string[] args)
        {

            /*
             * 
                SMTP Protocols are different for every mail service.
                The Protocols mentioned below are only for outlook.
                Look at the readme file & Resources section for more information.
                Go to this link for Smtp Settings "http://www.yetesoft.com/free-email-marketing-resources/pop-smtp-server-settings/"
                
                NOTE (For Gmail Users Only) For sending purpose:
                Gmail users will first have to activate the LessSecureApp Functionality
                Go to this link for more information "https://myaccount.google.com/lesssecureapps"
            *
            */

            // These smtp protocols are for Outlook
            string smtpAddress = "smtp-mail.outlook.com";
            int portNumber = 587;

            string emailTo = "Enter the Email id (Reciever)";
            string emailFrom = "Enter the Email id (Sender)";
            string password = "Password of Email id";
            string subject = "Subject of The Mail";
            string body = "Body of The Mail";
            Attachment attachment = new Attachment(@"Location of the attachment");

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom);
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = body;
                mail.Attachments.Add(attachment);
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFrom, password);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}