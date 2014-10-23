using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsqlChronos.Util;
using TsqlChronos;
using System.Collections.Generic;

namespace TsqlChronos.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var SQLQuery = @"INSERT INTO order_id
                           (A,B,C)
                           VALUES (1,2,3,4)";

            var sql = SqlParser.Parse(SQLQuery);
            var fragments = SqlUtil.EnumerateAll(sql);
            foreach (var fragment in fragments)
            {
                Console.WriteLine(fragment);
            }
            //Evidence evidence = new Evidence();
            //evidence.GatherEvidence(fragments);
            //Console.WriteLine(evidence.BooleanEvalsCollection);
        }

    }
}