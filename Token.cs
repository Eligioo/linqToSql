namespace ORM.Lexicography
{
    class Token
    {
        private string characters;
        private Tokens.Token type;

        public Token(Tokens.Token type, string characters)
        {
            this.type = type;
            this.characters = characters;
        }

        public override string ToString()
        {
            return this.type.ToString() + "( " + this.characters + " )";
        }

        public string Characters()
        {
            return this.characters;
        }

        public Tokens.Token Type()
        {
            return this.type;
        }
    }
}

namespace ORM.Lexicography.Tokens
{
    enum Token
    {
        DOT,                    // .
        ARROW,                  // =>
        IS_EQUAL,               // ==
        NOT_EQUAL,              // !=
        GREATER_THAN,           // >
        LESS_THAN,              // <
        LEFT_PARENTHESIS,       // (
        RIGHT_PARENTHESIS,      // )
        EQUAL,                  // =
        NEW_LAMBDA_KEYWORD,     // new
        OBJECT_PROPERTY,        // s.name
        SPACE,                  // " "
        CHAR,                   // a
        WORD,                   // student
        TRUE,                   // True
        FALSE,                  // False
        INTEGER,                // 0
        STRING,                  // "Hello"
        ORDERING,
        AND_ALSO,
        OR_ELSE,
        SELECT,
        WHERE,
        FROM,
        GROUP_BY,
        ORDER_BY,
        UNDEFINED_TOKEN
    }

    enum Ordering
    {
        Ascending,
        Descending
    }

    class TokenCls
    {
        public static List<Token> ComparisonTokenGroup = new List<Token>(){
            Token.IS_EQUAL,
            Token.NOT_EQUAL,
            Token.GREATER_THAN,
            Token.LESS_THAN,
            Token.AND_ALSO,
            Token.OR_ELSE,
        };

        public static bool isTokenPartOfGroup(Token token, List<Token> group)
        {
            return group.Contains(token);
        }
    }
}