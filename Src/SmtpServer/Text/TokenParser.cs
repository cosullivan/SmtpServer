namespace SmtpServer.Text
{
    public abstract class TokenParser
    {
        /// <summary>
        /// Delegate for the TryMake function.
        /// </summary>
        /// <returns>true if the make operation was a success, false if not.</returns>
        protected delegate bool TryMakeDelegate();

        /// <summary>
        /// Delegate for the TryMake function to allow for "out" parameters.
        /// </summary>
        /// <typeparam name="TOut">The type of the out parameter.</typeparam>
        /// <param name="found">The out parameter that was found during the make operation.</param>
        /// <returns>true if the make operation found a parameter, false if not.</returns>
        protected delegate bool TryMakeDelegate<TOut>(out TOut found);

        /// <summary>
        /// Delegate for the TryMake function to allow for "out" parameters.
        /// </summary>
        /// <typeparam name="TIn">The type of the in parameter.</typeparam>
        /// <typeparam name="TOut">The type of the out parameter.</typeparam>
        /// <param name="parameter">The input parameter for the function.</param>
        /// <param name="found">The out parameter that was found during the make operation.</param>
        /// <returns>true if the make operation found a parameter, false if not.</returns>
        protected delegate bool TryMakeDelegate<in TIn, TOut>(TIn parameter, out TOut found);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="enumerator">The token enumerator to handle the incoming tokens.</param>
        protected TokenParser(ITokenEnumerator enumerator)
        {
            Enumerator = enumerator;
        }

        /// <summary>
        /// Try to make a callback in a transactional way.
        /// </summary>
        /// <param name="delegate">The callback to perform the match.</param>
        /// <returns>true if the match could be made, false if not.</returns>
        protected bool TryMake(TryMakeDelegate @delegate)
        {
            var checkpoint = Enumerator.Checkpoint();

            if (@delegate() == false)
            {
                checkpoint.Rollback();
                return false;
            }

            checkpoint.Dispose();
            return true;
        }

        /// <summary>
        /// Try to make a callback in a transactional way.
        /// </summary>
        /// <param name="delegate">The callback to perform the match.</param>
        /// <param name="found">The parameter that was returned from the matching function.</param>
        /// <returns>true if the match could be made, false if not.</returns>
        protected bool TryMake<TOut>(TryMakeDelegate<TOut> @delegate, out TOut found)
        {
            var checkpoint = Enumerator.Checkpoint();

            if (@delegate(out found) == false)
            {
                checkpoint.Rollback();
                return false;
            }

            checkpoint.Dispose();
            return true;
        }

        /// <summary>
        /// Try to make a callback in a transactional way.
        /// </summary>
        /// <param name="delegate">The callback to perform the match.</param>
        /// <param name="parameter">The input parameter.</param>
        /// <param name="found">The parameter that was returned from the matching function.</param>
        /// <returns>true if the match could be made, false if not.</returns>
        protected bool TryMake<TIn, TOut>(TryMakeDelegate<TIn, TOut> @delegate, TIn parameter, out TOut found)
        {
            var checkpoint = Enumerator.Checkpoint();

            if (@delegate(parameter, out found) == false)
            {
                checkpoint.Rollback();
                return false;
            }

            checkpoint.Dispose();
            return true;
        }

        /// <summary>
        /// Returns the enumerator to handle the incoming tokens.
        /// </summary>
        protected ITokenEnumerator Enumerator { get; }
    }
}