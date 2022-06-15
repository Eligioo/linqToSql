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
            var tokens = Lexer.Tokenize(predicate.ToString());
            Console.WriteLine(predicate.ToString());
            foreach (var token in tokens)
            {
                Console.WriteLine(token.ToString());
            }
            Environment.Exit(0);
            return this;
        }

        public List<T> Execute(String server, String user, String password, String database)
        {
            var g = new Generator.Generator(this.ast).Generate();
            Console.WriteLine(g);

            string cs = @"server=" + server + ";userid=" + user + ";password=" + password + ";database=" + database;
            var con = new MySqlConnection(cs);
            con.Open();

            var reader = new MySqlCommand(g, con).ExecuteReader();

            int intFieldCount = reader.FieldCount;
            List<T> list = new List<T>();

            while (reader.Read())
            {
                object[] values = new object[intFieldCount];
                reader.GetValues(values);
                var instance = (T)Activator.CreateInstance(typeof(T), values);
                list.Add(instance);
            }

            con.Close();

            return list;
        }
    }
}