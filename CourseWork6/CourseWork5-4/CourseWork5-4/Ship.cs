using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork5_4
{
    class Ship
    {
        private int countOfDecks;

        public int CountOfDecks { get { return countOfDecks; } }

        public Ship(int countOfDecks)
        {
            this.countOfDecks = countOfDecks;
        }
    }
}
