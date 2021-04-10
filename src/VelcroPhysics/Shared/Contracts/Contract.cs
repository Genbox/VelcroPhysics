using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VelcroPhysics.Shared.Contracts
{
    public static class Contract
    {
        [Conditional("DEBUG")]
        public static void Requires(bool condition, string message)
        {
            if (condition)
                return;

            message = BuildMessage("REQUIRED", message);
            throw new RequiredException(message);
        }

        [Conditional("DEBUG")]
        public static void Warn(bool condition, string message)
        {
            message = BuildMessage("WARNING", message);
            Debug.WriteLineIf(!condition, message);
        }

        [Conditional("DEBUG")]
        public static void Ensures(bool condition, string message)
        {
            if (condition)
                return;

            message = BuildMessage("ENSURANCE", message);
            throw new EnsuresException(message);
        }

        [Conditional("DEBUG")]
        public static void RequireForAll<T>(IEnumerable<T> value, Predicate<T> check)
        {
            foreach (T item in value)
            {
                Requires(check(item), "Failed on: " + item);
            }
        }

        [Conditional("DEBUG")]
        public static void Fail(string message)
        {
            message = BuildMessage("FAILURE", message);
            throw new RequiredException(message);
        }

        private static string BuildMessage(string type, string message)
        {
            string stackTrace = string.Join(Environment.NewLine, Environment.StackTrace.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Skip(3));
            return message == null ? string.Empty : type + ": " + message + Environment.NewLine + stackTrace;
        }
    }
}