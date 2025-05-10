using System;
using EasySaveProSoft.Views;
using EasySaveProSoft.Services;

namespace EasySaveProSoft
{
    // Entry point for the EasySaveProSoft application
    class Program
    {

        static void Main(string[] args)
        {
            // Initialize and launch the console user interface
            ConsoleUI ui = new ConsoleUI();
            ui.DisplayMenu(); // Enters the main menu loop
        }
    }
}
