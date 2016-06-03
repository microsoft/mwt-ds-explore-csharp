﻿using System.Globalization;
using System.Text;

namespace Microsoft.Research.MultiWorldTesting.ExploreLibrary.SingleAction
{
    /// <summary>
	/// A sample recorder class that converts the exploration tuple into string format.
	/// </summary>
	/// <typeparam name="TContext">The Context type.</typeparam>
	public class StringRecorder<TContext> : IRecorder<TContext, int>
	{
        private StringBuilder recordingBuilder;

        public StringRecorder()
		{
            recordingBuilder = new StringBuilder();
		}


        /// <summary>
        /// Records the exploration data associated with a given decision.
        /// This implementation should be thread-safe if multithreading is needed.
        /// </summary>
        /// <param name="context">A user-defined context for the decision.</param>
        /// <param name="action">Chosen by an exploration algorithm given context.</param>
        /// <param name="probability">The probability of the chosen action given context.</param>
        /// <param name="uniqueKey">A user-defined identifer for the decision.</param>
        public void Record(TContext context, int value, object explorerState, object mapperState, string uniqueKey)
        {
            recordingBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
            recordingBuilder.Append(' ');
            recordingBuilder.Append(uniqueKey);
            recordingBuilder.Append(' ');

            recordingBuilder.Append(((GenericExplorerState)explorerState).Probability.ToString("0.00000", CultureInfo.InvariantCulture));

            recordingBuilder.Append(" | ");
            recordingBuilder.Append(context.ToString());
            recordingBuilder.Append("\n");
        }

		/// <summary>
		/// Gets the content of the recording so far as a string and optionally clears internal content.
		/// </summary>
		/// <param name="flush">A boolean value indicating whether to clear the internal content.</param>
		/// <returns>
		/// A string with recording content.
		/// </returns>
        public string GetRecording(bool flush = true)
		{
            string recording = this.recordingBuilder.ToString();

            if (flush)
            {
                this.recordingBuilder.Clear();
            }

            return recording;
		}
    };
}
