using OsEngine.OsTrader.Panels.Tab;
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

namespace OsEngine.Robots.MyTestRobot
{
    /// <summary>
    /// Логика взаимодействия для WindowMyTestBot.xaml
    /// </summary>
    public partial class WindowMyTestBot : Window
    {
        public WindowMyTestBot(MyTestBot robot) 
        {
            InitializeComponent();
            vm = new VM(robot);

            DataContext = vm;
        }

        private VM vm;

       
    }
}
