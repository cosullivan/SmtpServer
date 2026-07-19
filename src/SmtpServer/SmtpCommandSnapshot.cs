using System;
using SmtpServer.Mail;
using SmtpServer.Protocol;

namespace SmtpServer
{
    /// <summary>
    /// Provides a safe-to-log snapshot of an SMTP command.
    /// </summary>
    public sealed class SmtpCommandSnapshot
    {
        /// <summary>
        /// The replacement text used for sensitive command arguments.
        /// </summary>
        public const string Redacted = "<redacted>";

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpCommandSnapshot"/> class.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="argument">The safe command argument.</param>
        public SmtpCommandSnapshot(string name, string argument)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Argument = argument;
        }

        /// <summary>
        /// Creates a safe-to-log snapshot for the specified command.
        /// </summary>
        /// <param name="command">The command to snapshot.</param>
        /// <returns>The safe command snapshot.</returns>
        public static SmtpCommandSnapshot From(SmtpCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            switch (command)
            {
                case AuthCommand authCommand:
                    return new SmtpCommandSnapshot(authCommand.Name, $"{authCommand.Method} {Redacted}");

                case BdatCommand bdatCommand:
                    return new SmtpCommandSnapshot(bdatCommand.Name, bdatCommand.IsLast ? $"{bdatCommand.Size} LAST" : bdatCommand.Size.ToString());

                case EhloCommand ehloCommand:
                    return new SmtpCommandSnapshot(ehloCommand.Name, ehloCommand.DomainOrAddress);

                case HeloCommand heloCommand:
                    return new SmtpCommandSnapshot(heloCommand.Name, heloCommand.DomainOrAddress);

                case HelpCommand helpCommand:
                    return new SmtpCommandSnapshot(helpCommand.Name, helpCommand.Argument);

                case MailCommand mailCommand:
                    return new SmtpCommandSnapshot(mailCommand.Name, $"FROM:<{mailCommand.Address.AsAddress()}>");

                case RcptCommand rcptCommand:
                    return new SmtpCommandSnapshot(rcptCommand.Name, $"TO:<{rcptCommand.Address.AsAddress()}>");

                case VrfyCommand vrfyCommand:
                    return new SmtpCommandSnapshot(vrfyCommand.Name, vrfyCommand.Argument);

                case ExpnCommand expnCommand:
                    return new SmtpCommandSnapshot(expnCommand.Name, expnCommand.Argument);

                default:
                    return new SmtpCommandSnapshot(command.Name, null);
            }
        }

        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the safe command argument.
        /// </summary>
        public string Argument { get; }

        /// <summary>
        /// Returns the safe command text.
        /// </summary>
        /// <returns>The safe command text.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Argument) ? Name : $"{Name} {Argument}";
        }
    }
}
