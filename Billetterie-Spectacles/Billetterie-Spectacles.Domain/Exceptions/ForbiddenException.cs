using System;
using System.Collections.Generic;
using System.Text;

namespace Billetterie_Spectacles.Domain.Exceptions
{
    /// <summary>
    /// HTTP Status: 403 Forbidden
    /// Permission insuffisante
    /// Exception levée lorsqu'un utilisateur n'a pas les permissions pour effectuer une action
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message)
        {
        }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
