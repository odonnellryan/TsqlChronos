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
        List<BooleanEvals> BooleanEvalsCollection { get; set; }
        List<QuerySpecification> querySpecs { get; set; }
        // hashset because it is my favorite
        HashSet<string> referencedTables { get; set; }
        HashSet<string> referencedColumns { get; set; }

        private void GatherEvidence(List<TSqlFragment> fragments)
        {
            foreach (var fragment in fragments)
            {
                if (fragment is WhereClause)
                {
                    var whereClause = fragment as WhereClause;
                    var booleanEx = whereClause.SearchCondition as BooleanBinaryExpression;
                    handleBooleanBinaryExpression(booleanEx);
                }
                if (fragment is NamedTableReference)
                {
                    NamedTableReference tableRef = fragment as NamedTableReference;
                    referencedTables.Add(tableRef.SchemaObject.Identifiers[0].Value);
                }
                if (fragment is ColumnReferenceExpression)
                {
                    ColumnReferenceExpression cRefEx = fragment as ColumnReferenceExpression;
                    referencedColumns.Add(cRefEx.MultiPartIdentifier.Identifiers[0].Value);
                }
                if (fragment is QuerySpecification)
                {
                    QuerySpecification querySpec = fragment as QuerySpecification;
                    querySpecs.Add(querySpec);
                }
            }

        }

        private void handleBooleanBinaryExpression(BooleanBinaryExpression clause, int index = 0)
        {
            int firstIndex, secondIndex;
            if (clause.BinaryExpressionType == (BooleanBinaryExpressionType) BinaryExpressionType.BitwiseOr)
            {
                // this will add unneeded dicts, i think. oh well.
                BooleanEvalsCollection.Add(null);
                BooleanEvalsCollection.Add(null);
                firstIndex = BooleanEvalsCollection.Count - 2;
                secondIndex = BooleanEvalsCollection.Count - 1;
            }
            else if (index == 0 && BooleanEvalsCollection.Count == 0)
            {
                BooleanEvalsCollection.Add(null);
                firstIndex = secondIndex = BooleanEvalsCollection.Count - 1;
            }
            else if (clause.BinaryExpressionType == (BooleanBinaryExpressionType) BinaryExpressionType.BitwiseAnd)
            {
                BooleanEvalsCollection.Add(null);
                firstIndex = secondIndex = BooleanEvalsCollection.Count - 1;
            }
            else
            {
                firstIndex = secondIndex = index;
            }
            if (clause.FirstExpression is BooleanBinaryExpression)
            {
                BooleanBinaryExpression firstEx = (BooleanBinaryExpression)clause.FirstExpression;
                handleBooleanBinaryExpression(firstEx, firstIndex);
            }
            if (clause.SecondExpression is BooleanBinaryExpression)
            {
                BooleanBinaryExpression secondEx = (BooleanBinaryExpression)clause.SecondExpression;
                handleBooleanBinaryExpression(secondEx, secondIndex);
            }
            if (clause.FirstExpression is BooleanComparisonExpression)
            {
                BooleanComparisonExpression firstEx = (BooleanComparisonExpression)clause.FirstExpression;
                handleBooleanComparisonExpression(firstEx, firstIndex);
            }
            if (clause.FirstExpression is BooleanComparisonExpression)
            {
                BooleanComparisonExpression secondEx = (BooleanComparisonExpression)clause.FirstExpression;
                handleBooleanComparisonExpression(secondEx, secondIndex);
            }
            if (clause.FirstExpression is InPredicate)
            {
                InPredicate inPredicate = clause.FirstExpression as InPredicate;
                handleInPredicate(inPredicate, firstIndex);
            }
            if (clause.SecondExpression is InPredicate)
            {
                InPredicate inPredicate = clause.SecondExpression as InPredicate;
                handleInPredicate(inPredicate, firstIndex);
            }
            if (clause.FirstExpression is LikePredicate)
            {
                LikePredicate likePredicate = clause.FirstExpression as LikePredicate;
                handleLikePredicate(likePredicate, firstIndex);
            }
            if (clause.SecondExpression is LikePredicate)
            {
                LikePredicate likePredicate = clause.SecondExpression as LikePredicate;
                handleLikePredicate(likePredicate, secondIndex);
            }
        }

        private void handleWhereClause(WhereClause whereClause) 
        {
            if (whereClause.SearchCondition is BooleanBinaryExpression)
            {
                BooleanBinaryExpression boolBinEx = whereClause.SearchCondition as BooleanBinaryExpression;
                handleBooleanBinaryExpression(boolBinEx);
            }
            if (whereClause.SearchCondition is BooleanBinaryExpression)
            {
                BooleanBinaryExpression boolBinEx = whereClause.SearchCondition as BooleanBinaryExpression;
                handleBooleanBinaryExpression(boolBinEx);
            }
            if (whereClause.SearchCondition is BooleanBinaryExpression)
            {
                BooleanBinaryExpression boolBinEx = whereClause.SearchCondition as BooleanBinaryExpression;
                handleBooleanBinaryExpression(boolBinEx);
            }
            if (whereClause.SearchCondition is BooleanBinaryExpression)
            {
                BooleanBinaryExpression boolBinEx = whereClause.SearchCondition as BooleanBinaryExpression;
                handleBooleanBinaryExpression(boolBinEx);
            }
            handleChildExpressions<BooleanExpression>(whereClause.SearchCondition);
        }

        private void handleChildExpressions<T>(T clause) where T : BooleanExpression
        {
            
        }

        private void handleBooleanComparisonExpression(BooleanComparisonExpression boolCompEx, int index)
        {
            string _operator = boolCompEx.ComparisonType.ToString();

        }

        private void handleInPredicate(InPredicate inPredicate, int index)
        {
            ColumnReferenceExpression columnRef = inPredicate.Expression as ColumnReferenceExpression;
            string columnName = columnRef.MultiPartIdentifier.Identifiers[0].Value;
            StringLiteral stringLit = inPredicate.Values[0] as StringLiteral;
            handleDictUpdate(columnName, "IN", stringLit.Value, index);
        }

        private void handleLikePredicate(LikePredicate likePredicate, int index)
        {
            // eventually we should replace LIKE bits with regex bits?
            ColumnReferenceExpression columnRef = likePredicate.FirstExpression as ColumnReferenceExpression;
            string columnName = columnRef.MultiPartIdentifier.Identifiers[0].Value;
            if (likePredicate.SecondExpression is StringLiteral)
            {
                StringLiteral stringLit = likePredicate.SecondExpression as StringLiteral;
                handleDictUpdate(columnName, "LIKE", stringLit.Value, index);
            }
            else if (likePredicate.SecondExpression is ParenthesisExpression)
            {
                ParenthesisExpression pEx = likePredicate.SecondExpression as ParenthesisExpression;
                StringLiteral stringLit = pEx.Expression as StringLiteral;
                handleDictUpdate(columnName, "LIKE", stringLit.Value, index);
            }
        }

        private void handleDictUpdate(string columnName, string _operator, string element, int index)
        {
            // Example dictionary: { {"Column_A", {">", ["10","11"] } } }
            if (!BooleanEvalsCollection[index].ContainsKey(columnName))
            {
                BooleanEvalsCollection[index][columnName] = new Dictionary<string, List<string>>();
            }
            if (!BooleanEvalsCollection[index][columnName].ContainsKey(_operator))
            {
                BooleanEvalsCollection[index][columnName][_operator] = new List<string>();
            }
            BooleanEvalsCollection[index][columnName][_operator].Add(element);
        }

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

    }
}