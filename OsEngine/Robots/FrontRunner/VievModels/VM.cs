using OsEngine.Entity;
using OsEngine.Robots.FrontRunner.Commands;
using OsEngine.Robots.FrontRunner.Entity;
using OsEngine.Robots.FrontRunner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OsEngine.Robots.FrontRunner.VievModels
{
    public class VM : BaseVM
    {
        public VM(FrontRunnerBot bot)
        {
            _bot = bot;

            _bot.EventTradeDelegate += _bot_EventTradeDelegate;
        }
        

        #region Fields ==========================================

        private FrontRunnerBot _bot;       



        #endregion

        #region Properties ==========================================

        
        public string PositionStatus
        {
            get => _positionStatus;
            set
            {
                _positionStatus = value;                
                OnPropertyChanged(nameof(PositionStatus));               
            }
        }
        private string _positionStatus;

        public string PositionSide
        {
            get => _positionSide;
            set
            {
                _positionSide = value;
                OnPropertyChanged(nameof(PositionSide));
            }
        }
        private string _positionSide;

        public decimal OpeningVolume
        {
            get => _openingVolume;
            set
            {
                _openingVolume = value;
                OnPropertyChanged(nameof(OpeningVolume));
            }
        }
        private decimal _openingVolume;



        public decimal OpeningPrice
        {
            get => _openingPrice;
            set
            {
                _openingPrice = value;
                OnPropertyChanged(nameof(OpeningPrice));
            }
        }
        private decimal _openingPrice;

        public decimal VariationMargin
        {
            get => _variationMargin;
            set
            {
                _variationMargin = value;
                OnPropertyChanged(nameof(VariationMargin));
            }
        }
        private decimal _variationMargin;

        public decimal AccumulatedProfit
        {
            get => _accumulatedProfit;
            set
            {
                _accumulatedProfit = value;
                OnPropertyChanged(nameof(AccumulatedProfit));
            }
        }
        private decimal _accumulatedProfit;

        public string TakeStatus
        {
            get => _takeStatus;
            set
            {
                _takeStatus = value;
                OnPropertyChanged(nameof(TakeStatus));
            }
        }
        private string _takeStatus;

        public string BotStatus
        {
            get => _botStatus;
            set
            {
                _botStatus = value;
                OnPropertyChanged(nameof(BotStatus));
            }
        }
        private string _botStatus = "Робот остановлен";


        public decimal TakePrice
        {
            get => _takePrice;
            set
            {
                _takePrice = value;
                OnPropertyChanged(nameof(TakePrice));
            }
        }
        private decimal _takePrice;


        public decimal BigVolume
        {
            get => _bot.BigVolume;
            set
            {
                _bot.BigVolume = value;
                OnPropertyChanged(nameof(BigVolume));
            }
        }
       
        public int Offset
        {
            get => _bot.Offset;
            set
            {
                _bot.Offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }
        
        public int Take
        {
            get => _bot.Take;
            set
            {
                _bot.Take = value;
                OnPropertyChanged(nameof(Take));
            }
        }
        
        public decimal Lot
        {
            get => _bot.Lot;
            set
            {
                _bot.Lot = value;
                OnPropertyChanged(nameof(Lot));
            }
        }
       

        public Edit Edit
        {
            get => _bot.Edit;
            set
            {
                _bot.Edit = value;
                OnPropertyChanged(nameof(Edit));
            }
        }
       

        #endregion

        #region Commands ==========================================

        private DelegateCommand commandStart;

        public ICommand CommandStart
        {
            get
            {
                if(commandStart == null)
                {
                    commandStart = new DelegateCommand(Start);
                }
                return commandStart;
            }
        }

        #endregion

        #region Methods ==========================================

        private void _bot_EventTradeDelegate()
        {
            PositionStatus = _bot.PositionStatus;
            PositionSide = _bot.PositionSide;
            TakePrice = _bot.TakePrice;
            TakeStatus = _bot.TakeStatus;
            OpeningVolume = _bot.OpeningVolume;
            OpeningPrice = _bot.OpeningPrice;
            VariationMargin = _bot.VariationMargin;
            AccumulatedProfit = _bot.AccumulatedProfit;
        }

        private void Start(object obj)
        {
            if(Edit == Edit.Start)
            {
                Edit = Edit.Stop;
                BotStatus = "Робот остановлен";
            }
            else
            {
                Edit = Edit.Start;
                BotStatus = "Робот запущен";
            }
        }

        #endregion
    }

    public enum Edit
    {
        Start,

        Stop
    } 
    
}
