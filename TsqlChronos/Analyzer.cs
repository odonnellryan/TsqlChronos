using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TsqlChronos
{
    internal sealed class Evidence
    {
        internal sealed class BooleanEvals : Dictionary<string, Dictionary<string, List<string>>>
        {
            // Example of use: { {"Column_A", {">", ["10","11"] } } }
        }
        // we'll make the "ands" implicit, but we'll "split" each time we encounter an "or" statement.
        // Logic is that we'll loop through the below list and execute all the operators
        // should be good
        // maybe.... 
        // coffee
        List<BooleanEvals> BooleanEvalsCollection { get; set; }
        QuerySpecification querySpec { get; set; }
        // hashset because it is my favorite
        HashSet<string> referencedTables { get; set; }
        HashSet<string> referencedColumns { get; set; }
    }

    internal sealed class Judge
    {
        public static bool IsGuilty(CacheEntry entry, UpdateStatement statement)
        {
            SelectStatement selectStatement = entry.Statement;
            return true;
        }

        private static Evidence BreakdownSingleSelect(SelectStatement statements)
        {
            Evidence evidence = new Evidence();
            Console.WriteLine(evidence);
            return evidence;
        }

        private static Dictionary<string, List<string>> BreakdownWhereClause(WhereClause whereClause)
        {
            Dictionary<string, List<string>> booleanEvals = new Dictionary<string, List<string>>();
            return booleanEvals;
        }

        private static Hashtable GetSelectStatements(SelectStatement statements)
        {
            Hashtable selectStatements = new Hashtable();

            //foreach(var statement in statements)
            // {
            //    if(statements == QuerySpecification)
            //}

            return selectStatements;
        }
    }
}