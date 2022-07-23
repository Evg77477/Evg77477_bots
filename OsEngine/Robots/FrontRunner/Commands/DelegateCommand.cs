using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OsEngine.Robots.FrontRunner.Commands
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(DelegateFunction function)
        {
            _function = function;
        }
        public delegate void DelegateFunction(object obj);


        //========================================================
        private DelegateFunction _function;

        //=======================================================

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _function?.Invoke(parameter);
        }
    }
}
