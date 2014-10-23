using Microsoft.SqlServer.TransactSql.ScriptDom;
using TsqlChronos.Util;
using System.Collections.Generic;

namespace TsqlChronos
{

    internal sealed class Judge
    {
        internal sealed class Notes
        {
            public Dictionary<string, List<string>> insertIntoDetails = new Dictionary<string, List<string>>();
        }

        Notes notes = new Notes();
        public static bool IsGuilty(Evidence evidence, UpdateStatement statement)
        {
            
            return true;
        }

        private bool AnalyzeUpdateStatement(UpdateStatement statement)
        {
            var fragments = SqlUtil.EnumerateAll(statement);
            foreach (var fragment in fragments)
            {
                if (fragment is InsertStatement)
                {
                    InsertStatement insert = fragment as InsertStatement;
                    //foreach (var )
                }
            }
            return false;
        }

        private void handleDictUpdate(string column, string value)
        {
            if (!notes.insertIntoDetails.ContainsKey(column))
            {
                notes.insertIntoDetails[column] = new List<string>();
            }
            if (!notes.insertIntoDetails[column].Contains(value))
            {
                notes.insertIntoDetails[column].Add(value);
            }
        }

    }
}