using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.FrontRunner.VievModels;
using OsEngine.Robots.FrontRunner.Vievs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.Robots.FrontRunner.Models
{
    public class FrontRunnerBot : BotPanel
    {
        public FrontRunnerBot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);

            _tab = TabsSimple[0];

            _tab.MarketDepthUpdateEvent += _tab_MarketDepthUpdateEvent; //Пришел новый стакан

            _tab.PositionOpeningSuccesEvent += _tab_PositionOpeningSuccesEvent; // Позиция успешно открыта

            _tab.PositionClosingSuccesEvent += _tab_PositionClosingSuccesEvent; // Позиция успешно закрыта          
            
        }

        



        #region Fields ==========================================

        public decimal BigVolume = 20000;

        public int Offset = 1;

        public int Take = 5;

        public decimal Lot = 2;

        public Position Position = null;

        private BotTabSimple _tab;


        #endregion

        #region Properties ==========================================

       
        public decimal OpeningVolume
        {
            get => _openingVolume;
            set
            {
                _openingVolume = value;                
            }
        }
        private decimal _openingVolume = 0;

        public decimal OpeningPrice
        {
            get => _openingPrice;
            set
            {
                _openingPrice = value;
            }
        }
        private decimal _openingPrice = 0;

        public decimal TakePrice
        {
            get => _takePrice;
            set
            {
                _takePrice = value;
            }
        }
        private decimal _takePrice = 0;

        public string TakeStatus
        {
            get => _takeStatuse;
            set
            {
                _takeStatuse = value;
            }
        }
        private string _takeStatuse = "No";

        public decimal VariationMargin
        {
            get => _variationMargin;
            set
            {
                _variationMargin = value;
            }
        }
        private decimal _variationMargin = 0;

        public decimal AccumulatedProfit
        {
            get => _accumulatedProfit;
            set
            {
                _accumulatedProfit = value;
            }
        }
        private decimal _accumulatedProfit = 0;

        public string PositionStatus
        {
            get => _positionStatus;
            set
            {
                _positionStatus = value;
            }
        }
        private string _positionStatus = "Не установлено";

        public string PositionSide
        {
            get => _positionSide;
            set
            {
                _positionSide = value;
            }
        }
        private string _positionSide = "Не установлено";

        public Edit Edit
        {
            get => _edit;

            set
            {
                _edit = value;

                if(_edit == Edit.Stop
                   && Position != null
                   && Position.State == PositionStateType.Opening)
                {
                    _tab.CloseAllOrderInSystem();
                }
            }
        }
        private Edit _edit = VievModels.Edit.Stop;

        #endregion

        #region Methods ========================================== 


        private void _tab_PositionClosingSuccesEvent(Position pos)
        {
            Position = pos;
            if(Position.Direction == Side.Buy)
            {
                AccumulatedProfit += (Position.EntryPrice - Position.ClosePrice)
                            * _tab.Securiti.Lot;
            }
            else if(Position.Direction == Side.Sell)
            {
                AccumulatedProfit += (Position.ClosePrice - Position.EntryPrice)
                            * _tab.Securiti.Lot;
            }
            
            OpeningVolume = 0;
            OpeningPrice = 0;
            TakeStatus = "No";
            TakePrice = 0;
            VariationMargin = 0;
            PositionStatus = "Закрыта";
            PositionSide = "Не установлено";            
            Position = null;

            EventTradeDelegate?.Invoke();
        }

        private void _tab_PositionOpeningSuccesEvent(Position pos)
        {
            Position = pos;
            OpeningVolume = Position.OpenVolume;
            OpeningPrice = Position.EntryPrice;
            PositionStatus = "Открыта";            
            if (Position.Direction == Side.Buy)
            {
                PositionSide = "Buy";
                decimal takePrice = Position.EntryPrice + Take * _tab.Securiti.PriceStep;
                _tab.CloseAtProfit(Position, takePrice, takePrice);
                TakeStatus = "Yes";
                TakePrice = takePrice;
            }
            else if(Position.Direction == Side.Sell)
            {
                PositionSide = "Sell";
                decimal takePrice = Position.EntryPrice - Take * _tab.Securiti.PriceStep;
                _tab.CloseAtProfit(Position, takePrice, takePrice);
                TakeStatus = "Yes";
                TakePrice = takePrice;
            }            
            EventTradeDelegate?.Invoke();
        }

        private void _tab_MarketDepthUpdateEvent(MarketDepth marketDepth)
        {           

            if (Edit == Edit.Stop)
            {
                OpeningVolume = 0;
                OpeningPrice = 0;
                TakeStatus = "No";
                TakePrice = 0;
                VariationMargin = 0;
                PositionStatus = "Не установлено";
                PositionSide = "Не установлено";
                EventTradeDelegate?.Invoke();
                return;
            }
            
            if(marketDepth.SecurityNameCode != _tab.Securiti.Name)
            {
                return;
            }

            if(Position != null 
                && Position.State == PositionStateType.OpeningFail)
            {
                Position = null;
                PositionSide = "Не установлено";
                PositionStatus = "Не установлено";
                OpeningVolume = 0;
                OpeningPrice = 0;
                TakeStatus = "No";
                TakePrice = 0;
                VariationMargin = 0;
                EventTradeDelegate?.Invoke();
            }

            List<Position> positions = _tab.PositionsOpenAll;

            
            if (positions != null
                    && positions.Count > 0)
            {               

                foreach (Position pos in positions)
                {
                    Position = pos;                               

                    if (Position.Direction == Side.Sell
                            && Position.State == PositionStateType.Open)
                    {                   
                       VariationMargin = (marketDepth.Asks[0].Price - Position.EntryPrice) 
                                * Position.OpenVolume;                                                       
                       EventTradeDelegate?.Invoke();
                    }
                    else if (Position.Direction == Side.Buy
                            && Position.State == PositionStateType.Open)
                    {                      
                        VariationMargin = (Position.EntryPrice - marketDepth.Bids[0].Price)
                               * Position.OpenVolume;                                     
                        EventTradeDelegate?.Invoke();
                    }                    
                }
            }
            
           
            for (int i = 0; i < marketDepth.Asks.Count; i++)
            {
                if (marketDepth.Asks[i].Ask >= BigVolume 
                    && Position == null)                    
                {                 
                    decimal price = marketDepth.Asks[i].Price - Offset * _tab.Securiti.PriceStep; 

                    Position = _tab.SellAtLimit(Lot, price);
                    OpeningPrice = price;                                      
                    PositionStatus = "Открывается";
                    PositionSide = "Sell";                    

                    if (Position.State != PositionStateType.Open
                       && Position.State != PositionStateType.Opening)
                    {
                        Position = null;
                    }
                    EventTradeDelegate?.Invoke();
                }
                              
                if(Position != null
                    && marketDepth.Asks[i].Price == Position.EntryPrice 
                    && marketDepth.Asks[i].Ask < BigVolume / 2) 
                {
                    if (Position.State == PositionStateType.Open)
                    {
                        _tab.CloseAtMarket(Position, Position.OpenVolume);
                        PositionStatus = "Закрывается";                        
                    }
                    else if (Position.State == PositionStateType.Opening)
                    {
                        _tab.CloseAllOrderInSystem();
                    }
                    EventTradeDelegate?.Invoke();
                }
                else if (Position != null
                    && Position.State == PositionStateType.Opening
                    && marketDepth.Asks[i].Ask >= BigVolume 
                    && marketDepth.Asks[i].Price < Position.EntryPrice + Offset * _tab.Securiti.PriceStep) 
                {
                    _tab.CloseAllOrderInSystem();
                    Position = null;
                    break;
                }
            }
                       
            for (int i = 0; i < marketDepth.Bids.Count; i++)
            {
                if (marketDepth.Bids[i].Bid >= BigVolume
                    && Position == null)
                {
                    decimal price = marketDepth.Bids[i].Price + Offset * _tab.Securiti.PriceStep; 

                    Position = _tab.BuyAtLimit(Lot, price);
                    PositionStatus = "Открывается";
                    PositionSide = "Buy";
                    OpeningPrice = price;

                    if (Position.State != PositionStateType.Open
                        && Position.State != PositionStateType.Opening)
                    {
                        Position = null;
                    }
                    EventTradeDelegate?.Invoke();
                }               
                
                if (Position != null
                   && marketDepth.Bids[i].Price == Position.EntryPrice
                   && marketDepth.Bids[i].Bid < BigVolume / 2)
                {
                    if(Position.State == PositionStateType.Open)
                    {
                        _tab.CloseAtMarket(Position, Position.OpenVolume);
                        PositionStatus = "Закрывается";                        
                    }
                    else if(Position.State == PositionStateType.Opening)
                    {
                        _tab.CloseAllOrderInSystem(); 
                    }
                    EventTradeDelegate?.Invoke();
                }
                else if(Position != null
                    && Position.State == PositionStateType.Opening
                    && marketDepth.Bids[i].Bid >= BigVolume 
                    && marketDepth.Bids[i].Price > Position.EntryPrice - Offset * _tab.Securiti.PriceStep) 
                {
                    _tab.CloseAllOrderInSystem();
                    Position = null;
                    break;
                }
            }
            EventTradeDelegate?.Invoke();
        }

        public override string GetNameStrategyType()
        {
            return nameof(FrontRunnerBot);
        }

        public override void ShowIndividualSettingsDialog()
        {
            FrontRunnerUI window = new FrontRunnerUI(this);

            window.Show();
        }

        #endregion

        #region Events =================================================

        public delegate void tradeDelegate();
        public event tradeDelegate EventTradeDelegate;

        #endregion
    }
}
