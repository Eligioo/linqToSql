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
                else if (currentNode.TokenType() == Token.FROM)
                {
                    this.sqlQuery += " FROM";
                    this.sqlQuery += " " + currentNode.Children()
                                        .First()
                                        .GetTokens()
                                        .First()
                                        .Characters();
                }
                currentNode = currentNode.NextNode();
            }

            return this.sqlQuery.Trim();
        }
    }
}