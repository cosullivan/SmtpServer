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
        /// <typeparam name="TOut1">The type of the first out parameter.</typeparam>
        /// <typeparam name="TOut2">The type of the second out parameter.</typeparam>
        /// <param name="found1">The first out parameter that was found during the make operation.</param>
        /// <param name="found2">The first out parameter that was found during the make operation.</param>
        /// <returns>true if the make operation found a parameter, false if not.</returns>
        protected delegate bool TryMakeDelegate<TOut1, TOut2>(out TOut1 found1, out TOut2 found2);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="enumerator">The token enumerator to handle the incoming tokens.</param>
        protected TokenParser(ITokenEnumerator enumerator)
        {
            Enumerator = enumerator;
        }

        /// <summary>
        /// Try to take the tokens in sequence.
        /// </summary>
        /// <param name="tokens">The list of tokens to take in sequence.</param>
        /// <returns>true if the given list of tokens could be made in sequence, false if not.</returns>
        protected bool TryTakeTokens(params Token[] tokens)
        {
            var checkpoint = Enumerator.Checkpoint();

            foreach (var token in tokens)
            {
                if (Enumerator.Take() != token)
                {
                    checkpoint.Rollback();
                    return false;
                }
            }

            checkpoint.Dispose();
            return true;
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
        /// <param name="found1">The first parameter that was returned from the matching function.</param>
        /// <param name="found2">The second parameter that was returned from the matching function.</param>
        /// <returns>true if the match could be made, false if not.</returns>
        protected bool TryMake<TOut1, TOut2>(TryMakeDelegate<TOut1, TOut2> @delegate, out TOut1 found1, out TOut2 found2)
        {
            var checkpoint = Enumerator.Checkpoint();

            if (@delegate(out found1, out found2) == false)
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