using ORM.AST;
using ORM.Lexicography.Tokens;

namespace ORM.Generator
{
    class Generator
    {
        private readonly Node ast;
        private string sqlQuery;

        public Generator(Node ast)
        {
            this.ast = ast;
            this.sqlQuery = "";
        }

        public string Generate()
        {
            var currentNode = this.ast;
            while (currentNode != null)
            {
                // SELECT SECTION
                if (currentNode.TokenType() == Token.SELECT)
                {
                    this.sqlQuery += "SELECT";
                    foreach (var child in currentNode.Children())
                    {
                        var token = child.GetTokens().First();
                        var chars = token.Characters()
                                        .Split('.')
                                        .Last();
                        this.sqlQuery += " " + chars + ",";
                    }
                    this.sqlQuery = this.sqlQuery.TrimEnd(',');
                }
                // FROM SECTION
                else if (currentNode.TokenType() == Token.FROM)
                {
                    this.sqlQuery += " FROM";
                    this.sqlQuery += " " + currentNode.Children()
                                        .First()
                                        .GetTokens()
                                        .First()
                                        .Characters();
                }
                // WHERE SECTION
                else if (currentNode.TokenType() == Token.WHERE)
                {
                    this.sqlQuery += " WHERE";

                    // Where clause only has one predicate. No extra AndAlso or OrElse.
                    if (currentNode.Children().Count() == 1)
                    {
                        this.sqlQuery += this.constructWherePredicateString(currentNode.Children().First());
                    }
                    // Where clause that contains AndAlso or OrElse.
                    else if (currentNode.Children().Count() == 0)
                    {
                        var innerWhereNode = currentNode.NextNode();
                        while (innerWhereNode != null && TokenCls.isTokenPartOfGroup(innerWhereNode.TokenType(), TokenCls.ComparisonTokenGroup))
                        {
                            int predicateChildrenCount = innerWhereNode.Children().Count();

                            // Small optimization reducing code duplication. If the count = 2, means we have a "predicate AndAlso/OrElse predicate" construct.
                            if (predicateChildrenCount == 1 || predicateChildrenCount == 2)
                            {
                                // LHS of predicate
                                this.sqlQuery += this.constructWherePredicateString(innerWhereNode.Children().First());

                                // AndAlso or OrElse predicate
                                switch (innerWhereNode.TokenType())
                                {
                                    case Token.AND_ALSO:
                                        this.sqlQuery += " AND";
                                        break;
                                    case Token.OR_ELSE:
                                        this.sqlQuery += " OR";
                                        break;
                                }

                                // RHS of combined predicate
                                if (predicateChildrenCount == 2)
                                {
                                    this.sqlQuery += this.constructWherePredicateString(innerWhereNode.Children().Last());
                                }

                            }
                            else
                            {
                                throw new Exception("WHERE CLAUSE: invalid constructed.");
                            }

                            innerWhereNode = innerWhereNode.NextNode();
                        }
                    }
                    else if (currentNode.Children().Count() > 1)
                    {
                        throw new Exception("WHERE: incorrect construct of node. 2 or more children.");
                    }
                }
                currentNode = currentNode.NextNode();
            }

            return this.sqlQuery.Trim();
        }

        private string constructWherePredicateString(AST.Node node)
        {
            string property = node
                            .GetTokens()
                            .First()
                            .Characters()
                            .Split('.')
                            .Last();

            string op = node.TokenType() switch
            {
                Token.IS_EQUAL => "=",
                Token.NOT_EQUAL => "!=",
                _ => throw new Exception("WHERE: can't find operator.")
            };

            string value = node
                            .GetTokens()
                            .Last()
                            .Characters();
            return " " + property + " " + op + " " + value;
        }
    }
}