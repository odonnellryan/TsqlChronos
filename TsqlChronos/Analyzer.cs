using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TsqlChronos
{
    public class Evidence
    {
        public class BooleanEvals : Dictionary<string, Dictionary<string, List<string>>>
        {
            // Example of use: { {"Column_A", {"GreaterThan", ["10","11"] } } }
        }
        // we'll make the "ands" implicit, but we'll "split" each time we encounter an "or" statement.
        // Logic is that we'll loop through the below list and execute all the operators
        // should be good
        // maybe.... 
        public List<BooleanEvals> BooleanEvalsCollection = new List<BooleanEvals>();
        public List<QuerySpecification> querySpecs = new List<QuerySpecification>();
        // hashset because it is my favorite
        public HashSet<string> referencedTables = new HashSet<string>();
        public HashSet<string> referencedColumns = new HashSet<string>();

        public void GatherEvidence(IEnumerable<TSqlFragment> fragments)
        {
            foreach (var fragment in fragments)
            {
                if (fragment is WhereClause)
                {
                    var whereClause = fragment as WhereClause;
                    handleWhereClause(whereClause);
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
            BooleanBinaryExpressionType clause2 = (BooleanBinaryExpressionType)clause.BinaryExpressionType;
            if (clause.BinaryExpressionType.ToString() == "Or")
            {
                BooleanEvalsCollection.Add(new BooleanEvals());
                firstIndex = BooleanEvalsCollection.Count - 2;
                secondIndex = BooleanEvalsCollection.Count - 1;
            }
            else if (clause.BinaryExpressionType.ToString() == "And")
            {
                firstIndex = secondIndex = index;
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
            if (clause.SecondExpression is BooleanComparisonExpression)
            {
                BooleanComparisonExpression secondEx = (BooleanComparisonExpression)clause.SecondExpression;
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
            BooleanEvalsCollection.Add(new BooleanEvals());
            int index = BooleanEvalsCollection.Count - 1;
            if (whereClause.SearchCondition is BooleanBinaryExpression)
            {
                BooleanBinaryExpression boolBinEx = whereClause.SearchCondition as BooleanBinaryExpression;
                handleBooleanBinaryExpression(boolBinEx, index);
            }
            if (whereClause.SearchCondition is BooleanComparisonExpression)
            {
                BooleanComparisonExpression boolCompEx = whereClause.SearchCondition as BooleanComparisonExpression;
                handleBooleanComparisonExpression(boolCompEx, index);
            }
            if (whereClause.SearchCondition is LikePredicate)
            {
                LikePredicate likePredicate = whereClause.SearchCondition as LikePredicate;
                handleLikePredicate(likePredicate, index);
            }
            if (whereClause.SearchCondition is InPredicate)
            {
                InPredicate inPredicate = whereClause.SearchCondition as InPredicate;
                handleInPredicate(inPredicate, index);
            }
        }

        private void handleBooleanComparisonExpression(BooleanComparisonExpression boolCompEx, int index)
        {
            string columnName = null;
            if (boolCompEx.FirstExpression is ColumnReferenceExpression)
            {
                ColumnReferenceExpression firstEx = boolCompEx.FirstExpression as ColumnReferenceExpression;
                columnName = firstEx.MultiPartIdentifier.Identifiers[0].Value;
            }
            if (boolCompEx.FirstExpression is StringLiteral)
            {
                StringLiteral firstEx = boolCompEx.FirstExpression as StringLiteral;
                columnName = firstEx.Value;
            }
            if (boolCompEx.FirstExpression is IntegerLiteral)
            {
                IntegerLiteral firstEx = boolCompEx.FirstExpression as IntegerLiteral;
                columnName = firstEx.Value.ToString();
            }
            string element = null;
            if (boolCompEx.SecondExpression is ColumnReferenceExpression)
            {
                ColumnReferenceExpression secondEx = boolCompEx.SecondExpression as ColumnReferenceExpression;
                element = secondEx.MultiPartIdentifier.Identifiers[0].Value;
            }
            if (boolCompEx.SecondExpression is StringLiteral)
            {
                StringLiteral secondEx = boolCompEx.SecondExpression as StringLiteral;
                element = secondEx.Value;
            }
            if (boolCompEx.SecondExpression is IntegerLiteral)
            {
                IntegerLiteral secondEx = boolCompEx.SecondExpression as IntegerLiteral;
                element = secondEx.Value.ToString();
            }
            string _operator = boolCompEx.ComparisonType.ToString();
            handleDictUpdate(columnName, _operator, element, index);
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
            // Example dictionary: { {"Column_A", {"GreaterThan", ["10","11"] } } }
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

    }
}