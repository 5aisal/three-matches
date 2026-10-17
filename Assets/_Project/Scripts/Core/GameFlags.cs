using System;
using System.Collections.Generic;

namespace ThreeMatches.Core
{
    /// <summary>Named story flags ("HasMatches", ...). Plain C# so rules can be tested.</summary>
    public sealed class GameFlags
    {
        public const string HasMatches = "HasMatches";

        private readonly HashSet<string> set = new HashSet<string>();

        public event Action<string> FlagSet;

        public bool Has(string flag) => set.Contains(flag);

        /// <summary>Returns true only the first time the flag is set.</summary>
        public bool Set(string flag)
        {
            if (string.IsNullOrEmpty(flag) || !set.Add(flag)) return false;
            FlagSet?.Invoke(flag);
            return true;
        }
    }
}
