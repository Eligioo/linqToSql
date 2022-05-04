using ORM.Lexicography.Tokens;

namespace ORM.AST
{
    class Node
    {
        private readonly Token token;
        private List<Node> children;
        private List<Lexicography.Token> tokens;
        private Node? rhs;

        public Node(Token token, List<Lexicography.Token> tokens)
        {
            this.token = token;
            this.tokens = tokens;
            this.children = new List<Node>();
            this.rhs = null;

            this.Organize();
        }

        public void Append(Node node)
        {
            this.rhs = node;
        }

        public List<Node> Children()
        {
            return this.children;
        }

        private void Organize()
        {
            if (this.token == Token.SELECT)
            {
                // Only keep OBJECT_PROPERTY tokens.
                this.tokens = this.tokens.Where(t => t.Type() == Token.OBJECT_PROPERTY).ToList();

                // For every OBJECT_PROPERTY, append as child.
                foreach (var token in this.tokens)
                {
                    var node = new Node(Token.OBJECT_PROPERTY, new List<Lexicography.Token>() { token });
                    this.children.Add(node);
                }

                // Remove OBJECT_PROPERTY tokens because information has been moved to children.
                this.tokens.Clear();
            }
        }

        public Node? NextNode()
        {
            return this.rhs;
        }

        public List<Lexicography.Token> GetTokens()
        {
            return this.tokens;
        }

        public Token TokenType()
        {
            return this.token;
        }
    }
}