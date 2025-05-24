using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EasySaveProSoft.WPF.ViewModels;

namespace EasySaveProSoft.WPF.Views
{
    public partial class AllBackupsProgressWindow : Window
    {
        public AllBackupsProgressWindow(AllBackupsProgressViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}

