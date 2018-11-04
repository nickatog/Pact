using System;

namespace Pact.Extensions.Contract
{
    public static class ContractExtensions
    {
        public static T Require<T>(
            this T value,
            string name)
            where T : class
        {
            return value ?? throw new ArgumentNullException(name);
        }
    }
}
