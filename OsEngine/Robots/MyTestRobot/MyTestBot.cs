using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OsEngine.Robots.MyTestRobot
{
    public class MyTestBot : BotPanel
    {
        public MyTestBot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            this.TabCreate(BotTabType.Simple);

            _tab = TabsSimple[0];

            ParamMode = CreateParameter("Mode", "Edit", new[] { "Edit", "Trade" });

            _risk = CreateParameter("Risk %", 1m, 0.1m, 10m, 0.1m);

            _profitKoef = CreateParameter("Koef Profit", 1m, 0.1m, 10m, 0.1m);

            _countDownCandles = CreateParameter("CountvDownvCandles", 1, 1, 5, 1);

            _koefVolume = CreateParameter("Koef Volume", 2m, 2m, 10m, 0.5m);

            _countCandles = CreateParameter("Count Candles", 10, 5, 50, 1);

            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;

            _tab.PositionOpeningSuccesEvent += _tab_PositionOpeningSuccesEvent;

            _tab.PositionClosingSuccesEvent += _tab_PositionClosingSuccesEvent;

            

        }

      


        #region Fields =======================================

        private BotTabSimple _tab;

        private StrategyParameterDecimal _risk; //Риск на сделку в процентах

        private StrategyParameterDecimal _profitKoef;  //Во сколько раз тейкпрофит больше, чем риск

        private StrategyParameterInt _countDownCandles; // Кол-во падающих свечей перед объемным разворотом

        private StrategyParameterDecimal _koefVolume; //Во сколько раз объем больше, чем средний объем

        private decimal _averageVolume; // Средний объем

        private StrategyParameterInt _countCandles; //Кол-во свечей для вычисления среднего объема

        public StrategyParameterString ParamMode { get; set; }

        private int _punkts = 0; //Кол-во пунктов до стоп лосса

        private decimal _lowCandle = 0; //Минимум свечи для выставления стопа



        #endregion

        #region Methods ====================================================

        public override string GetNameStrategyType()
        {
            return nameof(MyTestBot);
        }

        public override void ShowIndividualSettingsDialog()
        {
            WindowMyTestBot window = new WindowMyTestBot(this);

            window.ShowDialog();
        }

        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            if (candles.Count < _countDownCandles.ValueInt + 1
                || candles.Count < _countCandles.ValueInt + 1)
            {
                return;
            }

            _averageVolume = 0;

            for (int i = candles.Count - 2; i > candles.Count - _countCandles.ValueInt - 2; i--)
            {
                _averageVolume += candles[i].Volume;
            }

            _averageVolume /= _countCandles.ValueInt;

            List<Position> positions = _tab.PositionOpenLong;

            Candle candle = candles[candles.Count - 1];

            if (positions.Count > 0) // Есть открытая позиция, проверяем наличие возможности переноса стоп-лосса
                                     // и переносим стоп-лосс в безубыток
            {
                Position pos = positions[0];

                if (candle.Close > pos.EntryPrice && candle.Close - pos.EntryPrice >= pos.EntryPrice - pos.StopOrderPrice)
                {
                    pos.StopOrderIsActiv = false;

                    _tab.CloseAtStop(pos, pos.EntryPrice, pos.EntryPrice - 100 * _tab.Securiti.PriceStep);
                }

                return;
            }

            if (candle.Close < (candle.High + candle.Low) / 2
                || candle.Volume < _averageVolume * _koefVolume.ValueDecimal)
            {
                return;
            }

            for (int i = candles.Count - 2; i > candles.Count - 2 - _countDownCandles.ValueInt; i--)
            {
                if (candles[i].Close > candles[i].Open)
                {                        
                    return;
                }
            }

            _punkts = (int)((candle.Close - candle.Low) / _tab.Securiti.PriceStep);

            if (_punkts < 5)
            {
                return;
            }

            decimal ammountStop = _punkts * _tab.Securiti.PriceStepCost; //Риск на один стоп

            decimal ammountRisk = _tab.Portfolio.ValueBegin * _risk.ValueDecimal / 100; //Риск в деньгах, которым м ыготовы рисковать

            decimal volume = ammountRisk / ammountStop; // Кол-во лотов

            decimal go = 10000;

            if (_tab.Securiti.Go > 1)
            {
                go = _tab.Securiti.Go;
            }

            decimal maxLot = _tab.Portfolio.ValueBegin / go;

            if (volume < maxLot)
            {
                _lowCandle = candle.Low;

                _tab.BuyAtMarket(volume);
            }

        }


        private void _tab_PositionOpeningSuccesEvent(Position pos) //Открытие позиции
        {
            decimal priceTake = pos.EntryPrice + _punkts * _profitKoef.ValueDecimal;

            _tab.CloseAtProfit(pos, priceTake, priceTake);

            _tab.CloseAtStop(pos, _lowCandle, _lowCandle - 100 * _tab.Securiti.PriceStep);

        }

        
        private void _tab_PositionClosingSuccesEvent(Position pos) //Закрытие позиции
        {
            SaveCSV(pos);
        }

        

        private void SaveCSV(Position pos)
        {

            if (!File.Exists(@"Engine\trades.csv"))
            {
                string header = ";Позиция;Символ;Лоты;Изменение/Максимум Лотов;Исполнение входа;" +
                "Сигнал входа;Бар входа;Дата входа;Время входа;Цена входа;Комиссия входа;" +
                "Исполнение выхода;Сигнал выхода;Бар выхода;Дата выхода;Время выхода;Цена выхода;" +
                "Комиссия выхода;Средневзвешенная цена входа;П/У;П/У сделки;П/У с одного лота;" +
                "Зафиксированная П/У;Открытая П/У;Продолж. (баров);Доход/Бар;Общий П/У;" +
                "% изменения;MAE;MAE %;MFE;MFE %";

                using (StreamWriter writer = new StreamWriter(@"Engine\trades.csv", false))
                {
                    writer.WriteLine(header);
                    writer.Close();
                }
            }
            using (StreamWriter writer = new StreamWriter(@"Engine\trades.csv", true))
            {
                string str = ";;;;;;;;" + pos.TimeOpen.ToShortDateString();

                str += ";" + pos.TimeOpen.TimeOfDay;
                str += ";;;;;;;;;;;;;;" + pos.ProfitPortfolioPunkt + ";;;;;;;;;";

                writer.WriteLine(str);

                writer.Close();
            }

        }

        #endregion

    }

}

