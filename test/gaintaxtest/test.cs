using gaintaxlibrary;

namespace gaintaxtest{
    public class UnitTest1
    {
        private static bool isClose(double a, double b)
        {
            if (a < 0.0000001 && a > -0.0000001 && b < 0.0000001 && b > -0.0000001) // zero case
            {
                return true;
            }

            if (a < b * 1.0000001 && a > b * 0.999999)
            {
                return true;
            }
            if (a > b * 1.0000001 && a < b * 0.999999) // for negative numbers comparision
            {
                return true;
            }

            return false;
        }

        private void addUsdSellTransaction(List<transaction> t, string symbol, DateTime d, double tokenSellAmount, double dollars, string exchange="exchange")
        {
            t.Add(new transaction
                {
                    buyAmount = dollars,
                    buySymbol = "usd",
                    sellAmount = tokenSellAmount,
                    sellSymbol = symbol,
                    dateTime = d,
                    exchangeRec = exchange,
                    exchangeSent = exchange,
                    combinedCount = 0
                });
        }

        private void addUsdBuyTransaction(List<transaction> t, string symbol, DateTime d, double tokenBuyAmount, double dollars, string exchange="exchange")
        {
            t.Add(new transaction
                {
                    buyAmount = tokenBuyAmount,
                    buySymbol = symbol,
                    sellAmount = dollars,
                    sellSymbol = "usd",
                    dateTime = d,
                    exchangeRec = exchange,
                    exchangeSent = exchange,
                    combinedCount = 1
                });
        }

        [Fact]
        public void HistoricPriceSimple()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();
            hp.dateOnly = new DateOnly(2018, 1, 3);
            hp.low = double.Parse("2000.0");
            hp.high = double.Parse("3000.0");
            hp.firstPrice = double.Parse("2000.0");
            hp.lastPrice = double.Parse("3000.0");
            t.historicPrices.Add(hp);

            historicDayPriceUSD hp2 = new historicDayPriceUSD();
            hp2.dateOnly = new DateOnly(2018, 1, 4);
            hp2.low = double.Parse("3000.0");
            hp2.high = double.Parse("4000.0");
            hp2.firstPrice = double.Parse("3000.0");
            hp2.lastPrice = double.Parse("4000.0");
            t.historicPrices.Add(hp2);

            // +$500 eth cost $500 for 5 eth
            // -$350 for 0.1BTC sell 2 eth at $175 for $150 gain
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2018, 1, 1), 1, 2000);
            //$100 gain btc
                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 5,
                    buySymbol = "eth",
                    sellAmount = 0.2,
                    sellSymbol = "btc",
                    dateTime = new DateTime(2018, 1, 3),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 0
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 0.1,
                    buySymbol = "btc",
                    sellAmount = 2,
                    sellSymbol = "eth",
                    dateTime = new DateTime(2018, 1, 4),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 0
                });

            t.convertBitcoinPairToTwoTransInUSD(t.transactionsOriginal);

            List<bucket> buckets = new List<bucket>();
            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);
            var bucketsString = t.summerizeBucketsToStringList(buckets);

            List<bucket> bucketsEth = new List<bucket>();
            var realizedeth = t.computeGains(out bucketsEth, false, "eth", "fiho", t.transactionsOriginal);
            var bucketsEthString = t.summerizeBucketsToStringList(bucketsEth);

            Assert.NotEmpty(bucketsString.Find(x => x.Exists(x=> x == "Bucket Price: 2000")));
            Assert.NotEmpty(bucketsString.Find(x => x.Exists(x=> x == "Amt: 0.8")));
            Assert.NotEmpty(bucketsString.Find(x => x.Exists(x=> x == "Bucket Price: 3500")));
            Assert.NotEmpty(bucketsString.Find(x => x.Exists(x=> x == "Amt: 0.1")));
            Assert.True(isClose(realizedbtc.First(x => x.trans.dateTime.Value.Year == 2018).gain, 100.0));
            
            Assert.NotEmpty(bucketsEthString.Find(x => x.Exists(x=> x == "Bucket Price: 100")));
            
            Assert.True(isClose(realizedeth.First(x => x.trans.dateTime.Value.Year == 2018).gain, 150.0));
        }

        [Fact]
        public void AddTransactions()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();

            if (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2018,1,i+1), 1,  (1000 + i * 5) * 1);
                }

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 1030 * 3,
                    buySymbol = "usd",
                    sellAmount = 3,
                    sellSymbol = "btc",
                    dateTime = new DateTime(2018, 11, 10),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 0
                });

            }

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 2);
            List<bucket> bucketsbtc = new List<bucket>();
            var realizedbtc = t.computeGains(out bucketsbtc, true, "btc", "fiho", t.transactionsOriginal);
            //t.computeBuysSellsSymbol(t.transactionsOriginal, "btc");

            Assert.Single(realizedbtc);
            var h = realizedbtc.First();
            Assert.True(isClose(h.gain, 75.0));
        }

        [Fact]
        public void Testfiho()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();
            historicDayPriceUSD hp = new historicDayPriceUSD();
            for (int i = 0; i < 3; i++)
            {
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2018, 1, i + 1), 1, (2000 + i * 100));
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2019, 1 , i + 1), 1, (100 + i * 100));
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2020, 1 , i + 1), 1, (1000 + i * 100));
                addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(2021, 1, i + 1), 1, 2000);
            }
            t.transactionsOriginal= t.transactionsOriginal.OrderBy(x => x.dateTime.Value).ToList();

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 100); // 100 hours 
            List<bucket> bucketsbtc = new List<bucket>();
            var realizedbtc = t.computeGains(out bucketsbtc, true, "btc", "fiho", t.transactionsOriginal);

            Assert.Single(realizedbtc);
            var h = realizedbtc.First();
            Assert.True(isClose(h.gain, -300.0));
            Assert.True(isClose(h.sellAmountReceivedUsuallyDollars, 6000.0));
        }

        [Fact]
        public void TestfihoDoNotCombine()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();
            historicDayPriceUSD hp = new historicDayPriceUSD();
            for (int i = 0; i < 3; i++)
            {
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2018, 1, i + 1), 1, (2000 + i * 100));
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2019, 1 , i + 1), 1, (100 + i * 100));
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2020, 1 , i + 1), 1, (1000 + i * 100));
                addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(2021, 1, i + 1), 1, 2000);
            }
            t.transactionsOriginal= t.transactionsOriginal.OrderBy(x => x.dateTime.Value).ToList();

            List<bucket> bucketsbtc = new List<bucket>();
            var realizedbtc = t.computeGains(out bucketsbtc, true, "btc", "fiho", t.transactionsOriginal);

            Assert.Equal(3, realizedbtc.Count());
            
            Assert.True(isClose(realizedbtc.First(x => isClose(x.gain, -200.0)).gain, -200.0));
            Assert.True(isClose(realizedbtc.First(x => isClose(x.gain, -100.0)).gain, -100.0));
            Assert.True(isClose(realizedbtc.First(x => isClose(x.gain, 0.0)).gain, 0.0));
        }

        [Fact]
        public void Testfilo()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();
            historicDayPriceUSD hp = new historicDayPriceUSD();
            for (int i = 0; i < 3; i++)
            {
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2018, 1, i + 1), 1, (2000 + i * 100));
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2019, 1 , i + 1), 1, (100 + i * 100));
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2020, 1 , i + 1), 1, (1000 + i * 100));
                addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(2021, 1, i + 1), 1, 2000);
            }
            t.transactionsOriginal= t.transactionsOriginal.OrderBy(x => x.dateTime.Value).ToList();

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 100); // 100 hours 
            List<bucket> bucketsbtc = new List<bucket>();
            var realizedbtc = t.computeGains(out bucketsbtc, true, "btc", "filo", t.transactionsOriginal);

            Assert.Single(realizedbtc);
            var h = realizedbtc.First();
            Assert.True(isClose(h.gain, 2700.0));
        }

        [Fact]
        public void Testfiso()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();
            historicDayPriceUSD hp = new historicDayPriceUSD();
            for (int i = 0; i < 3; i++)
            {
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2018, 1, i + 1), 1, (2000 + i * 100));
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2019, 1 , i + 1), 1, (100 + i * 100));
                addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2020, 1 , i + 1), 1, (1000 + i * 100));
                addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(2021, 1, i + 1), 1, 2000);
            }
            t.transactionsOriginal= t.transactionsOriginal.OrderBy(x => x.dateTime.Value).ToList();

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 100); // 100 hours 
            List<bucket> bucketsbtc = new List<bucket>();
            var realizedbtc = t.computeGains(out bucketsbtc, true, "btc", "fiso", t.transactionsOriginal);

            Assert.Single(realizedbtc);
            var h = realizedbtc.First();
            Assert.True(isClose(h.gain, 5400.0));
        }

        [Fact]
        public void AddManyTransactionsTwoSellsOnSameDay()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();

            if (true)
            {
                for (int i = 0; i < 30; i++)
                {
                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = 1,
                        buySymbol = "btc",
                        sellAmount = (1000 + i) * 1,
                        sellSymbol = "usd",
                        dateTime = new DateTime(2018, 1, i + 1),
                        exchangeRec = "binance",
                        exchangeSent = "binance",
                        combinedCount = 0
                    });
                }

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 1030 * 29,
                    buySymbol = "usd",
                    sellAmount = 29,
                    sellSymbol = "btc",
                    dateTime = new DateTime(2018, 11, 10),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 0
                });
                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 1030 * 1,
                    buySymbol = "usd",
                    sellAmount = 1,
                    sellSymbol = "btc",
                    dateTime = new DateTime(2018, 11, 10),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 0
                });

            }

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 2);
            List<bucket> bucketsbtc = new List<bucket>();
            var realizedbtc = t.computeGains(out bucketsbtc, true, "btc", "fiho", t.transactionsOriginal);
            //t.computeBuysSellsSymbol(t.transactionsOriginal, "btc");

            /*t.summerizeRealized("2018", realizedbtc);
            var h = realizedbtc.First(x => x.trans.transDate.Value.Year == 2018).gain;
            Assert.True(isClose(h, 465.0));
*/
            Assert.Single(realizedbtc);
            var h = realizedbtc.First();
            Assert.True(isClose(h.gain, 465.0));
        }

        [Fact]
        public void ManyTransactionsTestCombine1()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();

            if (true)
            {
                for (int i = 0; i < 12; i += 2)
                {
                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = 1,
                        buySymbol = "btc",
                        sellAmount = (1000 + i) * 1,
                        sellSymbol = "usd",
                        dateTime = new DateTime(2018, 1, i + 1),
                        exchangeRec = "binance",
                        exchangeSent = "binance",
                        combinedCount = 0
                    });

                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = (1030 + i) * 0.5,
                        buySymbol = "usd",
                        sellAmount = 0.5,
                        sellSymbol = "btc",
                        dateTime = new DateTime(2018, 1, i + 2),
                        exchangeRec = "coinbase",
                        exchangeSent = "coinbase",
                        combinedCount = 0
                    });
                }

            }
            t.transactionsOriginal.Add(new transaction
            {
                buyAmount = 1020 * 3,
                buySymbol = "usd",
                sellAmount = 3,
                sellSymbol = "btc",
                dateTime = new DateTime(2018, 9, 3),
                exchangeRec = "coinbase",
                exchangeSent = "coinbase",
                combinedCount = 0
            });
            List<bucket> buckets = new List<bucket>();

            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 24 * 90);

            var realizedbtc2 = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);

            var h = realizedbtc.Sum(x => x.gain);
            var h2 = realizedbtc2.Sum(x => x.gain);
            Assert.True(isClose(h, h2));

        }

        [Fact]
        public void ManyTransactionsTestCombine2()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();

            if (true)
            {
                for (int i = 0; i < 12; i += 2)
                {
                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = 1,
                        buySymbol = "btc",
                        sellAmount = (1000 + i) * 1,
                        sellSymbol = "usd",
                        dateTime = new DateTime(2018, 1, i + 1),
                        exchangeRec = "binance",
                        exchangeSent = "binance",
                        combinedCount = 0
                    });


                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = (1030 + i) * 0.5,
                        buySymbol = "usd",
                        sellAmount = 0.5,
                        sellSymbol = "btc",
                        dateTime = new DateTime(2018, 1, i + 2),
                        exchangeRec = "coinbase",
                        exchangeSent = "coinbase",
                        combinedCount = 0
                    });
                }
                for (int i = 0; i < 12; i += 2)
                {
                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = 0.1,
                        buySymbol = "btc",
                        sellAmount = (2000 + i) * 0.1,
                        sellSymbol = "usd",
                        dateTime = new DateTime(2018, 3, i + 1),
                        exchangeRec = "binance",
                        exchangeSent = "binance",
                        combinedCount = 0
                    });


                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = (2030 + i) * 0.2,
                        buySymbol = "usd",
                        sellAmount = 0.2,
                        sellSymbol = "btc",
                        dateTime = new DateTime(2018, 3, i + 2),
                        exchangeRec = "coinbase",
                        exchangeSent = "coinbase",
                        combinedCount = 0
                    });
                }

            }
            t.transactionsOriginal.Add(new transaction
            {
                buyAmount = 3020 * 2.4,
                buySymbol = "usd",
                sellAmount = 2.4,
                sellSymbol = "btc",
                dateTime = new DateTime(2018, 9, 3),
                exchangeRec = "coinbase",
                exchangeSent = "coinbase",
                combinedCount = 0
            });
            List<bucket> buckets = new List<bucket>();

            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 24 * 31);

            var realizedbtc2 = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);

            var h = realizedbtc.Sum(x => x.gain);
            var h2 = realizedbtc2.Sum(x => x.gain);
            Assert.True(isClose(h, h2));

        }

        [Fact]
        public void HistoricPrice1()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();
            hp.dateOnly = new DateOnly(2018, 1, 3);
            hp.high = double.Parse("3000.0");
            hp.low = double.Parse("2000.0");

            t.historicPrices.Add(hp);

            if (true)
            {
                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 1.1,
                    buySymbol = "btc",
                    sellAmount = 1000 * 1.1,
                    sellSymbol = "usd",
                    dateTime = new DateTime(2018, 1, 2),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 0
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 3000 * 0.9,
                    buySymbol = "usd",
                    sellAmount = 0.9,
                    sellSymbol = "btc",
                    dateTime = new DateTime(2019, 1, 10),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 0
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 2,
                    buySymbol = "eth",
                    sellAmount = 0.1,
                    sellSymbol = "btc",
                    dateTime = new DateTime(2018, 1, 3),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 0
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 200.0 * 1.0,
                    buySymbol = "usd",
                    sellAmount = 1.0,
                    sellSymbol = "eth",
                    dateTime = new DateTime(2018, 1, 5),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 0
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 300 * 1.0,
                    buySymbol = "usd",
                    sellAmount = 1.0,
                    sellSymbol = "eth",
                    dateTime = new DateTime(2019, 1, 6),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 0
                });
            }

            List<bucket> buckets = new List<bucket>();
            t.convertBitcoinPairToTwoTransInUSD(t.transactionsOriginal, 0.0);
            //t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 2);
            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);
            //t.computeBuysSellsSymbol(t.transactionsOriginal, "btc");
            var realizedeth = t.computeGains(out buckets, false, "eth", "fiho", t.transactionsOriginal);
            //t.computeBuysSellsSymbol(t.transactionsOriginal, "eth");

            //t.summerizeRealized("2018", realizedbtc);
            Assert.True(isClose(realizedbtc.First(x => x.trans.dateTime.Value.Year == 2018).gain, 100.0));
            //t.summerizeRealized("2019", realizedbtc);
            Assert.True(isClose(realizedbtc.First(x => x.trans.dateTime.Value.Year == 2019).gain, 1800.0));
            //t.summerizeRealized("2018", realizedeth);
            Assert.True(isClose(realizedeth.First(x => x.trans.dateTime.Value.Year == 2018).gain, 50.0));
            //t.summerizeRealized("2019", realizedeth);
            Assert.True(isClose(realizedeth.First(x => x.trans.dateTime.Value.Year == 2019).gain, 150.0));
        }

        [Fact]
        public void TestStingPad()
        {
            ClassGainTax t = new ClassGainTax();
            double s = 1;
            s = 1.0;
            Assert.Equal("     1.00", t.padStringFormat(s));
            s = 10.0;
            Assert.Equal("    10.00", t.padStringFormat(s));
            s = 100.0;
            Assert.Equal("   100.00", t.padStringFormat(s));
            s = 1000.0;
            Assert.Equal("  1000.00", t.padStringFormat(s));

            s = -1.0;
            Assert.Equal("    -1.00", t.padStringFormat(s));
            s = -10.0;
            Assert.Equal("   -10.00", t.padStringFormat(s));
            s = -100.0;
            Assert.Equal("  -100.00", t.padStringFormat(s));
            s = -1000.0;
            Assert.Equal(" -1000.00", t.padStringFormat(s));

        }

        [Fact]
        public void ManyTransactionsTestCombine3()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();

            if (true)
            {
                for (int i = 0; i < 12; i += 2)
                {
                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = 1,
                        buySymbol = "btc",
                        sellAmount = (1000 + i) * 1,
                        sellSymbol = "usd",
                        dateTime = new DateTime(2018, 1, i + 1),
                        exchangeRec = "binance",
                        exchangeSent = "binance",
                        combinedCount = 0
                    });


                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = (1030 + i) * 0.5,
                        buySymbol = "usd",
                        sellAmount = 0.5,
                        sellSymbol = "btc",
                        dateTime = new DateTime(2018, 1, i + 2),
                        exchangeRec = "coinbase",
                        exchangeSent = "coinbase",
                        combinedCount = 0
                    });
                }
                for (int i = 0; i < 12; i += 2)
                {
                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = 0.1,
                        buySymbol = "btc",
                        sellAmount = (2000 + i) * 0.1,
                        sellSymbol = "usd",
                        dateTime = new DateTime(2018, 3, i + 1),
                        exchangeRec = "binance",
                        exchangeSent = "binance",
                        combinedCount = 0
                    });


                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = (2030 + i) * 0.2,
                        buySymbol = "usd",
                        sellAmount = 0.2,
                        sellSymbol = "btc",
                        dateTime = new DateTime(2018, 3, i + 2),
                        exchangeRec = "coinbase",
                        exchangeSent = "coinbase",
                        combinedCount = 0
                    });
                }

            }
            t.transactionsOriginal.Add(new transaction
            {
                buyAmount = 3020 * 2.4,
                buySymbol = "usd",
                sellAmount = 2.4,
                sellSymbol = "btc",
                dateTime = new DateTime(2018, 9, 3),
                exchangeRec = "coinbase",
                exchangeSent = "coinbase",
                combinedCount = 0
            });
            List<bucket> buckets = new List<bucket>();

            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 24 * 31);

            var realizedbtc2 = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);

            var h = realizedbtc.Sum(x => x.gain);
            var h2 = realizedbtc2.Sum(x => x.gain);
            Assert.True(isClose(h, h2));

            var t22 = t.realizedTransToKeyValStringString(realizedbtc2);
            Assert.True(isClose(h, h2));
        }

        [Fact]
        public void DifferentSymbols()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            if (true)
            {
                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 1000.0,
                    buySymbol = "bnb",
                    sellAmount = 40 * 1000,
                    sellSymbol = "usd",
                    dateTime = new DateTime(2017, 1, 2),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 1
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 25.0,
                    buySymbol = "eth",
                    sellAmount = 250 * 25,
                    sellSymbol = "usd",
                    dateTime = new DateTime(2017, 1, 2),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 1
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 1000.0,
                    buySymbol = "sol",
                    sellAmount = 50 * 1000,
                    sellSymbol = "usd",
                    dateTime = new DateTime(2017, 1, 2),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 1
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 30000.0,
                    buySymbol = "hbar",
                    sellAmount = 30000.0 * 0.09,
                    sellSymbol = "usd",
                    dateTime = new DateTime(2017, 1, 2),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 1
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 20.0,
                    buySymbol = "btc",
                    sellAmount = 4000 * 20,
                    sellSymbol = "usd",
                    dateTime = new DateTime(2017, 1, 2),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 1
                });

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 5000*20.0,
                    buySymbol = "usd",
                    sellAmount =  20,
                    sellSymbol = "btc",
                    dateTime = new DateTime(2018, 1, 2),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 1


                });


                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 300*25.0,
                    buySymbol = "usd",
                    sellAmount = 25,
                    sellSymbol = "eth",
                    dateTime = new DateTime(2018, 1, 2),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 1
                });
            }
            List<bucket> buckets = new List<bucket>();
            t.convertBitcoinPairToTwoTransInUSD(t.transactionsOriginal);
            //t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 2);
            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);
            //t.computeBuysSellsSymbol(t.transactionsOriginal, "btc");
            var realizedeth = t.computeGains(out buckets, false, "eth", "fiho", t.transactionsOriginal);
            //t.computeBuysSellsSymbol(t.transactionsOriginal, "eth");


            //t.summerizeRealized("2017", realizedbtc);
            Assert.Null(realizedbtc.FirstOrDefault(x => x.trans.dateTime.Value.Year == 2017));
            //t.summerizeRealized("2018", realizedbtc);
            Assert.True(isClose(realizedbtc.First(x => x.trans.dateTime.Value.Year == 2018).gain, 20000.0));
            //t.summerizeRealized("2018", realizedeth);
            Assert.True(isClose(realizedeth.First(x => x.trans.dateTime.Value.Year == 2018).gain, 1250.0));
        }

        [Fact]
        public void SomeBTCTransactionsTestRealizedGainFIHO()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2017,1,1), 0.1, 200);
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2017,2,2), 0.1, 300);
            addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(2017,3,3), 0.15, 3100*0.15);
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2017,4,4), 0.1, 50);
            addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(2017,5,5), 0.12, 2100*0.12);
            
            List<bucket> buckets = new List<bucket>();

            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);
            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 24 * 31, true, 4.0);
            var realizedbtc2 = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);

            var realizedTrans38 = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);
            var t22 = t.realizedTransToKeyValStringString(realizedTrans38);
            t.printListListKeyValueStringString(t22, "\t", 7);

            int date1 = 0;
            int sellamount = 1;
            int gain = 3;
            int buyavg = 5;

            var firstRow = t22.First(x => x.First(y => y.Key.Contains("date")).Value.Contains("3/3/2017"));

            var f = isClose(float.Parse(firstRow.First(y => y.Key.Contains("gain")).Value), 10.0);

            /*
            $2000 buy 0.1
            $3000 buy 0.1
            $3100 sell -0.15 means two sell groups: $100 profit 0.1     100*0.1=$10        and $1100 profit with 0.05      1100 * 0.005 = $55
            $500 buy 0.1
            previouls have 0.05 at $2000
            $2100 sell 0.12 means sell 0.05 *100 = $5   and 0.07 * 1600 = $112
            */

            var frr = findRow(t22, "3/3/2017", "avg3000", 0.15);
            Assert.Equal("10", findRow(t22, "3/3/2017", "avg3000", 0.15).First(x => x.Key.Contains("gain")).Value);
            Assert.Equal("55", findRow(t22, "3/3/2017", "avg2000", 0.15).First(x => x.Key.Contains("gain")).Value);
            Assert.Equal("5", findRow(t22, "5/5/2017", "avg2000", 0.12).First(x => x.Key.Contains("gain")).Value);
            Assert.Equal("112", findRow(t22, "5/5/2017", "avg500", 0.12).First(x => x.Key.Contains("gain")).Value);  
        }

        [Fact]
        public void SomeBTCTransactionsTestReportExchangeBuySells()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year-1,1,1), 0.1, 200, "mtgox");

            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,1,1), 0.1, 200, "mtgox");
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,2,1), 0.1, 200, "mtgox");
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,3,1), 0.1, 200, "mtgox");
            addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,3,3), 0.5, 3000*0.5, "mtgox");
            addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,5,5), 0.5, 2000*0.5, "mtgox");
            
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,2,2), 0.1, 300,"coinbase");

            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,4,4), 0.1, 50, "kracken");
            addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,5,5), 0.12, 2100*0.12, "kracken");


            List<KeyValuePair<string, double>> perExchangeAndYear = t.computeVolumeExchanges(t.transactionsOriginal);

            foreach(KeyValuePair<string, double> g in perExchangeAndYear){

            
                Console.WriteLine(g.Key + "\t\t" + g.Value.ToString("000000.##") );
            }

            Assert.True(isClose(perExchangeAndYear.First(x => x.Key.Contains("btc_coinbase_b_sell_usd_" + DateTime.Now.Year)).Value, 0.1));

            Assert.True(isClose(perExchangeAndYear.First(x => x.Key.Contains("btc_mtgox_b_sell_usd_" + DateTime.Now.Year)).Value, 0.3));
            Assert.True(isClose(perExchangeAndYear.First(x => x.Key.Contains("usd_mtgox_s_buy__btc_" + DateTime.Now.Year)).Value, 600));

            Assert.True(isClose(perExchangeAndYear.First(x => x.Key.Contains("btc_mtgox_s_buy__usd_" + DateTime.Now.Year)).Value, 1.0));
            Assert.True(isClose(perExchangeAndYear.First(x => x.Key.Contains("usd_mtgox_b_sell_btc_" + DateTime.Now.Year)).Value, 2500));

            Assert.Equal(4, perExchangeAndYear.Count(x=> x.Key.Contains("mtgox")));

        }

        
        [Fact]
        public void testLastNTrans()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2017,1,1), 0.1, 200);
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2017,2,2), 0.1, 300);
            addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(2017,3,3), 0.15, 3100*0.15);
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2017,4,4), 0.1, 50);
            addUsdSellTransaction(t.transactionsOriginal, "btc", new DateTime(2017,5,5), 0.12, 2100*0.12);
            
            /*
            $2000 buy 0.1
            $3000 buy 0.1
            $3100 sell -0.15 means two sell groups: $100 profit 0.1     100*0.1=$10        and $1100 profit with 0.05      1100 * 0.005 = $55
            $500 buy 0.1
            previouls have 0.05 at $2000
            $2100 sell 0.12 means sell 0.05 *100 = $5   and 0.07 * 1600 = $112
            */

            List<String> g = t.lastNtrans("btc", t.transactionsOriginal, false); // get all of them

            foreach(string l in g)
            {
                Console.WriteLine(l);
            }

            List<String> expectOneResult = g.Where(x => x.Contains("-200.0")).ToList().Where(x => x.Contains("17/01/01")).ToList().Where(x=> x.Contains(".1000")).ToList();

            Assert.Equal(1, expectOneResult.Count());
        }

        [Fact]
        public void testVolumePerExchangeAndYear(){
            
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2024,1,1), 0.1, 200, "mtgox");
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(2024,2,2), 0.1, 300,"coinbase");

            List<KeyValuePair<string, double>> perExchangeAndYear = t.computeVolumeExchanges(t.transactionsOriginal, true, false);

            foreach(KeyValuePair<string, double> g in perExchangeAndYear)
            {
            
                Console.WriteLine(g.Key + "\t\t" + g.Value.ToString("000000.##") );
            }
            Assert.True(isClose(perExchangeAndYear.First(kvp => kvp.Key == "btc_coinbase_b_sell_usd_2024").Value, 0.10));
        }

        [Fact]
        public void testVolumePerExchangeAndYearONLYCURRENTYEAR(){
            
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicDayPriceUSD hp = new historicDayPriceUSD();
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,1,1), 0.1, 200, "mtgox");
            addUsdBuyTransaction(t.transactionsOriginal, "btc", new DateTime(DateTime.Now.Year,2,2), 0.1, 300,"coinbase");

            List<KeyValuePair<string, double>> perExchangeAndYear = t.computeVolumeExchanges(t.transactionsOriginal);

            foreach(KeyValuePair<string, double> g in perExchangeAndYear)
            {
            
                Console.WriteLine(g.Key + "\t\t" + g.Value.ToString("000000.##") );
            }
            // btc_coinbase_b_sell_usd_YEAR
            Assert.True(isClose(perExchangeAndYear.First(kvp => kvp.Key.Contains("btc_coinbase_b_sell_usd_")).Value, 0.10));
        }
        
        public List<KeyValuePair<string,string>> findRow(List<List<KeyValuePair<string,string>>> source, string containsArg1, string containsArg2, double floatArg3)
        {
            foreach(List<KeyValuePair<string,string>> row in source)
            {
                bool match1 = false;
                bool match2 = false;
                bool matchFloat = false;

                foreach(KeyValuePair<string, string> element in row)
                {
                    if(element.Value.ToLower().Contains(containsArg1))
                    {
                        match1 = true;
                    }
                    
                    if(element.Value.ToLower().Contains(containsArg2))
                    {
                        match2 = true;
                    }
                    double g = float.MinValue;
                    if(double.TryParse(element.Value, out g))
                    {
                        if(isClose(g, floatArg3))
                        {
                            matchFloat = true;
                        }
                    }
                    if(match1 && matchFloat && match2)
                    {
                        return row;
                    }
                }

            }
            return new List<KeyValuePair<string, string>>();
        }
    }
}
