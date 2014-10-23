using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TsqlChronos
{
    internal sealed class CacheEntry
    {
        private readonly IResultProvider provider;

        public enum CacheEntryState
        {
            Valid,
            Invalid,
            Processing,
        }


        public SelectStatement Statement { get; set; }
        public int HashCode { get; set; }

        private Mutex lck = new Mutex(false);
        private ResultSet CacheSet;

        public Evidence evidence;

        public CacheEntryState State { get; private set; }


        public CacheEntry(IResultProvider provider)
        {
            this.provider = provider;
        }

        public void Convicted()
        {
            State = CacheEntryState.Invalid;

            Task.Run(() => Appeal());
        }

        private async void Appeal()
        {
            try
            {
                var results = await provider.GetResults(Statement);
                State = CacheEntryState.Valid;
                CacheSet = results;
                lck.ReleaseMutex();
            }
            catch (Exception)
            {
                State = CacheEntryState.Valid;

                lck.ReleaseMutex();
            }
        }

        public void Indict()
        {
            lck.WaitOne();
            State = CacheEntryState.Processing;
        }

        public void Acquited()
        {
            State = CacheEntryState.Valid;
            lck.ReleaseMutex();
        }

        public ResultSet GetValue()
        {
            lck.WaitOne();
            Debug.Assert(State == CacheEntryState.Valid);
            var set = CacheSet;
            lck.ReleaseMutex();
            return set;
        }
    }
}