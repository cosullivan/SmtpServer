namespace SmtpServer.Mail
{
    public sealed class MessageSerializerFactory
    {
        /// <summary>
        /// Create an instance of a message serializer.
        /// </summary>
        /// <returns>An instance of a message serializer.</returns>
        public IMessageSerializer CreateInstance()
        {
            return new TextMessageSerializer();
        }
    }
}