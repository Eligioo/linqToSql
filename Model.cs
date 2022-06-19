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

        private AST.Node ParseWhereStatement(List<Token> tokens, AST.Node whereAST)
        {
            var indexAndAlso = tokens.FindIndex(a => a.Type() == Lexicography.Tokens.Token.AND_ALSO);
            var indexOrElse = tokens.FindIndex(a => a.Type() == Lexicography.Tokens.Token.OR_ELSE);

            // TODO: This if and next else if can get merged.
            if (indexAndAlso != -1 && (indexAndAlso < indexOrElse || indexOrElse == -1))
            {
                var lhs = tokens.TakeWhile(a => a.Type() != Lexicography.Tokens.Token.AND_ALSO).ToList();
                var rhs = tokens.GetRange(indexAndAlso + 1, tokens.Count - indexAndAlso - 1);

                AST.Node AndAlso = new AST.Node(Lexicography.Tokens.Token.AND_ALSO, new List<Token>());
                whereAST.Append(AndAlso);
                ParseWhereStatement(lhs, whereAST);
                ParseWhereStatement(rhs, whereAST);

            }
            else if (indexOrElse != -1 && (indexOrElse < indexAndAlso || indexAndAlso == -1))
            {
                var lhs = tokens.TakeWhile(a => a.Type() != Lexicography.Tokens.Token.OR_ELSE).ToList();
                var rhs = tokens.GetRange(indexOrElse + 1, tokens.Count - indexOrElse - 1);

                AST.Node OrElse = new AST.Node(Lexicography.Tokens.Token.OR_ELSE, new List<Token>());
                whereAST.Append(OrElse);
                ParseWhereStatement(lhs, whereAST);
                ParseWhereStatement(rhs, whereAST);

            }
            else if (tokens.FindIndex(a => a.Type() == Lexicography.Tokens.Token.IS_EQUAL) != -1)
            {
                var index = tokens.FindIndex(a => a.Type() == Lexicography.Tokens.Token.IS_EQUAL);
                List<Token> children = new List<Token>();

                // Add lhs and rhs of IS_EQUAL token
                children.Add(tokens[index - 1]);
                children.Add(tokens[index + 1]);
                tokens.RemoveRange(index - 1, 3);

                whereAST.Last().AddChild(new AST.Node(Lexicography.Tokens.Token.IS_EQUAL, children));
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