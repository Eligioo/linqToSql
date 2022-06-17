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
                        AST.Node child = currentNode.Children().First();
                        string property = child
                                        .GetTokens()
                                        .First()
                                        .Characters()
                                        .Split('.')
                                        .Last();

                        string op = child.TokenType() switch
                        {
                            Token.IS_EQUAL => "=",
                            _ => throw new Exception("WHERE: can't find operator.")
                        };

                        string value = child
                                        .GetTokens()
                                        .Last()
                                        .Characters();

                        this.sqlQuery += " " + property + " " + op + " " + value;
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
    }
}