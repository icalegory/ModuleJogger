namespace ModuleJogger.java.lang
{
    /// <summary>
    /// This is a conversion from the JDK source (OpenJDK) of Java's CharacterData class.
    /// </summary>
    abstract class CharacterData
    {
        abstract internal int getProperties(int ch);

        // Character <= 0xff (basic latin) is handled by internal fast-path
        // to avoid initializing large tables.
        // Note: performance of this "fast-path" code may be sub-optimal
        // in negative cases for some accessors due to complicated ranges.
        // Should revisit after optimization of table initialization.

        //static final CharacterData of(int ch)
        static public CharacterData of(int ch)
        {
            //if (ch >>> 8 == 0)
            if ((int)((uint)ch >> 8) == 0)
            {     // fast-path
                return CharacterDataLatin1.instance;
            }
            else
            {
                //switch (ch >>> 16)
                switch ((int)((uint)ch >> 16))
                {  //plane 00-16
                    case (0):
                        return CharacterData00.instance;
                    case (1):
                        return CharacterData01.instance;
                    case (2):
                        return CharacterData02.instance;
                    case (14):
                        return CharacterData0E.instance;
                    case (15):   // Private Use
                    case (16):   // Private Use
                        return CharacterDataPrivateUse.instance;  //return false for isJavaIdentifierStart(int) and is JavaIdentifierPart(int)
                    default:
                        return CharacterDataUndefined.instance;   //return false for isJavaIdentifierStart(int) and is JavaIdentifierPart(int)
                }
            }
        }

        public bool isJavaIdentifierStart(int ch)
        {
            int props = getProperties(ch);
            return ((props & 0x00007000) >= 0x00005000);
        }

        public bool isJavaIdentifierPart(int ch)
        {
            int props = getProperties(ch);
            return ((props & 0x00003000) != 0);
        }
    }
}
