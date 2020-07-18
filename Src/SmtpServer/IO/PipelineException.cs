using System;

namespace SmtpServer.IO
{
    public abstract class PipelineException : Exception { }

    public sealed class PipelineCancelledException : PipelineException { }
}
