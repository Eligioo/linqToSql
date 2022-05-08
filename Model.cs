using ORM.Lexicography;
using System.Linq.Expressions;

namespace ORM
{
    abstract class Model<T>
    {
        public PendingQuery<T> Select<Selector>(Expression<Func<T, Selector>> s)
        {
            return new PendingQuery<T>(s.ToString());
        }
    }

    class PendingQuery<T>
    {
        private AST.Node ast;
        public PendingQuery(String selector)
        {
            var tokens = Lexer.Tokenize(selector);
            this.ast = new AST.Node(Lexicography.Tokens.Token.SELECT, tokens);
        }

        public PendingQuery<T> From(String selector)
        {
            var tokens = Lexer.Tokenize(selector);
            this.ast.Append(new AST.Node(Lexicography.Tokens.Token.FROM, tokens));
            return this;
        }

        public List<T> Execute()
        {
            var g = new Generator.Generator(this.ast).Generate();
            Console.WriteLine(g);
            return new List<T>();
        }
    }
}