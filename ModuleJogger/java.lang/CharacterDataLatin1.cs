using System;
using System.Diagnostics;

namespace ModuleJogger.java.lang
{
    class CharacterDataLatin1 : CharacterData
    {
        internal static readonly CharacterDataLatin1 instance = new CharacterDataLatin1();
        private CharacterDataLatin1() { }

        override internal int getProperties(int ch)
        {
            char offset = (char)ch;
            int props = A[offset];
            return props;
        }

        //  static final int A[] = new int[256];
        static readonly int[] A = new int[256];
        // NOTE: \ followed by three digits in Java is an octal literal, and there are no octal literals in C#.
        // So, the Java octal characters are converted to hexadecimal (\201 -> \x81 and \202 -> \x82).
        static readonly String A_DATA =
            "\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800" +
            "\u100F\u4800\u100F\u4800\u100F\u5800\u400F\u5000\u400F\u5800\u400F\u6000\u400F" +
            "\u5000\u400F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800" +
            "\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F" +
            "\u4800\u100F\u4800\u100F\u5000\u400F\u5000\u400F\u5000\u400F\u5800\u400F\u6000" +
            "\u400C\u6800\030\u6800\030\u2800\030\u2800\u601A\u2800\030\u6800\030\u6800" +
            "\030\uE800\025\uE800\026\u6800\030\u2000\031\u3800\030\u2000\024\u3800\030" +
            "\u3800\030\u1800\u3609\u1800\u3609\u1800\u3609\u1800\u3609\u1800\u3609\u1800" +
            "\u3609\u1800\u3609\u1800\u3609\u1800\u3609\u1800\u3609\u3800\030\u6800\030" +
            "\uE800\031\u6800\031\uE800\031\u6800\030\u6800\030\x82\u7FE1\x82\u7FE1\x82" +
            "\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1" +
            "\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82" +
            "\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1\x82\u7FE1" +
            "\x82\u7FE1\uE800\025\u6800\030\uE800\026\u6800\033\u6800\u5017\u6800\033\x81" +
            "\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2" +
            "\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81" +
            "\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2\x81\u7FE2" +
            "\x81\u7FE2\x81\u7FE2\x81\u7FE2\uE800\025\u6800\031\uE800\026\u6800\031\u4800" +
            "\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u5000\u100F" +
            "\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800" +
            "\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F" +
            "\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800" +
            "\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F\u4800\u100F" +
            "\u3800\014\u6800\030\u2800\u601A\u2800\u601A\u2800\u601A\u2800\u601A\u6800" +
            "\034\u6800\034\u6800\033\u6800\034\000\u7002\uE800\035\u6800\031\u4800\u1010" +
            "\u6800\034\u6800\033\u2800\034\u2800\031\u1800\u060B\u1800\u060B\u6800\033" +
            "\u07FD\u7002\u6800\034\u6800\030\u6800\033\u1800\u050B\000\u7002\uE800\036" +
            "\u6800\u080B\u6800\u080B\u6800\u080B\u6800\030\x82\u7001\x82\u7001\x82\u7001" +
            "\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82" +
            "\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001" +
            "\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\u6800\031\x82\u7001\x82" +
            "\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\x82\u7001\u07FD\u7002\x81\u7002" +
            "\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81" +
            "\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002" +
            "\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\u6800" +
            "\031\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002\x81\u7002" +
            "\u061D\u7002";

        // In all, the character property tables require 1024 bytes.
        static CharacterDataLatin1()
        {
            char[] data = A_DATA.ToCharArray();
            //assert(data.length == (256 * 2));
            Debug.Assert(data.Length == (256 * 2));
            int i = 0, j = 0;
            while (i < (256 * 2))
            {
                int entry = data[i++] << 16;
                A[j++] = entry | data[i++];
            }
        }

    }
}