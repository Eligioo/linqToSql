using ORM.Lexicography.Tokens;

namespace ORM.AST
{
    class Node
    {
        private readonly Token token;
        private List<Node> children;
        private List<Lexicography.Token> tokens;
        private Node? next;

        public Node(Token token, List<Lexicography.Token> tokens)
        {
            this.token = token;
            this.tokens = tokens;
            this.children = new List<Node>();
            this.next = null;

            this.Organize();
        }

        public void Append(Node node)
        {
            // Incorrect implementation! Needs to be the deepest node of th AST.
            this.next = node;
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
            else if (this.token == Token.FROM)
            {
                // Verify if only one token is passed as parameter OR the token is not identified as a word.
                if (this.tokens.Count != 1 || this.tokens[0].Type() != Token.WORD)
                {
                    throw new Exception("FROM: incorrect tokens or token type is not word.");
                }
                var node = new Node(Token.WORD, new List<Lexicography.Token>() { this.tokens[0] });
                this.children.Add(node);

                // Remove WORD token because information has been moved to child.
                this.tokens.Clear();
            }
        }

        public Node? NextNode()
        {
            return this.next;
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