using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermoLib
{
    public class Letter
    {
        public char Character;
        public TypedStatus Status;

        public Letter(char character, TypedStatus status)
        {
            Character = character;
            Status = status;
        }
    }
}
