namespace SmtpServer.Mime
{
    public sealed class MimeVersion : IMimeHeader
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public MimeVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name => "MIME-Version";

        /// <summary>
        /// The major version number.
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// The minor version number.
        /// </summary>
        public int Minor { get; }
    }
}