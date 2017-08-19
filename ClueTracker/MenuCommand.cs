using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    class MenuCommand
    {
        public string DisplayText { get; }
        public Action CommandAction { get; }

        public MenuCommand(string displayText, Action commandAction)
        {
            DisplayText = displayText;
            CommandAction = commandAction;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
