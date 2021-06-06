/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.sockets
{
    /// <summary>
    /// [sockets.users] slot that returns currently connected users.
    /// </summary>
    [Slot(Name = "sockets.users")]
    public class Users : ISlot
    {
        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Retrieving arguments.
            var args = GetArguments(input);

            // House cleaning.
            input.Clear();
            input.Value = null;

            // Retrieving users and iterating through them.
            foreach (var idxUser in MagicHub.GetUsers(args.Filter, args.Offset, args.Limit))
            {
                // Creating a return node for currently iterated user and each connection user has.
                var cur = new Node(".");
                var username = new Node("username", idxUser.Username);
                cur.Add(username);
                var roles = new Node("connections");
                foreach (var idxRole in idxUser.Connections)
                {
                    roles.Add(new Node(".", idxRole));
                }
                cur.Add(roles);

                // Adding currently iterated user to return node.
                input.Add(cur);
            }
        }

        #region [ -- Private helper methods -- ]

        /*
         * Returns arguments to caller.
         */
        static (string? Filter, int Offset, int Limit) GetArguments(Node input)
        {
            // Retrieving arguments, making sure we default to sane values.
            var filter = input.Children.FirstOrDefault(x => x.Name == "filter")?.GetEx<string>();
            var offset = input.Children.FirstOrDefault(x => x.Name == "offset")?.GetEx<int>() ?? 0;
            var limit = input.Children.FirstOrDefault(x => x.Name == "limit")?.GetEx<int>() ?? 10;

            // Returning arguments to caller.
            return (filter, offset, limit);
        }

        #endregion
    }
}
