using ORM.Lexicography;
using System.Linq.Expressions;
using Pluralize.NET;
using MySql.Data.MySqlClient;

namespace ORM
{
    abstract class Model<T>
    {
        public PendingQuery<Selector> Select<Selector>(Expression<Func<T, Selector>> s)
        {
            IPluralize pluralizer = new Pluralizer();
            var objectName = typeof(T).ToString().Split('.').Last().ToLower();
            return new PendingQuery<Selector>(s.ToString(), pluralizer.Pluralize(objectName));
        }
    }

    class PendingQuery<T>
    {
        private AST.Node ast;
        public PendingQuery(String selector, String modelPluralized)
        {
            // Tokenize and store SELECT
            var tokens = Lexer.Tokenize(selector);
            this.ast = new AST.Node(Lexicography.Tokens.Token.SELECT, tokens);

            // Implicity tokenize and store FROM
            tokens = Lexer.Tokenize(modelPluralized);
            this.ast.Append(new AST.Node(Lexicography.Tokens.Token.FROM, tokens));
        }

        public PendingQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            string trimmedPredicate = predicate.ToString();
            // Trim everything before the actual expression
            trimmedPredicate = trimmedPredicate.Substring(trimmedPredicate.LastIndexOf("=> ") + 3);

            var tokens = Lexer
                        .Tokenize(trimmedPredicate)
                        .FindAll(t => t.Type() != Lexicography.Tokens.Token.UNDEFINED_TOKEN);

            var whereAST = this.ParseWhereStatement(tokens, new AST.Node(Lexicography.Tokens.Token.WHERE, new List<Token>()));
            this.ast.Append(whereAST);
            return this;
        }

        public PendingQuery<T> OrderBy(Expression<Func<T, int>> element, Lexicography.Tokens.Ordering ordering = Lexicography.Tokens.Ordering.Descending)
        {
            var orderingToken = new Token(Lexicography.Tokens.Token.ORDERING, ordering.ToString());
            var orderByNode = new AST.Node(
                    Lexicography.Tokens.Token.ORDER_BY,
                    Lexer.Tokenize(element.ToString())
                    .Append(orderingToken)
                    .ToList()
                );
            this.ast.Append(orderByNode);
            return this;
        }

        private AST.Node ParseWhereStatement(List<Token> tokens, AST.Node whereAST)
        {
            var leftPredicateCombiner = new List<(int, Lexicography.Tokens.Token)>()
            {
                (tokens.FindIndex(a => a.Type() == Lexicography.Tokens.Token.AND_ALSO), Lexicography.Tokens.Token.AND_ALSO),
                (tokens.FindIndex(a => a.Type() == Lexicography.Tokens.Token.OR_ELSE), Lexicography.Tokens.Token.OR_ELSE)
            }
            .OrderBy(tuple => tuple.Item1)
            .ToList()
            .Find(tuple => tuple.Item1 > -1);

            // TODO: this computation is not necassary when an AndAlso or OrElse index is already found.
            var leftSidePredicate = new List<(int, Lexicography.Tokens.Token)>()
            {
                (tokens.FindIndex(a => a.Type() == Lexicography.Tokens.Token.IS_EQUAL), Lexicography.Tokens.Token.IS_EQUAL),
                (tokens.FindIndex(a => a.Type() == Lexicography.Tokens.Token.NOT_EQUAL), Lexicography.Tokens.Token.NOT_EQUAL)
            }
            .OrderBy(tuple => tuple.Item1)
            .ToList()
            .Find(tuple => tuple.Item1 > -1);

            // Chosen Lexicography.Tokens.Token.DOT because it's the default value for the Token enum.
            if (leftPredicateCombiner.Item2 != Lexicography.Tokens.Token.DOT)
            {
                var lhs = tokens.TakeWhile(a => a.Type() != leftPredicateCombiner.Item2).ToList();
                var rhs = tokens.GetRange(leftPredicateCombiner.Item1 + 1, tokens.Count - leftPredicateCombiner.Item1 - 1);

                AST.Node predicateCombiner = new AST.Node(leftPredicateCombiner.Item2, new List<Token>());
                whereAST.Append(predicateCombiner);
                ParseWhereStatement(lhs, whereAST);
                ParseWhereStatement(rhs, whereAST);

            }
            // Chosen Lexicography.Tokens.Token.DOT because it's the default value for the Token enum.
            else if (leftSidePredicate.Item2 != Lexicography.Tokens.Token.DOT)
            {
                List<Token> children = new List<Token>();

                // Add lhs and rhs of comparison token
                children.Add(tokens[leftSidePredicate.Item1 - 1]);
                children.Add(tokens[leftSidePredicate.Item1 + 1]);
                tokens.RemoveRange(leftSidePredicate.Item1 - 1, 3);

                whereAST.Last().AddChild(new AST.Node(leftSidePredicate.Item2, children));
                ParseWhereStatement(tokens, whereAST);
            }
            return whereAST;
        }

        public List<T> Execute(String server, String user, String password, String database)
        {
            var g = new Generator.Generator(this.ast).Generate();
            Console.WriteLine(g);

            string cs = @"server=" + server + ";userid=" + user + ";password=" + password + ";database=" + database;
            var con = new MySqlConnection(cs);
            con.Open();

            var reader = new MySqlCommand(g, con).ExecuteReader();

            int fieldCount = reader.FieldCount;
            List<T> list = new List<T>();

            object[] values = new object[fieldCount];
            while (reader.Read())
            {
                reader.GetValues(values);
                var instance = (T)Activator.CreateInstance(typeof(T), values);
                list.Add(instance);
            }

            con.Close();

            return list;
        }
    }
}