using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VelcroPhysics.Shared.Contracts
{
    public static class Contract
    {
        public static void Requires(bool required, string message = null)
        {
            Debug.Assert(required, message);

#if !DEBUG
            if (!required)
                throw new RequiredException(message);
#endif
        }

        public static void Ensures(bool ensures, string message = null)
        {
            Debug.Assert(ensures, message);

#if !DEBUG
            if (!ensures)
                throw new EnsuresException(message);
#endif
        }

        public static void ForAll<T>(IEnumerable<T> value, Predicate<T> check)
        {
            foreach (T item in value)
            {
                Requires(check(item));
            }
        }

        public static void Fail(string reason)
        {
            Debug.Fail(reason);

#if !DEBUG
            throw new RequiredException(reason);
#endif
        }
    }
}