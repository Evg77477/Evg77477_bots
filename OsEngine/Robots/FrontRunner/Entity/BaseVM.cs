using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.FrontRunner.Entity
{
    public class BaseVM : INotifyPropertyChanged
    {

        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        //========================== Events ===================================

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
