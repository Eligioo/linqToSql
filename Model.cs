using ORM.Lexicography;
using System.Linq.Expressions;
using Pluralize.NET;
using MySql.Data.MySqlClient;

namespace ORM
{
    abstract class Model<T>
    {
        public PendingQuery<T> Select<Selector>(Expression<Func<T, Selector>> s)
        {
            IPluralize pluralizer = new Pluralizer();
            var objectName = typeof(T).ToString().Split('.').Last().ToLower();
            return new PendingQuery<T>(s.ToString(), pluralizer.Pluralize(objectName));
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
            var tokens = Lexer.Tokenize(predicate.ToString());
            Console.WriteLine(predicate.ToString());
            foreach (var token in tokens)
            {
                Console.WriteLine(token.ToString());
            }
            Environment.Exit(0);
            return this;
        }

        public List<object> Execute(String server, String user, String password, String database)
        {
            var g = new Generator.Generator(this.ast).Generate();
            Console.WriteLine(g);

            string cs = @"server=" + server + ";userid=" + user + ";password=" + password + ";database=" + database;
            using var con = new MySqlConnection(cs);
            con.Open();
            var reader = new MySqlCommand(g, con).ExecuteReader();

            int intFieldCount = reader.FieldCount;
            List<object> list = new List<object>();

            while (reader.Read())
            {
                object[] objValues = new object[intFieldCount];
                reader.GetValues(objValues);
                var a = reader.GetFieldValue<int>(0);
                list.Add(objValues);
                // Console.WriteLine(reader.GetValues(objs));
                Console.WriteLine(reader.GetString(1));
            }
            con.Close();

            return list;
        }
    }
}