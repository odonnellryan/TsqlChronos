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
            var SQLQuery = @"SELECT order_id
                            FROM suppliers 
                            WHERE a > 3";

            var sql = SqlParser.Parse(SQLQuery);
            var fragments = SqlUtil.EnumerateAll(sql);

            Evidence evidence = new Evidence();
            evidence.GatherEvidence(fragments);
            Console.WriteLine(evidence.BooleanEvalsCollection);
        }
      
    }
}