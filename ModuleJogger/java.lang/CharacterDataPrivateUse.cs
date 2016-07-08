using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleJogger.java.lang
{
    class CharacterDataPrivateUse : CharacterData
    {
        internal static readonly CharacterDataPrivateUse instance = new CharacterDataPrivateUse();
        private CharacterDataPrivateUse() { }

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
