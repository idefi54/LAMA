using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    /// <summary>
    /// Static class defining special characters used in the application.
    /// </summary>
    public static class SpecialCharacters
    {
        /// <summary>
        /// Character indicating an end of the message (used because the messages can be concatenated)
        /// </summary>
        public static char messageSeparator = '';
        /// <summary>
        /// Character separating different parts of the messages sent over the network.
        /// </summary>
        public static char messagePartSeparator = '';
        /// <summary>
        /// Character used to separate the attributes of a serialized object (in a message)
        /// </summary>
        public static char attributesSeparator = '¦';
        /// <summary>
        /// Character indicating, that a channel has been archived.
        /// </summary>
        public static char archivedChannelIndicator = '⍧';
    }
}
