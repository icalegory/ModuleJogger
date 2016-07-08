﻿using System;
using System.Text.RegularExpressions;

namespace ModuleJogger
{
    public static class Character
    {
        // For reference see the following pages:
        //http://stackoverflow.com/questions/1904252/is-there-a-method-in-c-sharp-to-check-if-a-string-is-a-valid-identifier
        //http://stackoverflow.com/questions/24831853/antlr4-csharp-target-grammar-java-g4-generated-javalexer-cs-does-not-comp

        public static bool isJavaIdentifierStart(int i)
        {
            char c = Convert.ToChar(i);
            string s = new string(c, 1);
            const string start = @"(\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl})";
            const string extend = @"(\p{Mn}|\p{Mc}|\p{Nd}|\p{Pc}|\p{Cf})";
            Regex ident = new Regex(string.Format("{0}({0}|{1})*", start, extend));
            s = s.Normalize();
            return ident.IsMatch(s);
        }

        public static int LA(this Antlr4.Runtime.ICharStream cs, int la)
        {
            return cs.La(la);
        }

        public static int toCodePoint(char c)
        {
            // We only have a char input, so we can use this:
            return (int) c;

            // From http://stackoverflow.com/questions/13894021/return-code-point-of-characters-in-c-sharp
            // If we had a string input:
            // To address the comments, A char in C# is a 16 bit number, and holds a UTF16 code point. Code points above 16 the bit
            // space cannot be represented in a C# character. Characters in C# is not variable width. A string however can have 2 chars
            // following each other, each being a code unit, forming a UTF16 code point. If you have a string input and characters above
            // the 16 bit space, you can use char.IsSurrogatePair and Char.ConvertToUtf32, as suggested in another answer:
            //string input = ....
            //for (int i = 0; i < input.Length; i += Char.IsSurrogatePair(input, i) ? 2 : 1)
            //{
            //    int x = Char.ConvertToUtf32(input, i);
            //    Console.WriteLine("U+{0:X4}", x);
            //}
        }


    }
}
