/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using Microsoft.Extensions.Configuration;
using magic.node.extensions;

namespace magic.lambda.sockets.slots
{
    /*
     * Helper class for commonalities.
     */
    internal static class Utilities
    {
        /*
         * Returns true if sockets are enabled, otherwise false.
         */
        public static bool SocketEnabled(IConfiguration configuration)
        {
            return configuration["magic:sockets:url"] != null;
        }

        /*
         * Helper method throwing an exception if sockets are not enabled in server.
         */
        public static void ThrowIfNotEnabled(IConfiguration configuration)
        {
            if (!SocketEnabled(configuration))
                throw new HyperlambdaException("Sockets are not enabled for this server");
        }
    }
}
