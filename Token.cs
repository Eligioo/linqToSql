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
        SELECT,
        WHERE,
        FROM,
        GROUP_BY,
        ORDER_BY,
        UNDEFINED_TOKEN
    }
}