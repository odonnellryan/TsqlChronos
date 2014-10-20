using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TsqlChronos
{
    internal sealed class Cache
    {
        private readonly IResultProvider provider;
        private readonly ConcurrentDictionary<int, CacheEntry> entries = new ConcurrentDictionary<int, CacheEntry>();

        public Cache(IResultProvider provider)
        {
            this.provider = provider;
        }

        public void Notify(UpdateStatement statement)
        {
            IReadOnlyCollection<CacheEntry> allEntries;
            lock (this)
            {
                allEntries = entries.Select(f => f.Value).ToArray();
                foreach (var entry in allEntries)
                    entry.Indict();
            }

            foreach (var entry in allEntries)
            {
                if (Judge.IsGuilty(entry, statement))
                    entry.Convicted();
                else
                    entry.Acquited();
            }
        }
    }
}
