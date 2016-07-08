namespace ModuleJogger.java.lang
{
    class CharacterDataUndefined : CharacterData
    {
        internal static readonly CharacterDataUndefined instance = new CharacterDataUndefined();
        private CharacterDataUndefined() { }

        override internal int getProperties(int ch)
        {
            return 0;
        }

        public new bool isJavaIdentifierStart(int ch)
        {
            return false;
        }

        public new bool isJavaIdentifierPart(int ch)
        {
            return false;
        }
    }
}
