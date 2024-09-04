using System;

namespace SmtpServer.IO
{
    /// <summary>
    /// Pipeline Exception
    /// </summary>
    public abstract class PipelineException : Exception { }

    /// <summary>
    /// Pipeline Cancelled Exception
    /// </summary>
    public sealed class PipelineCancelledException : PipelineException { }
}
