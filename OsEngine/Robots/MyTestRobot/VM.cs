using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OsEngine.Robots.MyTestRobot
{
    public class VM : Base_VM
    {
        public VM(MyTestBot robot)
        {
            _robot = robot;
        }
        private MyTestBot _robot;

       
        public string ParamMode
        {
            get => _robot.ParamMode.ValueString;
            set
            {
                _robot.ParamMode.ValueString = value;
                OnPropertyChanged(nameof(ParamMode));
            }
        }




        //public int ParamLot
        //{
        //    get => _robot.ParamLot.ValueInt;
        //    set
        //    {
        //        _robot.ParamLot.ValueInt = value;
        //        OnPropertyChanged(nameof(ParamLot));
        //    }
        //}


        //public int ParamStop
        //{
        //    get => _robot.ParamStop.ValueInt;
        //    set
        //    {
        //        _robot.ParamStop.ValueInt = value;
        //        OnPropertyChanged(nameof(ParamStop));
        //    }
        //}


        //public int ParamTake
        //{
        //    get => _robot.ParamTake.ValueInt;

        //    set
        //    {
        //        _robot.ParamTake.ValueInt = value;
        //        OnPropertyChanged(nameof(ParamTake));
        //    }
        //}



    }
}
