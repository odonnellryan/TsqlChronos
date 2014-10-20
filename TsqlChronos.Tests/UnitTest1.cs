using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsqlChronos.Util;
using System.Collections.Generic;

namespace TsqlChronos.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var SQLQuery = @"SELECT order_id
                            FROM suppliers 
                            WHERE a > 3 
                            AND b = 1 
                            OR c > 2 
                            AND d IN ('Blade')
                            OR e like ('%Frame%')";

            var sql = SqlParser.Parse(SQLQuery);
            var fragments = SqlUtil.EnumerateAll(sql);

            foreach (var fragment in fragments)
            {
                Console.WriteLine(fragment);
                if (fragment is QuerySpecification)
                {
                    var spec = (QuerySpecification)fragment;
                    var tbl = spec.FromClause.TableReferences[0] as NamedTableReference;
                    Console.WriteLine(spec.SelectElements.Count + " select elements, from table "
                        + tbl.SchemaObject.Identifiers[0].Value);
                }
                if (fragment is SelectStatement)
                {
                    var select = (SelectStatement)fragment;
                    // cast queryexpression to queryspecification
                    var qex = select.QueryExpression as QuerySpecification;
                    var whereClause = qex.WhereClause as WhereClause;
                    var booleanEx = whereClause.SearchCondition as BooleanBinaryExpression;
                    Console.WriteLine(qex.WhereClause.SearchCondition);
                    Console.WriteLine("testing");
                }
            }
            
        }
      
    }
}