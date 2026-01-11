namespace Billetterie_Spectacles.Domain.Exceptions
{
    public class DomainException : Exception
    {
        /// <summary>
        /// HTTP Status: 400 bad Request
        /// Règle métier violée
        /// </summary>
        /// <param name="message"></param>
        public DomainException(string message) : base(message)
        {
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}