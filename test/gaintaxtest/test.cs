using gaintaxlibrary;

namespace gaintaxtest{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }

        [Fact]
        public void Test2()
        {
            Assert.Equal(4,gaintaxlibrary.ClassGainTax.Go());
        
        }
 
        static bool isClose(double a, double b)
        {
            if (a < b * 1.0000001 && a > b * 0.999999)
            {
                return true;
            }
            return false;
        }

        /*[Fact]
        public void AddTransactions()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicPrice hp = new historicPrice();

            if (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    t.transactionsOriginal.Add(new transaction
                    {
                        buyAmount = 1,
                        buySymbol = "btc",
                        sellAmount = (1000 + i * 5) * 1,
                        sellSymbol = "usd",
                        transDate = new DateTime(2018, 1, i + 1),
                        exchangeRec = "binance",
                        exchangeSent = "binance",
                        combinedCount = 0
                    });
                }

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 1030 * 3,
                    buySymbol = "usd",
                    sellAmount = 3,
                    sellSymbol = "btc",
                    transDate = new DateTime(2018, 11, 10),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 0
                });

            }

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 2);
            List<bucket> bucketsbtc = new List<bucket>();
            var realizedbtc = t.computeGains(out bucketsbtc, false, "btc", "fiho", t.transactionsOriginal);
            t.computeBuysSellsSymbol(t.transactionsOriginal, "btc");

            t.summerizeRealized("2018", realizedbtc);
            var h = realizedbtc.First(x => x.trans.transDate.Value.Year == 2018).gain;
            Assert.True(isClose(h, 75.0));

        }*/

/*        [Fact]
        public void AddManyTransactions()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicPrice hp = new historicPrice();

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
                        transDate = new DateTime(2018, 1, i + 1),
                        exchangeRec = "binance",
                        exchangeSent = "binance",
                        combinedCount = 0
                    });
                }

                t.transactionsOriginal.Add(new transaction
                {
                    buyAmount = 1030 * 30,
                    buySymbol = "usd",
                    sellAmount = 30,
                    sellSymbol = "btc",
                    transDate = new DateTime(2018, 11, 10),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 0
                });

            }

            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 2);
            List<bucket> bucketsbtc = new List<bucket>();
            var realizedbtc = t.computeGains(out bucketsbtc, false, "btc", "fiho", t.transactionsOriginal);
            t.computeBuysSellsSymbol(t.transactionsOriginal, "btc");

            t.summerizeRealized("2018", realizedbtc);
            var h = realizedbtc.First(x => x.trans.transDate.Value.Year == 2018).gain;
            Assert.True(isClose(h, 465.0));

        }
*/

        [Fact]
        public void ManyTransactionsTestCombine1()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicPrice hp = new historicPrice();

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
                        transDate = new DateTime(2018, 1, i + 1),
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
                        transDate = new DateTime(2018, 1, i + 2),
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
                transDate = new DateTime(2018, 9, 3),
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

            historicPrice hp = new historicPrice();

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
                        transDate = new DateTime(2018, 1, i + 1),
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
                        transDate = new DateTime(2018, 1, i + 2),
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
                        transDate = new DateTime(2018, 3, i + 1),
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
                        transDate = new DateTime(2018, 3, i + 2),
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
                transDate = new DateTime(2018, 9, 3),
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

            historicPrice hp = new historicPrice();
            hp.historicDate = new DateOnly(2018, 1, 3);
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
                    transDate = new DateTime(2018, 1, 2),
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
                    transDate = new DateTime(2019, 1, 10),
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
                    transDate = new DateTime(2018, 1, 3),
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
                    transDate = new DateTime(2018, 1, 5),
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
                    transDate = new DateTime(2019, 1, 6),
                    exchangeRec = "binance",
                    exchangeSent = "binance",
                    combinedCount = 0
                });
            }

            List<bucket> buckets = new List<bucket>();
            t.convertBitcoinPairToTwoDollarTrans(t.transactionsOriginal, 0.0);
            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 2);
            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);
            t.computeBuysSellsSymbol(t.transactionsOriginal, "btc");
            var realizedeth = t.computeGains(out buckets, false, "eth", "fiho", t.transactionsOriginal);
            t.computeBuysSellsSymbol(t.transactionsOriginal, "eth");

            t.summerizeRealized("2018", realizedbtc);
            Assert.True(isClose(realizedbtc.First(x => x.trans.transDate.Value.Year == 2018).gain, 100.0));
            t.summerizeRealized("2019", realizedbtc);
            Assert.True(isClose(realizedbtc.First(x => x.trans.transDate.Value.Year == 2019).gain, 1800.0));
            t.summerizeRealized("2018", realizedeth);
            Assert.True(isClose(realizedeth.First(x => x.trans.transDate.Value.Year == 2018).gain, 50.0));
            t.summerizeRealized("2019", realizedeth);
            Assert.True(isClose(realizedeth.First(x => x.trans.transDate.Value.Year == 2019).gain, 150.0));


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

            historicPrice hp = new historicPrice();

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
                        transDate = new DateTime(2018, 1, i + 1),
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
                        transDate = new DateTime(2018, 1, i + 2),
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
                        transDate = new DateTime(2018, 3, i + 1),
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
                        transDate = new DateTime(2018, 3, i + 2),
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
                transDate = new DateTime(2018, 9, 3),
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

            var t22 = t.realizedTransToString(realizedbtc2);
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
                    transDate = new DateTime(2017, 1, 2),
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
                    transDate = new DateTime(2017, 1, 2),
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
                    transDate = new DateTime(2017, 1, 2),
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
                    transDate = new DateTime(2017, 1, 2),
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
                    transDate = new DateTime(2017, 1, 2),
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
                    transDate = new DateTime(2018, 1, 2),
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
                    transDate = new DateTime(2018, 1, 2),
                    exchangeRec = "coinbase",
                    exchangeSent = "coinbase",
                    combinedCount = 1
                });
            }
            List<bucket> buckets = new List<bucket>();
            t.convertBitcoinPairToTwoDollarTrans(t.transactionsOriginal);
            t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 2);
            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);
            t.computeBuysSellsSymbol(t.transactionsOriginal, "btc");
            var realizedeth = t.computeGains(out buckets, false, "eth", "fiho", t.transactionsOriginal);
            t.computeBuysSellsSymbol(t.transactionsOriginal, "eth");


            t.summerizeRealized("2017", realizedbtc);
            Assert.Null(realizedbtc.FirstOrDefault(x => x.trans.transDate.Value.Year == 2017));
            t.summerizeRealized("2018", realizedbtc);
            Assert.True(isClose(realizedbtc.First(x => x.trans.transDate.Value.Year == 2018).gain, 20000.0));
            t.summerizeRealized("2018", realizedeth);
            Assert.True(isClose(realizedeth.First(x => x.trans.transDate.Value.Year == 2018).gain, 1250.0));
        }

        [Fact]
        public void SomeBTCTransactionsTestRealizedGain()
        {
            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = new List<transaction>();

            historicPrice hp = new historicPrice();

            t.transactionsOriginal.Add(new transaction
            {
                buyAmount = 0.1,
                buySymbol = "btc",
                sellAmount = (2000) * 0.1,
                sellSymbol = "usd",
                transDate = new DateTime(2017, 1, 1),
                exchangeRec = "binance",
                exchangeSent = "binance",
                combinedCount = 0
            });
            t.transactionsOriginal.Add(new transaction
             {
                 buyAmount = 0.1,
                 buySymbol = "btc",
                 sellAmount = (3000) * 0.1,
                 sellSymbol = "usd",
                 transDate = new DateTime(2017, 2, 2),
                 exchangeRec = "binance",
                 exchangeSent = "binance",
                 combinedCount = 0
             });

            t.transactionsOriginal.Add(new transaction
            {
                buyAmount = (3100) * 0.15, //465
                buySymbol = "usd",
                sellAmount = 0.15,
                sellSymbol = "btc",
                transDate = new DateTime(2017, 3, 3),
                exchangeRec = "coinbase",
                exchangeSent = "coinbase",
                combinedCount = 0
            });

            t.transactionsOriginal.Add(new transaction
            {
                buyAmount = 0.1,
                buySymbol = "btc",
                sellAmount = (500) * 0.1,
                sellSymbol = "usd",
                transDate = new DateTime(2017, 4, 4),
                exchangeRec = "binance",
                exchangeSent = "binance",
                combinedCount = 0
            });

            t.transactionsOriginal.Add(new transaction
            {
                buyAmount = (2100) * 0.12,
                buySymbol = "usd",
                sellAmount = 0.12,
                sellSymbol = "btc",
                transDate = new DateTime(2017, 5, 5),
                exchangeRec = "coinbase",
                exchangeSent = "coinbase",
                combinedCount = 0
            });

            List<bucket> buckets = new List<bucket>();

            //var realizedbtc = t.computeGains(out buckets, true, "btc", "fiho", t.transactionsOriginal);
            //t.combineTransactionsInHourLongWindow_MODIFIES_transactions(t.transactionsOriginal, 24 * 31);
            //var realizedbtc2 = t.computeGains(out buckets, true, "btc", "fiho", t.transactionsOriginal);

            var realizedTrans38 = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);
            var t22 = t.realizedTransToString(realizedTrans38);
            t.printListListString(7, "\t", t22);

            int date1 = 0;
            int sellamount = 1;
            int gain = 2;
            int buyavg = 4;

            var MYDEBUG = t22.First(x => x[date1].Contains("3/3/2017"));
            var f = isClose(float.Parse(MYDEBUG[sellamount]),10.0);

            /*
            $2000 buy 0.1
            $3000 buy 0.1
            $3100 sell -0.15 means two sell groups: $100 profit 0.1     100*0.1=$10        and $1100 profit with 0.05      1100 * 0.005 = $55
            $500 buy 0.1
            previouls have 0.05 at $2000
            $2100 sell 0.12 means sell 0.05 *100 = $5   and 0.07 * 1600 = $112
            */

            Assert.Equal(t22.First(x => x[date1].Contains("3/3/2017") && isClose(float.Parse(x[sellamount]), 0.15) && x[buyavg].Contains("avg3000"))[gain], "10");
            Assert.Equal(t22.First(x => x[date1].Contains("3/3/2017") && isClose(float.Parse(x[sellamount]), 0.15) && x[buyavg].Contains("avg2000"))[gain], "55");
            Assert.Equal(t22.First(x => x[date1].Contains("5/5/2017") && isClose(float.Parse(x[sellamount]), 0.12) && x[buyavg].Contains("avg2000"))[gain], "5");
            Assert.Equal(t22.First(x => x[date1].Contains("5/5/2017") && isClose(float.Parse(x[sellamount]), 0.12) && x[buyavg].Contains("avg500"))[gain], "112");
        }
    }

}