using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveProSoft.Views;
namespace EasySaveProSoft
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleUI ui = new ConsoleUI();
            ui.DisplayMenu();
        }
    }
}
