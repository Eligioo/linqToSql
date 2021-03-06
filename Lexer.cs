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
                case "!=":
                    token = Tokens.Token.NOT_EQUAL;
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
                case "True":
                    token = Tokens.Token.TRUE;
                    break;
                case "False":
                    token = Tokens.Token.FALSE;
                    break;
                case "new":
                    token = Tokens.Token.NEW_LAMBDA_KEYWORD;
                    break;
                case " ":
                    token = Tokens.Token.SPACE;
                    break;
                case "AndAlso":
                    token = Tokens.Token.AND_ALSO;
                    break;
                case "OrElse":
                    token = Tokens.Token.OR_ELSE;
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

                if (Regex.Match(characters, @"^[A-Za-z]+$").Success)
                {
                    return new Token(Tokens.Token.WORD, characters);
                }

                if (Regex.Match(characters, @"^\d+$").Success)
                {
                    return new Token(Tokens.Token.INTEGER, characters);
                }

                if (Regex.Match(characters, @"(\w*\s\w*)\s+\w{2}\d\s+\d*").Success)
                {
                    return new Token(Tokens.Token.SPACE, characters);
                }

                if (characters.StartsWith('"') && characters.EndsWith('"'))
                {
                    return new Token(Tokens.Token.STRING, characters);
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