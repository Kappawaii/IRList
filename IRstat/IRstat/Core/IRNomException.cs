using System;

namespace IRStat.Core
{
    [Serializable]
    class IRNomException : Exception
    {
        public IRNomException(string message) : base(message)
        {

        }
    }
}
