using OsEngine.Robots.FrontRunner.Models;
using OsEngine.Robots.FrontRunner.VievModels;
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

namespace OsEngine.Robots.FrontRunner.Vievs
{
    /// <summary>
    /// Логика взаимодействия для FrontRunnerUI.xaml
    /// </summary>
    public partial class FrontRunnerUI : Window
    {
        public FrontRunnerUI(FrontRunnerBot bot)
        {
            InitializeComponent();
            vm = new VM(bot);

            DataContext = vm;
        }
        private VM vm;
    
    }
}
