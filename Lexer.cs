using System.Text.RegularExpressions;

namespace ORM.Lexicography
{
    class Lexer
    {
        private static Token MapToToken(string characters)
        {
            var token = Tokens.Token.UNDEFINED_TOKEN;
            switch (characters)
            {
                case ".":
                    token = Tokens.Token.DOT;
                    break;
                case "=>":
                    token = Tokens.Token.ARROW;
                    break;
                case "==":
                    token = Tokens.Token.IS_EQUAL;
                    break;
                case "(":
                    token = Tokens.Token.LEFT_PARENTHESIS;
                    break;
                case ")":
                    token = Tokens.Token.RIGHT_PARENTHESIS;
                    break;
                case "=":
                    token = Tokens.Token.EQUAL;
                    break;
                case "new":
                    token = Tokens.Token.NEW_LAMBDA_KEYWORD;
                    break;
                case " ":
                    token = Tokens.Token.SPACE;
                    break;
                default:
                    token = Tokens.Token.UNDEFINED_TOKEN;
                    break;
            }

            if (token == Tokens.Token.UNDEFINED_TOKEN)
            {
                if (Regex.Match(characters, @"[a-zA-Z]\.[a-zA-Z]").Success)
                {
                    return new Token(Tokens.Token.OBJECT_PROPERTY, characters.Replace(",", ""));
                }

            }

            return new Token(token, characters);
        }

        public static List<Token> Tokenize(String expr)
        {
            var splitted = expr
                                .Replace("(", " ( ")
                                .Replace(")", " ) ")
                                .Split(' ');
            var tokens = new List<Token>();
            foreach (var item in splitted)
            {
                tokens.Add(Lexer.MapToToken(item));
            }
            return tokens;
        }
    }
}