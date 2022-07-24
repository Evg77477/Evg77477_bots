using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OsEngine.Robots.PriceChanel
{
    public class PriceChanelFix : BotPanel
    {
        public PriceChanelFix(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);

            _tab = TabsSimple[0];

            LenghtUp = CreateParameter("Length Channel Up", 12, 5, 80, 2);
            LenghtDown = CreateParameter("Length Channel Down", 12, 5, 80, 2);

            Mode = CreateParameter("Mode", "Off", new[] { "Off", "OnlyLong", "OnlyShort", "Long_&_Short", "OnlyClosePosition" });

            Lot = CreateParameter("Lot", 10, 5, 20, 1);

            Risk = CreateParameter("Risk", 1m, 0.2m, 3m, 0.1m);

            _pc = IndicatorsFactory.CreateIndicatorByName("PriceChannel", name + "PriceChannel", false);

            _pc.ParametersDigit[0].Value = LenghtUp.ValueInt;

            _pc.ParametersDigit[1].Value = LenghtDown.ValueInt;

            _pc = (Aindicator)_tab.CreateCandleIndicator(_pc, "Prime"); //Добавляем индиктор на чарт.
                                                                       //Если прайм, то индикатор в поле свечей,
                                                                       //если другое - то в отдельном окне

            _pc.Save();

            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;
        }

        

        #region Fields ========================================

        private BotTabSimple _tab;

        private Aindicator _pc;

        private StrategyParameterInt LenghtUp; //Количество периодов для расчета прайс ченнел

        private StrategyParameterInt LenghtDown;

        private StrategyParameterString Mode; 

        private StrategyParameterInt Lot;

        private StrategyParameterDecimal Risk;

        #endregion



        #region Methods =======================================

        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            if (Mode.ValueString == "Off")
            {
                return;
            }

            if (_pc.DataSeries[0].Values == null
                || _pc.DataSeries[1].Values == null
                || _pc.DataSeries[0].Values.Count < LenghtUp.ValueInt + 1
                || _pc.DataSeries[1].Values.Count < LenghtDown.ValueInt + 1)
            {
                return;
            }

            Candle candle = candles[candles.Count - 1]; // Последняя свеча

            decimal lastUp = _pc.DataSeries[0].Values[_pc.DataSeries[0].Values.Count - 2]; // Предпоследнее значение
                                                                                           // индикатора

            decimal lastDown = _pc.DataSeries[1].Values[_pc.DataSeries[1].Values.Count - 2];// Предпоследнее значение

            if (lastUp == 0 || lastDown == 0)
            {
                return;
            }

            List<Position> positions = _tab.PositionsOpenAll;
            List<Position> positionsLong = _tab.PositionOpenLong;
            List<Position> positionsShort = _tab.PositionOpenShort;

            if (Mode.ValueString != "OnlyClosePosition")
            {
                if (Mode.ValueString == "Long_&_Short"
                     || Mode.ValueString == "OnlyLong"
                     || Mode.ValueString == "OnlyShort")
                {
                    decimal riskMoney = _tab.Portfolio.ValueBegin * Risk.ValueDecimal / 100;

                    decimal costPriceStep = _tab.Securiti.PriceStepCost; // цена шага

                    //costPriceStep = 1;

                    decimal steps = Math.Abs((lastUp - lastDown) / _tab.Securiti.PriceStepCost); // количество шагов цены
                                                                                                 // между уровнями
                    decimal lot = riskMoney / steps * costPriceStep;

                    if (Mode.ValueString != "OnlyShort")
                    {
                        if (candle.Close > lastUp
                            && candle.Open < lastUp
                            && positionsLong.Count == 0) // Long
                        {
                            _tab.BuyAtMarket((int)lot);
                        }
                    }
                    if (Mode.ValueString != "OnlyLong")
                    {
                        if (candle.Close < lastDown
                            && candle.Open > lastDown
                            && positionsShort.Count == 0)// Short
                        {
                            _tab.SellAtMarket((int)lot);
                        }
                    }
                }

            }
            if (positions.Count > 0)
            {
                Trailing(positions);
            }           
        }

        private void Trailing(List<Position> positions)
        {
            foreach(Position pos in positions)
            {
                decimal lastDown = _pc.DataSeries[1].Values.Last(); 
                decimal lastUp = _pc.DataSeries[0].Values.Last();

                if (pos.State == PositionStateType.Open)
                {
                    if(pos.Direction == Side.Buy)
                    {
                        _tab.CloseAtTrailingStop(pos, lastDown, lastDown - 100 * _tab.Securiti.PriceStep);
                    }
                    else if(pos.Direction == Side.Sell)
                    {
                        _tab.CloseAtTrailingStop(pos, lastUp, lastUp + 100 * _tab.Securiti.PriceStep);
                    }
                }
            }
        }

        public override string GetNameStrategyType()
        {
            return nameof(PriceChanelFix);
        }

        public override void ShowIndividualSettingsDialog()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
