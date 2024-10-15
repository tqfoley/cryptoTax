//using System.Reflection.Metadata.Ecma335;
using System.Globalization;


namespace gaintaxlibrary
{
    
    public class transaction
    {
        public DateTime? dateTime;
        public int combinedCount = 0;
        public DateTime? optionalSecondTansDate;

        public string? buySymbol;
        public double buyAmount;
        public string? exchangeRec;

        public string? sellSymbol;
        public double sellAmount;
        public string? exchangeSent;

        public string? feeSymbol;
        public double feeAmount;
        public string? altFeeSymbol;
        public double? altFeeAmount;

        public transaction deepCopy()
        {
            transaction ret = new transaction();
            ret.dateTime = this.dateTime;
            ret.buySymbol = this.buySymbol;
            ret.sellSymbol = this.sellSymbol;
            ret.buyAmount = this.buyAmount;
            ret.sellAmount = this.sellAmount;
            ret.exchangeRec = this.exchangeRec;
            ret.exchangeSent = this.exchangeSent;
            ret.optionalSecondTansDate = this.optionalSecondTansDate;
            ret.feeSymbol = this.feeSymbol;
            ret.feeAmount = this.feeAmount;
            ret.altFeeAmount = this.altFeeAmount;
            ret.altFeeSymbol = this.altFeeSymbol;
            return ret;
        }
    }

    public class transfer
    {
        public DateTime? dateTime;
        public bool receive;
        public string? symbol;
        public double amount;
        public string? toAccount;
        public string? fromAccount;
    }

    public class bucket // buckets for selling, can reorder for FIFO, LIFO, MaxCost
    {
        public string? symbol;
        public double amount;
        public double price;
        public transaction? trans;
    }

    public class splitTransaction
    {
        public transaction trans;
        public double portion; // aka weight, when selling a buy might need to by split into multiple portions
    }

    public class realizedTransaction
    {
        public double gain;
        public transaction? trans;
        public double amount;
        public double sellAmountReceivedUsuallyDollars; // usually in dollars
        public List<splitTransaction>? initialEntryBuySplitTrans;

        public realizedTransaction deepCopy()
        {  
            realizedTransaction b = new realizedTransaction();
            b.amount = this.amount;
            b.gain = this.gain;
            b.initialEntryBuySplitTrans = new List<splitTransaction>();
            foreach(var t in this.initialEntryBuySplitTrans){
                var r = new splitTransaction{portion = t.portion, trans = new transaction()};
                r.trans = t.trans.deepCopy();
                b.initialEntryBuySplitTrans.Add(r);
            }
            b.sellAmountReceivedUsuallyDollars = this.sellAmountReceivedUsuallyDollars;
            b.trans = this.trans.deepCopy();
            return b;
        }
    }

    public class historicDayPriceUSD
    {
        public double low;
        public double high;
        public double firstPrice;
        public double lastPrice;
        public double volume;
        public DateOnly dateOnly;
    }

    public class ClassGainTax
    {
        public class total
        {
            public string? buySymbol;
            public string? sellSymbol;
            public double Amount;
            public double Average;

        }

        public List<transaction> transactionsOriginal = new List<transaction>();
        public List<transfer> transfers1 = new List<transfer>();
        public List<historicDayPriceUSD> historicPrices = new List<historicDayPriceUSD>();

        public bool isCloseToCombine(double a, double b, double combineSensetivity)
        {
            if (a * (1.0 + combineSensetivity) > b && a * (1.0 - combineSensetivity) < b)
                return true;
            return false;
        }
        public bool IsTransactionMarkedDeleted(transaction t)
        {
            if(t.buySymbol.Contains("deleted"))
            {
                return true;
            }
            if(t.sellSymbol.Contains("deleted"))
            {
                return true;
            }
            if(t.feeSymbol != null)
            {
                if(t.feeSymbol.Contains("deleted"))
                {
                    return true;
                }
            }
            return false;
        }
        
        public void MarkTransactionInsignificant(transaction t)
        {
            //    if(t.buyAmount > 0.000001)
            t.buyAmount = t.buyAmount / 1000000000;
            t.buySymbol = t.buySymbol + "deleted";
            t.sellSymbol = t.sellSymbol + "deleted";
            t.sellAmount = t.sellAmount / 1000000000;
            t.feeAmount = 0;
            t.feeSymbol = "deleted";
            //t.sellSymbol = "usd";
            //t.transDate = new DateTime(2017, 1, 1) ;

        }
        
        public void combineTwoTransactions(transaction a, transaction b, int timeSpanHours, List<transaction> transactionsCheck, double combineSensetivity)
        {

            double originalBuyAmount = 0;
            double originalSellAmount = 0;


            if ((a.buySymbol == b.buySymbol &&
                a.sellSymbol == b.sellSymbol) &&
                a.dateTime.Value.Add(new TimeSpan(timeSpanHours, 0, 0)) > b.dateTime &&
                a.dateTime.Value.Subtract(new TimeSpan(timeSpanHours, 0, 0)) < b.dateTime &&
                isCloseToCombine(a.sellAmount / a.buyAmount,
                    b.sellAmount / b.buyAmount, combineSensetivity) &&
                a.dateTime.Value.Year == b.dateTime.Value.Year
                )
            {
                //originalBuyAmount = a.buyAmount;
                //originalSellAmount = a.sellAmount;
                a.buyAmount += b.buyAmount;
                a.sellAmount += b.sellAmount;
                //a.exchangeRec = a.exchangeRec;
                //a.exchangeSent = a.exchangeSent;
                if (!a.exchangeRec.Contains("comb"))
                {
                    a.exchangeRec += a.exchangeRec + " comb ";
                }
                if (!a.exchangeSent.Contains("comb"))
                {
                    a.exchangeSent += a.exchangeSent + " comb ";
                }
                if (a.feeSymbol != null && b.feeSymbol != null)
                {
                    if (b.feeSymbol.Contains(a.feeSymbol))
                    {
                        a.feeAmount += b.feeAmount;
                    }
                    else
                    {
                        Console.WriteLine("fee issue");

                    }
                }
                else
                {
                    Console.WriteLine("fee issue");

                }
                //if (b.exchangeRec.Contains("coinbase"))
                //{

                //}

                if (a.optionalSecondTansDate is null)
                {
                    a.optionalSecondTansDate = b.dateTime.Value;
                }
                else
                {
                    if (a.optionalSecondTansDate.Value < b.dateTime.Value)
                    {
                        a.optionalSecondTansDate = b.dateTime.Value;
                    }
                }
                MarkTransactionInsignificant(b);
                a.combinedCount = a.combinedCount + 1 + b.combinedCount;
                if (a.combinedCount > 51153 || b.combinedCount > 51153)
                {
                    Console.WriteLine("Ds");
                }
            }
            else
            {
                if (
                a.dateTime.Value.Year != b.dateTime.Value.Year)
                {
                    throw new Exception("bad date");

                }
                //Console.WriteLine("bad trying to combine but not purchased near same amount");
            }

        }


        public int combineTransactionsInHourLongWindow_MODIFIES_transactions(List<transaction> transactions, int timeSpanHours, bool limitOnlyUSDTransactions = true, double combineSensitivityPercentDiff = 0.05)
        {
            if (timeSpanHours == 22)
            {
                timeSpanHours = 22;
            }

            int combined = 0;
            //int currentcombined = 0
            for (int i = 0; i < transactions.Count; i++)
            {
                //if (i < transactions.Count-5) THIS CAUSES A BUG
                //{
                //    i = transactions.Count - 5;
                //}
                transaction first = transactions.ElementAt(i);

                //if (first.sellSymbol.Contains("bnb") && first.transDate.ToString().Contains("5/6/2024") && 
                //    first.transDate.ToString().Contains("8:15")
                //    )
                //{//5 / 6 / 2024 8:15:55 AM btc bnb 0.00051778 0.056 ratio 0.009246071428571428)
                //first.sellSymbol = "bnb";
                //}
                if (!limitOnlyUSDTransactions
                    || (limitOnlyUSDTransactions == true
                       && (!first.sellSymbol.ToLower().Contains("delete"))
                       && (!first.buySymbol.ToLower().Contains("delete"))
                        && (first.sellSymbol.ToLower().Contains("usd") || first.buySymbol.ToLower().Contains("usd"))
                       )
                   )
                {

                    for (int j = i + 1; j < i + 5329 && j < transactions.Count; j++)
                    {
                        if (first.dateTime.Value.Year == 2024)
                        {
                            //Console.WriteLine("sdf");
                        }
                            //var exchangeone = transactions.ElementAt(j).exchangeRec;
                            //var exchangetwo = transactions.ElementAt(j).exchangeSent;
                            
                        if (first.buySymbol == transactions.ElementAt(j).buySymbol &&
                           first.sellSymbol.ToLower() == transactions.ElementAt(j).sellSymbol.ToLower() &&
                           first.dateTime.Value.Add(new TimeSpan(timeSpanHours, 0, 0)) > transactions.ElementAt(j).dateTime &&
                           first.dateTime.Value.Subtract(new TimeSpan(timeSpanHours, 0, 0)) < transactions.ElementAt(j).dateTime &&
                           first.buyAmount > 0.00000007 &&
                           //first.exchangeRec == transactions.ElementAt(j).exchangeRec &&
                           //first.exchangeSent == transactions.ElementAt(j).exchangeSent &&
                           first.dateTime.Value.Year == transactions.ElementAt(j).dateTime.Value.Year 
                           
                           )
                        {
                            if (isCloseToCombine(first.sellAmount / first.buyAmount,
                                transactions.ElementAt(j).sellAmount / transactions.ElementAt(j).buyAmount, combineSensitivityPercentDiff) ||
                                isCloseToCombine(first.buyAmount / first.sellAmount,
                                transactions.ElementAt(j).buyAmount / transactions.ElementAt(j).sellAmount, combineSensitivityPercentDiff)
                                )
                            {
                                if (false//(first.transDate.Value.Day == 1 && first.transDate.Value.Month == 1) ||
                                         //(first.transDate.Value.Day == 31 && first.transDate.Value.Month == 12)
                                   )
                                {
                                    //Console.WriteLine(first.transDate.ToString());
                                }
                                else
                                {
                                    var g = transactions.ElementAt(j);
                                    //if((g.buyAmount > 1325 && g.buyAmount < 1326) || (first.buyAmount > 1325 && first.buyAmount < 1326))
                                    //{
                                    //    int h = 0;
                                    //}
                                    //combineTwoTransactions(first, transactions.ElementAt(j), timeSpanHours, transactions, combineSensetivity);
                                    combineTwoTransactions(first, transactions.ElementAt(j), timeSpanHours, transactions, combineSensitivityPercentDiff);
                                    combined++;
                                    //currentcombined++;
                                }
                            }
                            else
                            {
                                if (combineSensitivityPercentDiff < 0.15)
                                {
                                    Console.WriteLine("large chnage in " + timeSpanHours + " hours  " + first.dateTime + first.buySymbol);
                                }
                            }
                        }
                    }




                    /*for (int j = i + 1; j < i + 5329 && j < transactions.Count; j++)
                    {
                        var exchangeone = transactions.ElementAt(j).exchangeRec;
                        var exchangetwo = transactions.ElementAt(j).exchangeSent;
                        if (first.buySymbol == transactions.ElementAt(j).buySymbol &&
                           first.sellSymbol.ToLower() == transactions.ElementAt(j).sellSymbol.ToLower() &&
                           first.transDate.Value.Add(new TimeSpan(timeSpanHours, 0, 0)) > transactions.ElementAt(j).transDate &&
                           first.transDate.Value.Subtract(new TimeSpan(timeSpanHours, 0, 0)) < transactions.ElementAt(j).transDate &&
                           first.buyAmount > 0.00000007 &&
                                                        first.exchangeRec == transactions.ElementAt(j).exchangeRec &&
                                                        first.exchangeSent == transactions.ElementAt(j).exchangeSent &&
                                                        first.transDate.Value.Year == transactions.ElementAt(j).transDate.Value.Year
                           )
                        {
                            if (isCloseToCombine(first.sellAmount / first.buyAmount,
                                transactions.ElementAt(j).sellAmount / transactions.ElementAt(j).buyAmount, combineSensitivityPercentDiff) &&
                                first.transDate.Value.Year == transactions.ElementAt(j).transDate.Value.Year)
                            {
                                if (false//(first.transDate.Value.Day == 1 && first.transDate.Value.Month == 1) ||
                                         //(first.transDate.Value.Day == 31 && first.transDate.Value.Month == 12)
                                   )
                                {
                                    //Console.WriteLine(first.transDate.ToString());
                                }
                                else
                                {
                                    var g = transactions.ElementAt(j);
                                    //if((g.buyAmount > 1325 && g.buyAmount < 1326) || (first.buyAmount > 1325 && first.buyAmount < 1326))
                                    //{
                                    //    int h = 0;
                                    //}
                                    combineTwoTransactions(first, transactions.ElementAt(j), timeSpanHours, transactions, combineSensitivityPercentDiff);
                                    combined++;
                                    //currentcombined++;
                                }
                            }
                            else
                            {
                                //if (combineSensitivity < 0.15)
                                {
                                    Console.WriteLine("large chnage in " + timeSpanHours + " hours  " + first.transDate + first.buySymbol);
                                }
                            }
                        }
                    }
                    for (int j = i + 1; j < i + 5329 && j < transactions.Count; j++)
                    {

                        if (first.sellSymbol == transactions.ElementAt(j).sellSymbol &&
                           first.buySymbol.ToLower() == transactions.ElementAt(j).buySymbol.ToLower() &&
                           first.transDate.Value.Add(new TimeSpan(timeSpanHours, 0, 0)) > transactions.ElementAt(j).transDate &&
                           first.transDate.Value.Subtract(new TimeSpan(timeSpanHours, 0, 0)) < transactions.ElementAt(j).transDate &&
                           ((first.buyAmount > 2.5 && limitOnlyUSDTransactions) || (!limitOnlyUSDTransactions && first.buyAmount > 0.00000007))  //&& // only combine if over $2.5 
                                                                                                                                                 // first.exchangeRec == transactions.ElementAt(j).exchangeRec &&
                                                                                                                                                 // first.exchangeSent == transactions.ElementAt(j).exchangeSent
                           )
                        {
                            if (isCloseToCombine(first.buyAmount / first.sellAmount,
                                transactions.ElementAt(j).buyAmount / transactions.ElementAt(j).sellAmount, combineSensitivityPercentDiff) &&
                                first.transDate.Value.Year == transactions.ElementAt(j).transDate.Value.Year)
                            {
                                if (false//(first.transDate.Value.Day == 1 && first.transDate.Value.Month == 1) ||
                                         //(first.transDate.Value.Day == 31 && first.transDate.Value.Month == 12)
                                   )
                                {
                                    //Console.WriteLine(first.transDate.ToString());
                                }
                                else
                                {
                                    var g = transactions.ElementAt(j);
                                    //if ((g.buyAmount > 1325 && g.buyAmount < 1326) || (first.buyAmount > 1325 && first.buyAmount < 1326))
                                    //{
                                    //    int h = 0;
                                    //}
                                    //combineTwoTransactions(first, transactions.ElementAt(j), timeSpanHours, transactions, combineSensetivity);
                                    combineTwoTransactions(first, transactions.ElementAt(j), timeSpanHours, transactions, combineSensitivityPercentDiff);
                                    combined++;
                                    //currentcombined++;
                                }
                            }
                            else
                            {
                                if (combineSensitivityPercentDiff < 0.15)
                                {
                                    Console.WriteLine("large chnage in " + timeSpanHours + " hours  " + first.transDate + first.buySymbol);
                                }
                            }
                        }
                    }*/
                }
            }
            Console.WriteLine("combined " + combined);

            List<transaction> ignoreInsignificantTrans = new List<transaction>();
            foreach (transaction f in transactions)
            {
                if (f.buySymbol.Contains("deleted") || f.sellSymbol.Contains("deleted"))
                { }
                else
                {
                    //if (f.exchangeRec.Contains("oinbase") || f.exchangeSent.Contains("oinbase"))
                    {
                        ignoreInsignificantTrans.Add(f);
                    }
                }
            }
            return ignoreInsignificantTrans.Count;

        }

        public void deleteSingleLastZeroAmountInList(List<bucket> t)
        {
            int lastindex = t.Count - 1;
            if (t.ElementAt(lastindex).amount > 0.001)
            {
                Console.WriteLine("bad");
            }
            t.RemoveAt(lastindex);

            foreach (var s in t)
            {
                if (s.amount < 0.000001)
                {
                    //Console.WriteLine("Small Amount");
                }
            }

        }

        public List<realizedTransaction> computeGains(out List<bucket> buckets, bool combineRealizedTransactionOnSameDay, string symbol, string mode, List<transaction> transactions, DateTime? endDate = null, bool printIndividualTrans = false)
        {
            Console.WriteLine("gains for " + symbol);
            
            List<realizedTransaction> realizedTransactions = new List<realizedTransaction>();
            
            if (endDate == null)
            {
                Console.WriteLine("END DATE Dec 31 2024!");
                endDate = new DateTime(2024, 12, 31);
            }
            buckets = new List<bucket>();
            
            List<KeyValuePair<string, string>> ValidPairSymbols = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>(symbol, "usd")
            };

            DateOnly prevDate = new DateOnly(2016, 12, 31);
            double lastExchangePrice = 0;

            int countTrans = 0;
            bool Inverted = false; // used for selling first
            bool firstTransaction = false;  // set inverted based on first transaction

            var filteredTrans = transactions.Where(x => x.buySymbol.Contains(symbol) || x.sellSymbol.Contains(symbol));
            double sellAmount = transactions.Where(x => x.sellSymbol.Contains(symbol)).Sum(x => x.sellAmount);
            double buyAmount = transactions.Where(x => x.buySymbol.Contains(symbol)).Sum(x => x.buyAmount);

            foreach (transaction currentTransaction in filteredTrans)
            {
                if (!firstTransaction && currentTransaction.buySymbol.Contains(symbol))
                {
                    firstTransaction = true;
                }
                    
                if (!firstTransaction && currentTransaction.sellSymbol.Contains(symbol))
                {
                    // since selling the symbol its not selling USD and buying the symbol 
                    Inverted = true;
                    firstTransaction = true;
                }
                countTrans++;
                //Console.WriteLine(countTrans);
                /*if (countTrans == 135)
                {
                    countTrans = 135;
                }*/
                DateOnly currentDate = new DateOnly(currentTransaction.dateTime.Value.Date.Year,
                    currentTransaction.dateTime.Value.Date.Month,
                    currentTransaction.dateTime.Value.Date.Day);

                if (currentDate < prevDate)
                {
                    Console.WriteLine("Exception transactions not ordered properly");
                    throw new Exception("bad");
                }
                prevDate = new DateOnly(currentTransaction.dateTime.Value.Date.Year,
                    currentTransaction.dateTime.Value.Date.Month,
                    currentTransaction.dateTime.Value.Date.Day);

                transaction? prevTrans = null;
                if (currentTransaction.dateTime.Value < endDate.Value)
                {
                    // ensure  tranactions in chronological order todo
                    if(prevTrans != null && prevTrans.dateTime > currentTransaction.dateTime)
                    {
                        throw new Exception("Transactions not in expected order");
                    }
                    prevTrans = currentTransaction;

                    // buy with  usd
                    KeyValuePair<string, string> specificPair = default(KeyValuePair<string, string>);
                    if (!Inverted)
                    {
                        specificPair = ValidPairSymbols.FirstOrDefault(validpair =>
                        validpair.Key == currentTransaction.buySymbol.ToLower() &&
                        currentTransaction.sellSymbol.ToLower().Contains("usd")); // sell symbol
                    }
                    else
                    {
                        specificPair = ValidPairSymbols.FirstOrDefault(validpair =>
                        validpair.Key == currentTransaction.sellSymbol.ToLower() &&
                        currentTransaction.buySymbol.ToLower().Contains("usd")); // buy symbol
                    }

                    if (specificPair.Equals(default(KeyValuePair<string, string>)))
                    {
                        //not found
                        //Console.WriteLine("not found");
                    }
                    else
                    {
                        if (!Inverted)
                        {
                            if (currentTransaction.buyAmount > 0.00001 && !IsTransactionMarkedDeleted(currentTransaction))
                            {
                                buckets.Add(new bucket
                                {
                                    amount = currentTransaction.buyAmount,
                                    symbol = currentTransaction.sellSymbol,
                                    price = currentTransaction.sellAmount / currentTransaction.buyAmount, // sell over buy
                                    trans = currentTransaction
                                });
                            }
                        }
                        if (Inverted)
                        {
                            if (currentTransaction.sellAmount > 0.00001 && !IsTransactionMarkedDeleted(currentTransaction))
                            {
                                buckets.Add(new bucket
                                {
                                    amount = currentTransaction.sellAmount,
                                    symbol = currentTransaction.buySymbol,
                                    price = currentTransaction.buyAmount / currentTransaction.sellAmount, // buy over sell
                                    trans = currentTransaction
                                });
                            }
                        }

                        if (buckets.Last().amount < 0.00001)
                        {
                            int t = 0;

                            throw new Exception("unexpected last bucket");
                        }

                        if (!Inverted || Inverted)
                        {
                            if (mode == "fiho") // first in highest out
                            {
                                //rearrange List Highest At Back
                                buckets = buckets.OrderBy(x => x.price).ToList();
                            }
                            if (mode == "fiso") // first in smallest out
                            {
                                //rearrange List Lowest At Back
                                buckets = buckets.OrderBy(x => -1.0 * x.price).ToList();
                            }
                            if (mode == "filo") // first in last out
                            {
                                //rearrange List last transaction At Back
                                buckets = buckets.OrderBy(x => x.trans.dateTime.Value).ToList();
                            }
                        }
                    }

                    //sell token get usd
                    KeyValuePair<string, string> specificPairSell = default(KeyValuePair<string, string>);// new KeyValuePair<string, string>("BAD", "BAD");
                    if (!Inverted)
                    {
                        specificPairSell = ValidPairSymbols.FirstOrDefault(validpair =>
                        validpair.Key == currentTransaction.sellSymbol.ToLower() &&
                        currentTransaction.buySymbol.ToLower().Contains("usd"));
                    }
                    else
                    {
                        specificPairSell = ValidPairSymbols.FirstOrDefault(validpair =>
                        validpair.Key == currentTransaction.buySymbol.ToLower() &&
                        currentTransaction.sellSymbol.ToLower().Contains("usd"));
                    }


                    if (specificPairSell.Equals(default(KeyValuePair<string, string>)))
                    {
                        Console.WriteLine("not found");
                        //not found
                        //throw new Exception("specific pair not found");
                    }
                    else
                    {
                        //need to loop untill all the sell amount is matched with enough buckets to value amount of sell
                        transaction copyCurrentTransaction = currentTransaction.deepCopy();
                        bool keepGoing = true;
                        while (keepGoing)//need to loop untill all the sell amount is matched with enough buckets to value amount of sell
                        {
                            if (!Inverted)
                            {
                                if (buckets.Last().amount > copyCurrentTransaction.sellAmount)
                                {
                                    double realizedGain = (copyCurrentTransaction.buyAmount / copyCurrentTransaction.sellAmount - buckets.Last().price) * copyCurrentTransaction.sellAmount;
                                    //transaction t = currentTransaction;
                                    //double a = copyCurrentTransaction.sellAmount;
                                    //double g = realizedGain;
                                    lastExchangePrice = copyCurrentTransaction.buyAmount / copyCurrentTransaction.sellAmount;

                                    var newRealizedGain = new realizedTransaction
                                    {
                                        trans = currentTransaction,  // sell trans
                                        amount = copyCurrentTransaction.sellAmount,
                                        sellAmountReceivedUsuallyDollars = copyCurrentTransaction.buyAmount,
                                        gain = realizedGain,
                                        initialEntryBuySplitTrans = new List<splitTransaction>()
                                    };
                                    splitTransaction splittrans = new splitTransaction();
                                    splittrans.portion = copyCurrentTransaction.sellAmount / buckets.Last().amount; // portion or weight
                                    splittrans.trans = buckets.Last().trans;
                                    newRealizedGain.initialEntryBuySplitTrans.Add(splittrans);
                                    realizedTransactions.Add(newRealizedGain);

                                    buckets.Last().amount -= copyCurrentTransaction.sellAmount;
                                    if (buckets.Last().amount < 0.000001)
                                    {
                                        if (buckets.Last().amount < -0.0001)
                                        {
                                            throw new Exception("Bad selling more than exists");
                                        }
                                        deleteSingleLastZeroAmountInList(buckets);
                                    }
                                    keepGoing = false;
                                }
                                else
                                {
                                    double realizedGain = (copyCurrentTransaction.buyAmount / copyCurrentTransaction.sellAmount - buckets.Last().price) * buckets.Last().amount;
                                    double volume = copyCurrentTransaction.buyAmount * (buckets.Last().amount / copyCurrentTransaction.sellAmount); // check this ratio later, buyamount in dollars
                                    if (buckets.Last().amount > copyCurrentTransaction.sellAmount)
                                    {
                                        throw new Exception("Ratio error");
                                    }

                                    var temp = new realizedTransaction
                                    {
                                        trans = currentTransaction,
                                        amount = buckets.Last().amount,
                                        gain = realizedGain,
                                        sellAmountReceivedUsuallyDollars = volume,
                                        initialEntryBuySplitTrans = new List<splitTransaction>()
                                    };

                                    splitTransaction splittrans = new splitTransaction();
                                    splittrans.portion = 1.0; // portion or weight
                                    splittrans.trans = buckets.Last().trans;

                                    temp.initialEntryBuySplitTrans.Add(splittrans);
                                    realizedTransactions.Add(temp);

                                    //update copyCurrentTransaction
                                    copyCurrentTransaction.buyAmount *= (copyCurrentTransaction.sellAmount - buckets.Last().amount) / copyCurrentTransaction.sellAmount;
                                    copyCurrentTransaction.sellAmount -= buckets.Last().amount;
                                    buckets.Last().amount = 0;
                                    deleteSingleLastZeroAmountInList(buckets);
                                    if (copyCurrentTransaction.sellAmount > 0.0001 && buckets.Count == 0)
                                    {
                                        throw new Exception("bad not enough in buckets to sell");

                                    }
                                    if (copyCurrentTransaction.sellAmount < 0)
                                    {
                                        Console.Write("error");
                                        throw new Exception("error sell amount less than zero");
                                    }
                                    if (copyCurrentTransaction.sellAmount < 0.000001)
                                    {
                                        keepGoing = false;
                                    }
                                }
                            }


                            if (Inverted)
                            {

                                if (buckets.Last().amount > copyCurrentTransaction.buyAmount)
                                {
                                    double realizedGain = (copyCurrentTransaction.sellAmount / copyCurrentTransaction.buyAmount - buckets.Last().price) * copyCurrentTransaction.buyAmount * -1.0; // inverted so times -1

                                    lastExchangePrice = copyCurrentTransaction.sellAmount / copyCurrentTransaction.buyAmount;

                                    var temp = new realizedTransaction
                                    {
                                        trans = currentTransaction,  // sell trans
                                        amount = copyCurrentTransaction.buyAmount,//  fix aug 28th   was dividing sell amount by  buckets.Last().amount),
                                        sellAmountReceivedUsuallyDollars = copyCurrentTransaction.sellAmount,
                                        gain = realizedGain,
                                        initialEntryBuySplitTrans = new List<splitTransaction>()
                                    };
                                    splitTransaction splittrans = new splitTransaction();
                                    splittrans.portion = copyCurrentTransaction.buyAmount / buckets.Last().amount; // portion or weight
                                    splittrans.trans = buckets.Last().trans;
                                    temp.initialEntryBuySplitTrans.Add(splittrans);
                                    realizedTransactions.Add(temp);


                                    buckets.Last().amount -= copyCurrentTransaction.buyAmount;
                                    if (buckets.Last().amount < 0.000001)
                                    {
                                        if (buckets.Last().amount < -0.0001)
                                        {
                                            throw new Exception("Bad selling more");
                                        }
                                        deleteSingleLastZeroAmountInList(buckets);
                                    }
                                    keepGoing = false;
                                }
                                else
                                {
                                    double realizedGain = (copyCurrentTransaction.sellAmount / copyCurrentTransaction.buyAmount - buckets.Last().price) * buckets.Last().amount * -1.0; // inverted so times -1
  
                                    double volume = copyCurrentTransaction.sellAmount * (buckets.Last().amount / copyCurrentTransaction.buyAmount); // check this ratio later, buyamount in dollars
                                    if (buckets.Last().amount > copyCurrentTransaction.buyAmount)
                                    {
                                        throw new Exception("Ratio error");
                                    }

                                    var temp = new realizedTransaction
                                    {
                                        trans = currentTransaction,
                                        amount = buckets.Last().amount,
                                        gain = realizedGain,
                                        sellAmountReceivedUsuallyDollars = volume,// not sure what to put here
                                        initialEntryBuySplitTrans = new List<splitTransaction>()
                                    };

                                    splitTransaction splittrans = new splitTransaction();
                                    splittrans.portion = 1.0; // portion or weight
                                    splittrans.trans = buckets.Last().trans;

                                    temp.initialEntryBuySplitTrans.Add(splittrans);
                                    realizedTransactions.Add(temp);

                                    copyCurrentTransaction.sellAmount *= (copyCurrentTransaction.buyAmount - buckets.Last().amount) / copyCurrentTransaction.buyAmount;
                                    copyCurrentTransaction.buyAmount -= buckets.Last().amount;
                                    buckets.Last().amount = 0;
                                    deleteSingleLastZeroAmountInList(buckets);
                                    if (copyCurrentTransaction.buyAmount > 0.0001 && buckets.Count == 0)
                                    {
                                        throw new Exception("bad not enough in buckets to sell");

                                    }
                                    if (copyCurrentTransaction.buyAmount < 0)
                                    {
                                        Console.Write("error");
                                    }
                                    if (copyCurrentTransaction.buyAmount < 0.000001)
                                    {
                                        keepGoing = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<string> taxyears =
            [
                "2017",
                "2018",
                "2019",
                "2020",
                "2021",
                "2022",
                "2023",
                "2024",
            ];
            double totalAllYears = 0;
            double totalAllYearsVolume = 0;
            foreach (var currentTaxYear in taxyears)
            {
                string doubleFormat = "0.####";
                double totalGain3 = 0;
                double totalVolume = 0;
                if (true)
                {
                    foreach (realizedTransaction rt in realizedTransactions)
                    {
                        if (rt.trans.dateTime.ToString().Contains(currentTaxYear))
                        {
                            totalGain3 += rt.gain;
                            totalVolume += rt.sellAmountReceivedUsuallyDollars;
                            totalAllYears += rt.gain;
                            totalAllYearsVolume += rt.sellAmountReceivedUsuallyDollars;
                        }
                        if (printIndividualTrans && rt.trans.dateTime.Value.Year >= 2016)
                        {
                            Console.WriteLine(rt.trans.sellSymbol.ToString() + " " + rt.trans.dateTime.ToString() + " " + rt.amount.ToString(doubleFormat) + " price " + rt.gain.ToString(doubleFormat));
                        }
                    }
                }
                if (totalGain3 != 0)
                {
                    Console.WriteLine("TOTAL GAIN " + currentTaxYear + "\t" + symbol + "\tprofit:" + (totalGain3 / 1000).ToString("0.###") + "K\tSell Volume:" + (totalVolume / 1000).ToString("0.###") + "K");
                }
                string doubleFormat2 = "0.####";
                //if (symbol.Contains("eth"))
                {
                    foreach (bucket bucket in buckets)
                    {
                        if (bucket.amount > 89999990.05)
                        {
                            //Console.WriteLine(bucket.amount.ToString(doubleFormat) + " bucket " + bucket.price.ToString(doubleFormat));
                        }
                    }
                }
                //foreach (KeyValuePair<string, double> gainInYear in gainsInYear)
                {
                    //Console.WriteLine(gainInYear.Key + " gainInYear " + gainInYear.Value.ToString(doubleFormat));
                }
            }
            Console.WriteLine("TOTAL GAIN YEARS" + "\tprofit:" + (totalAllYears / 1000).ToString("0.###") + "K\tSell Volume:" + (totalAllYearsVolume / 1000).ToString("0.###") + "K");


            //unrealized
            double averageBuyPrice = 0;
            double averageBuyAmount = 0;
            foreach (var b in buckets)
            {
                averageBuyAmount += b.amount;
                averageBuyPrice += b.amount * b.price;

            }
            averageBuyPrice /= averageBuyAmount;

            double unrealizedProfit = (lastExchangePrice - averageBuyPrice) * averageBuyAmount;

            Console.WriteLine("UNREALIZED     " + "\tprofit:" + (unrealizedProfit / 1000).ToString("0.###") + "K AMOUNT TO SELL:" + averageBuyAmount);
            // end unrealized

            if (combineRealizedTransactionOnSameDay)
            {
                Console.WriteLine("combine realized");
                List<realizedTransaction> combineRealizedTrans = new List<realizedTransaction>();
                realizedTransaction? currentCombineTrans = null;
                DateOnly currentDate = new DateOnly(1970,1,1);
                foreach (var g in realizedTransactions)
                {
                    DateOnly thisDate = new DateOnly(g.trans.dateTime.Value.Year, g.trans.dateTime.Value.Month, g.trans.dateTime.Value.Day);
                    
                    if (thisDate != currentDate)
                    {
                        if(currentCombineTrans != null)
                        {
                            combineRealizedTrans.Add(currentCombineTrans);
                        }
                        currentDate = thisDate;
                        currentCombineTrans = g.deepCopy();
                    }
                    else// (thisDate == currentDate)
                    {
                        if(currentCombineTrans == null)
                        {
                            throw new Exception("error null currentombinetran");
                        }

                        currentCombineTrans.gain += g.gain;
                        currentCombineTrans.amount += g.amount;
                        currentCombineTrans.sellAmountReceivedUsuallyDollars += g.sellAmountReceivedUsuallyDollars;
                        currentCombineTrans.trans = null; //fix later todo trevor
                        currentCombineTrans.initialEntryBuySplitTrans = null; //fix later todo trevor
                        //foreach(var initialEntryBuySplitTran in g.initialEntryBuySplitTrans)
                        //{
                        //    currentCombineTrans.initialEntryBuySplitTrans.Add(initialEntryBuySplitTrans);
                        //}
                    }

                }
                if (currentDate !=  new DateOnly())
                {
                    combineRealizedTrans.Add(currentCombineTrans);
                }
                return combineRealizedTrans;
            }

            return realizedTransactions;
        }

        /*private realizedTransaction deepCopy(realizedTransaction a)
        { delete later in class now
            realizedTransaction b = new realizedTransaction();
            b.amount = a.amount;
            b.gain = a.gain;
            b.initialEntryBuySplitTrans = new List<splitTransaction>();
            foreach(var t in a.initialEntryBuySplitTrans){
                var r = new splitTransaction{portion = t.portion, trans = new transaction()};
                r.trans = t.trans.deepCopy();
                b.initialEntryBuySplitTrans.Add(r);
            }
            b.sellAmountReceivedUsuallyDollars = a.sellAmountReceivedUsuallyDollars;
            b.trans = a.trans.deepCopy();
            
            return b;
        }*/

        /*
        public void computeBuysSellsSymbol(List<transaction> transactions, string symbol)
        {
            Console.WriteLine(symbol);

            List<string> taxyear = new List<string>();
            taxyear.Add("2017");
            taxyear.Add("2018");
            taxyear.Add("2019");
            taxyear.Add("2020");
            taxyear.Add("2021");
            taxyear.Add("2022");
            taxyear.Add("2023");
            taxyear.Add("2024");
            double totalAmount = 0;

            foreach (var year in taxyear)
            {
                List<KeyValuePair<string, string>> ValidPairSymbols = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>(symbol, "usd")
                };

                List<total> totalBuy = new List<total>();
                List<total> totalSell = new List<total>();
                //List<total> totalBoth = new List<total>();

                //buys
                foreach (transaction t in transactions)
                {
                    bool includeTransDueToYear = false;
                    if (year == null)
                    {
                        includeTransDueToYear = false;
                    }
                    else
                    {
                        if (t.transDate.ToString().Contains(year))
                        {
                            includeTransDueToYear = true;

                        }
                    }

                    if (includeTransDueToYear)
                    {
                        var specificPair = ValidPairSymbols.FirstOrDefault(s =>
                        s.Key == t.buySymbol.ToLower() && s.Value == t.sellSymbol.ToLower());

                        if (specificPair.Equals(default(KeyValuePair<string, string>)))
                        {
                            //ignore
                            //Console.WriteLine("ignore");
                        }
                        else //match exists
                        {
                            if (totalBuy.FirstOrDefault(t =>
                            t.buySymbol.ToLower() == specificPair.Key &&
                            t.sellSymbol.ToLower() == specificPair.Value) == null)// does entry exist? if not add
                            {
                                total r = new total();
                                r.buySymbol = t.buySymbol;
                                r.sellSymbol = t.sellSymbol;
                                r.Amount = t.buyAmount;
                                r.Average = t.sellAmount / t.buyAmount;
                                totalBuy.Add(r); // add
                            }
                            else
                            {
                                var specificTotal = totalBuy.FirstOrDefault(t =>
                                t.buySymbol.ToLower() == specificPair.Key &&
                                t.sellSymbol.ToLower() == specificPair.Value);

                                if (specificTotal == null)
                                {
                                    throw new Exception("bad");
                                }
                                else
                                {
                                    double totalValue = specificTotal.Average * specificTotal.Amount + t.sellAmount;
                                    specificTotal.Amount += t.buyAmount;
                                    specificTotal.Average = totalValue / specificTotal.Amount;
                                }
                            }
                        }
                    }
                }

                //sells
                foreach (transaction t in transactions)
                {
                    bool includeTransDueToYear = false;
                    if (year == null)
                    {
                        includeTransDueToYear = true;
                    }
                    else
                    {
                        if (t.transDate.ToString().Contains(year))
                        {
                            includeTransDueToYear = true;

                        }
                    }

                    if (includeTransDueToYear)
                    {
                        var specificPair = ValidPairSymbols.FirstOrDefault(s =>
                        s.Key == t.sellSymbol.ToLower() &&
                        s.Value == "usd");
                        if (specificPair.Equals(default(KeyValuePair<string, string>)))
                        {
                            //ignore
                            //Console.WriteLine("ignore");
                        }
                        else
                        {
                            if (totalSell.FirstOrDefault(t =>
                            t.buySymbol.ToLower() == "usd" &&
                            t.sellSymbol.ToLower() == specificPair.Key) == null)// does entry exist? if not add
                            {
                                total r = new total();
                                r.buySymbol = t.buySymbol;
                                r.sellSymbol = t.sellSymbol;
                                r.Amount = t.sellAmount; // sold something like btc
                                r.Average = t.buyAmount / t.sellAmount; // buy something like usd
                                totalSell.Add(r); // add
                            }
                            else
                            {
                                var specificTotal = totalSell.FirstOrDefault(t =>
                                t.buySymbol.ToLower() == "usd" &&
                                t.sellSymbol.ToLower() == specificPair.Key);

                                if (specificTotal == null)
                                {
                                    throw new Exception("bad");
                                }
                                else
                                {
                                    double totalValue = specificTotal.Average * specificTotal.Amount + t.buyAmount;
                                    specificTotal.Amount += t.sellAmount;
                                    specificTotal.Average = totalValue / specificTotal.Amount;
                                }
                            }

                            //total u = totalsSell.First(s => s.buySymbol == t.buySymbol && s.sellSymbol == t.sellSymbol);

                            //if (u is null)
                            {
                            //    throw new Exception("bad");
                            }
                            //else
                            {
                                // t is current transaction
                             //   double totalValue = u.Average * u.Amount + t.buyAmount;
                               // u.Amount += t.sellAmount;
                                // update the average
                               // u.Average = totalValue / u.Amount;
                            }

                        }
                    }
                }
                if (totalBuy.Count != 1 && totalSell.Count != 1)
                {//
                 // throw new Exception("ds");

                }
                else if (totalSell.Count != 1)
                {
                    totalAmount += totalBuy.First().Amount;
                }

                else if (totalBuy.Count != 1)
                {
                    totalAmount -= totalSell.First().Amount;
                }
                else
                {
                    totalAmount += totalBuy.First().Amount - totalSell.First().Amount;
                }

                //totals.Sort();
                //totalsSell.Sort();

                CultureInfo culture;
                culture = CultureInfo.CreateSpecificCulture("en-US");

                List<string> seenSymbol = new List<string?>();

                string doubleFormat = "0.####";

                foreach (total mysell in totalSell)
                {
                    if (mysell.sellSymbol == "eth")
                    {
                        //Console.WriteLine(mysell.sellSymbol + " sell amt:" + mysell.Amount + " avg:" + mysell.Average);

                    }
                }

                foreach (total mybuy in totalBuy)
                {
                    if (mybuy.buySymbol == "eth")
                    {
                        //Console.WriteLine(mybuy.sellSymbol + " buy amt:" + mybuy.Amount + " avg:" + mybuy.Average);

                    }
                }
                Console.WriteLine(year + " totalAmount " + totalAmount);
            }

        }*/


        public void summerizeRealized(string year, List<realizedTransaction> realizedTransactions)
        {
            //throw new Exception("bad should not be using first");
            Console.WriteLine("year");
            Console.WriteLine(year);
            Console.WriteLine(year);


            double totalRealizedGainCoinbase = 0;
            double totalRealizedGainBinance = 0;
            double totalRealizedGainKraken = 0;

            double realizedGainCoinbaseVarious = 0;
            double realizedGainBinanceVarious = 0;
            double realizedGainKrakenVarious = 0;


            double totalRealizedLossCoinbase = 0;
            double totalRealizedLossBinance = 0;
            double totalRealizedLossKraken = 0;

            double realizedLossCoinbaseVarious = 0;
            double realizedLossBinanceVarious = 0;
            double realizedLossKrakenVarious = 0;

            double totalCostBasisUSD_Coinbase = 0;
            double totalCostBasisUSD_Binance = 0;
            double totalCostBasisUSD_Kraken = 0;


            double totalSellVolumeUSD_Coinbase = 0;
            double totalSellVolumeUSD_Binance = 0;
            double totalSellVolumeUSD_Kraken = 0;


            double listedCostBasisUSD_Coinbase = 0;
            double listedCostBasisUSD_Binance = 0;
            double listedCostBasisUSD_Kraken = 0;

            double listedGain = 0;
            double listedLoss = 0;

            string doubleFormat = "0.####";
            string doubleFormatdollar = "0.##";

            int count = 0;
            foreach (realizedTransaction realized in realizedTransactions)
            {
                if (count++ == 5000)
                {
                    //Console.WriteLine("fsf");
                    int y = 0;

                }
                if (realized.trans.dateTime.ToString().Contains(year.ToString()))
                {
                    string sellExchangeAbriviation = "XX";
                    if (realized.trans.exchangeRec.ToLower().Contains("binance"))
                    {
                        sellExchangeAbriviation = "Binance:     ";

                        if (realized.gain > 100000)
                        {
                            Console.WriteLine("sd");
                        }
                        if (realized.gain > 0)
                        {
                            realizedGainBinanceVarious += realized.gain;
                            totalRealizedGainBinance += realized.gain;
                        }
                        else
                        {
                            realizedLossBinanceVarious += realized.gain;
                            totalRealizedLossBinance += realized.gain;
                        }
                        double avgsell = realized.trans.buyAmount / realized.trans.sellAmount;
                        double avgbuy = realized.initialEntryBuySplitTrans.First().trans.sellAmount / realized.initialEntryBuySplitTrans.First().trans.buyAmount;
                        totalSellVolumeUSD_Binance += avgsell * realized.amount;
                        totalCostBasisUSD_Binance += avgbuy * realized.amount;
                        //totalReceivedUSD_Binance += realized.trans.buyAmount;
                    }
                    if (realized.trans.exchangeRec.ToLower().Contains("coinbase"))
                    {
                        sellExchangeAbriviation = "Coinbase:    ";

                        if (realized.gain > 0)
                        {
                            realizedGainCoinbaseVarious += realized.gain;
                            totalRealizedGainCoinbase += realized.gain;
                        }
                        else
                        {
                            realizedLossCoinbaseVarious += realized.gain;
                            totalRealizedLossCoinbase += realized.gain;
                        }
                        double avgsell = realized.trans.buyAmount / realized.trans.sellAmount;
                        double avgbuy = realized.initialEntryBuySplitTrans.First().trans.sellAmount / realized.initialEntryBuySplitTrans.First().trans.buyAmount;
                        //totalReceivedUSD_Coinbase += realized.trans.buyAmount;
                        totalSellVolumeUSD_Coinbase += avgsell * realized.amount;
                        totalCostBasisUSD_Coinbase += avgbuy * realized.amount;
                    }
                    if (realized.trans.exchangeRec.ToLower().Contains("kraken"))
                    {
                        sellExchangeAbriviation = "Kraken: ";

                        if (realized.gain > 0)
                        {
                            realizedGainKrakenVarious += realized.gain;
                            totalRealizedGainKraken += realized.gain;
                        }
                        else
                        {
                            realizedLossKrakenVarious += realized.gain;
                            totalRealizedLossKraken += realized.gain;
                        }
                        //totalReceivedUSD_Kraken += realized.trans.buyAmount;

                        double avgsell = realized.trans.buyAmount / realized.trans.sellAmount;
                        double avgbuy = realized.initialEntryBuySplitTrans.First().trans.sellAmount / realized.initialEntryBuySplitTrans.First().trans.buyAmount;
                        //totalCostBasisUSD_Kraken += avgsell * realized.amount + avgbuy * realized.amount;

                        totalSellVolumeUSD_Kraken += avgsell * realized.amount;
                        totalCostBasisUSD_Kraken += avgbuy * realized.amount;
                        //if (realized.trans.sellAmount < 0)
                    }
                    //Console.WriteLine(realized.trans.sellSymbol);
                    //Console.WriteLine(realized.trans.);
                    if (((realized.gain > 200 || realized.gain < -500) && !realized.trans.exchangeRec.ToLower().Contains("binance")) ||
                        ((realized.gain > 500 || realized.gain < -700) && realized.trans.exchangeRec.ToLower().Contains("binance"))
                        //                       (realized.trans.sellAmount > 0.33)
                        )
                    {
                        string longterm = "";
                        TimeSpan diff = realized.trans.dateTime.Value.Subtract(realized.initialEntryBuySplitTrans.First().trans.dateTime.Value);
                        var days = diff.TotalDays;
                        if (days > 365)
                        {
                            longterm = "  LT";
                        }
                        double avgsell = realized.trans.buyAmount / realized.trans.sellAmount;
                        double avgbuy = realized.initialEntryBuySplitTrans.First().trans.sellAmount / realized.initialEntryBuySplitTrans.First().trans.buyAmount;
                        Console.WriteLine(sellExchangeAbriviation + "\t" + realized.gain.ToString(doubleFormatdollar) + "  \t   basis " + realized.trans.sellSymbol + ":" + realized.amount.ToString(doubleFormat) + "\t" +
                            realized.trans.dateTime.Value.ToString("yyyy-MM-dd") + "\t" + realized.initialEntryBuySplitTrans.First().trans.dateTime.Value.ToString("yyyy-MM-dd") + " " +
                            //" buy:" + realized.trans.buySymbol + 
                            "\tcost basis:" + (avgsell * realized.amount).ToString(doubleFormatdollar) + " " + (avgbuy * realized.amount).ToString(doubleFormatdollar) + " " +
                            //" total sell:" + realized.trans.sellAmount.ToString(doubleFormat) + " avg:" + avgsell.ToString(doubleFormat) +
                            // " buy:" + realized.initialEntryBuyTrans.First().buyAmount.ToString(doubleFormat) + " avg:" + avgbuy.ToString(doubleFormat) +
                            longterm
                            //+ "comb" + realized.initialEntryBuyTrans.First().combinedCount + " " + realized.trans.combinedCount
                            //+ " sanity:" + (realized.amount * (avgsell - avgbuy)).ToString(doubleFormat)
                            );


                        if (realized.gain > 0)
                        {
                            if (realized.trans.exchangeRec.ToLower().Contains("binance"))
                            {
                                realizedGainBinanceVarious -= realized.gain;
                                listedCostBasisUSD_Binance += avgbuy * realized.amount;
                                listedGain += realized.gain;
                            }
                            if (realized.trans.exchangeRec.ToLower().Contains("coinbase"))
                            {
                                realizedGainCoinbaseVarious -= realized.gain;
                                listedCostBasisUSD_Coinbase += avgbuy * realized.amount;
                                listedGain += realized.gain;
                            }
                            if (realized.trans.exchangeRec.ToLower().Contains("kraken"))
                            {
                                realizedGainKrakenVarious -= realized.gain;
                                listedCostBasisUSD_Kraken += avgbuy * realized.amount;
                                listedGain += realized.gain;
                            }
                        }
                        if (realized.gain < 0)
                        {
                            if (realized.trans.exchangeRec.ToLower().Contains("binance"))
                            {
                                realizedLossBinanceVarious -= realized.gain;
                                listedCostBasisUSD_Binance += avgsell * realized.amount + avgbuy * realized.amount;
                                listedLoss += realized.gain;
                            }
                            if (realized.trans.exchangeRec.ToLower().Contains("coinbase"))
                            {
                                realizedLossCoinbaseVarious -= realized.gain;
                                listedCostBasisUSD_Coinbase += avgsell * realized.amount + avgbuy * realized.amount;
                                listedLoss += realized.gain;
                            }
                            if (realized.trans.exchangeRec.ToLower().Contains("kraken"))
                            {
                                realizedLossKrakenVarious -= realized.gain;
                                listedCostBasisUSD_Kraken += avgsell * realized.amount + avgbuy * realized.amount;
                                listedLoss += realized.gain;
                            }
                        }

                    }

                }

            }
            Console.WriteLine("realizedBU:" + realizedLossBinanceVarious.ToString(doubleFormat) + " " + realizedGainBinanceVarious.ToString(doubleFormat) + " " + (realizedLossBinanceVarious + realizedGainBinanceVarious).ToString(doubleFormat));
            Console.WriteLine("realizedCB:" + realizedLossCoinbaseVarious.ToString(doubleFormat) + " " + realizedGainCoinbaseVarious.ToString(doubleFormat) + " " + (realizedLossCoinbaseVarious + realizedGainCoinbaseVarious).ToString(doubleFormat));
            Console.WriteLine("realizedKR:" + realizedLossKrakenVarious.ToString(doubleFormat) + " " + realizedGainKrakenVarious.ToString(doubleFormat) + " " + (realizedLossKrakenVarious + realizedGainKrakenVarious).ToString(doubleFormat));


            //Console.WriteLine("ReceivedUSD_BU:" + listedCostBasisUSD_Binance.ToString(doubleFormat));
            //Console.WriteLine("ReceivedUSD_CB:" + listedCostBasisUSD_Coinbase.ToString(doubleFormat));
            //Console.WriteLine("ReceivedUSD_KK:" + listedCostBasisUSD_Kraken.ToString(doubleFormat));


            Console.WriteLine("--");
            Console.WriteLine("Total Volume BU: " + (totalSellVolumeUSD_Binance).ToString(doubleFormat));
            Console.WriteLine("Total Volume CP: " + (totalSellVolumeUSD_Coinbase).ToString(doubleFormat));
            Console.WriteLine("Total Volume KR: " + (totalSellVolumeUSD_Kraken).ToString(doubleFormat));
            Console.WriteLine("--");


            Console.WriteLine("--");
            Console.WriteLine("Total Cost basis BU: " + (totalCostBasisUSD_Binance).ToString(doubleFormat));
            Console.WriteLine("Total Cost basis CP: " + (totalCostBasisUSD_Coinbase).ToString(doubleFormat));
            Console.WriteLine("Total Cost basis KR: " + (totalCostBasisUSD_Kraken).ToString(doubleFormat));
            Console.WriteLine("--");

            Console.WriteLine("--");
            //Console.WriteLine("Volume: " + (totalCostBasisUSD_Binance  -listedCostBasisUSD_Binance).ToString(doubleFormat));
            //Console.WriteLine("Volume: " + (totalCostBasisUSD_Coinbase -listedCostBasisUSD_Coinbase).ToString(doubleFormat));
            //Console.WriteLine("Volume: " + (totalCostBasisUSD_Kraken   -listedCostBasisUSD_Kraken).ToString(doubleFormat));
            Console.WriteLine("Listed Gain: " + listedGain.ToString(doubleFormat));
            Console.WriteLine("Listed Loss: " + listedLoss.ToString(doubleFormat));
            Console.WriteLine("Listed Total: " + (listedGain + listedLoss).ToString(doubleFormat));
            Console.WriteLine("--");

            //Console.WriteLine("totalReceivedUSD_BU:" + totalCostBasisUSD_Binance.ToString(doubleFormat));
            //Console.WriteLine("totalReceivedUSD_CB:" + totalCostBasisUSD_Coinbase.ToString(doubleFormat));
            //Console.WriteLine("totalReceivedUSD_KK:" + totalCostBasisUSD_Kraken.ToString(doubleFormat));

            Console.WriteLine("realizedBU gain loss total:" + totalRealizedGainBinance.ToString(doubleFormat) + " " + totalRealizedLossBinance.ToString(doubleFormat) + " " + (totalRealizedGainBinance + totalRealizedLossBinance).ToString(doubleFormat));
            Console.WriteLine("realizedCB gain loss total:" + totalRealizedGainCoinbase.ToString(doubleFormat) + " " + totalRealizedLossCoinbase.ToString(doubleFormat) + " " + (totalRealizedGainCoinbase + totalRealizedLossCoinbase).ToString(doubleFormat));
            Console.WriteLine("realizedKR gain loss total:" + totalRealizedGainKraken.ToString(doubleFormat) + " " + totalRealizedLossKraken.ToString(doubleFormat) + " " + (totalRealizedGainKraken + totalRealizedLossKraken).ToString(doubleFormat));


        }

        public void convertBitcoinPairToTwoTransInUSD(List<transaction> transactions, double averageFactorPercentZeroToOne = 0.5)
        {
            if (averageFactorPercentZeroToOne > 1 || averageFactorPercentZeroToOne < 0)
            {
                throw new Exception("bad averageFactorPercentZeroToOne");
            }

            List<transaction> transToAdd = new List<transaction>();
            Dictionary<string, int> skippedPairs = new Dictionary<string, int>();

            foreach (transaction t in transactions)
            {
                /*if (t.dateTime.Value.Date.Year == 2022 &&
                    t.dateTime.Value.Date.Month == 9 &&
                    t.dateTime.Value.Date.Day == 19
                    )
                {
                    //Console.WriteLine("sd");
                }
                if (t.sellAmount > 0.037083 && t.sellAmount < 0.0370831)
                {
                    //Console.WriteLine("sd");
                }
                if (t.buySymbol.Contains("usd") || t.sellSymbol.Contains("usd"))
                {
                    //Console.WriteLine("skip");
                }*/
                if (t.buySymbol.ToLower() == ("btc") || t.sellSymbol.ToLower() == ("btc"))// only have historic info for btc
                {
                    double highPrice = 0;
                    double lowPrice = 0;
                    bool found = false;
                    foreach (historicDayPriceUSD hp in historicPrices)
                    {
                        if (t.dateTime.Value.Date.Year == hp.dateOnly.Year &&
                            t.dateTime.Value.Date.Month == hp.dateOnly.Month &&
                            t.dateTime.Value.Date.Day == hp.dateOnly.Day
                            )
                        {
                            found = true;
                            highPrice = hp.high;
                            lowPrice = hp.low;
                        }
                    }
                    if (found)
                    {
                        if (t.sellSymbol.ToLower() == "btc")
                        {
                            transaction newTrans = new transaction();
                            newTrans.dateTime = t.dateTime.Value;
                            newTrans.exchangeRec = t.exchangeRec;
                            newTrans.exchangeSent = t.exchangeSent;
                            newTrans.sellAmount = (highPrice * (1.0 - averageFactorPercentZeroToOne) +
                                                   lowPrice * averageFactorPercentZeroToOne) * t.sellAmount;
                            newTrans.sellSymbol = "usd";
                            newTrans.buySymbol = t.buySymbol;
                            newTrans.buyAmount = t.buyAmount;
                            transToAdd.Add(newTrans);

                            t.buySymbol = "usd";
                            t.buyAmount = (lowPrice * (1.0 - averageFactorPercentZeroToOne) +
                                           highPrice * averageFactorPercentZeroToOne) * t.sellAmount;
                        }
                        else if (t.buySymbol.ToLower() == "btc")
                        {
                            transaction newTrans = new transaction();
                            newTrans.dateTime = t.dateTime.Value;
                            newTrans.exchangeRec = t.exchangeRec;
                            newTrans.exchangeSent = t.exchangeSent;
                            newTrans.sellAmount = t.sellAmount;
                            newTrans.sellSymbol = t.sellSymbol;
                            newTrans.buySymbol = "usd";
                            newTrans.buyAmount = (lowPrice * (1.0 - averageFactorPercentZeroToOne) +
                                                  highPrice * averageFactorPercentZeroToOne) * t.buyAmount;
                            transToAdd.Add(newTrans);

                            t.sellSymbol = "usd";
                            t.sellAmount = (highPrice * (1.0 - averageFactorPercentZeroToOne) +
                                            lowPrice * averageFactorPercentZeroToOne) * t.buyAmount;
                        }
                        else
                        {
                            throw new Exception("Expected BTC");
                        }
                    }
                }
                else
                {
                    if (skippedPairs.ContainsKey(t.buySymbol + t.sellSymbol))
                    {
                        skippedPairs[(t.buySymbol + t.sellSymbol)]++;
                    }
                    else
                    {
                        skippedPairs[(t.buySymbol + t.sellSymbol)] = 1;
                    }
                }
            }
            foreach (var t in transToAdd)
            {
                transactions.Add(t);
            }
            transactions.Sort((x, y) => x.dateTime.Value.CompareTo(y.dateTime.Value));

            foreach (var d in skippedPairs)
            {
                if(d.Key.Contains("usd"))
                {
                    Console.WriteLine(d.Key + " " + d.Value.ToString());
                }
                else
                {
                    Console.WriteLine(d.Key + " " + d.Value.ToString());
                    throw new Exception("bad averageFactorPercentZeroToOne");
                }
            }
        }

        public List<List<string>> realizedTransToString(List<realizedTransaction> realizedTrans)
        {
            List<List<string>> ret = new List<List<string>>();

            string doubleFormat = "0.#####";
            string doubleFormatMoney = "0.####";
            foreach (realizedTransaction f in realizedTrans)
            {
                double basis = 0;
                double amt = 0;
                double avgbuy = 0;
                double totalcost = 0;
                int count = 0;
                foreach (var t in f.initialEntryBuySplitTrans)
                {
                    count++;
                    basis += t.trans.sellAmount * t.portion;
                    if(!(t.trans.sellSymbol.ToLower().Contains("usd"))) {
                        throw new Exception("bad");
                    }


                    amt += t.trans.buyAmount * t.portion;
                }
                avgbuy = basis / amt;

                List<string> outline = new List<string>();
                outline.Add(f.trans.dateTime.ToString());
                //outline.Add(f.trans.sellSymbol + "amt:");
                outline.Add("" + f.trans.sellAmount.ToString(doubleFormat));
                //outline.Add(f.trans.exchangeRec);
                outline.Add("" + f.gain.ToString(doubleFormatMoney));
                //outline.Add("$" + f.sellAmountReceivedUsuallyDollars.ToString());
                outline.Add("$" + basis.ToString(doubleFormatMoney));
                outline.Add("avg" + avgbuy.ToString(doubleFormatMoney));
                outline.Add("avs" + (f.trans.buyAmount/f.trans.sellAmount).ToString(doubleFormatMoney));
                //outline.Add(count.ToString());
                //outline.Add(f.gain.ToString());
                //outline.Add(f.gain.ToString());

                ret.Add(outline);
            }
            return ret;
        }

        public string padStringFormat(double f)
        {
            string doubleFormat = "0.00";

            string ret = " ";
            int g = f.ToString(doubleFormat).Length;
            bool negative = false;
            if (f < 0)
            {
                negative = true;
                f = f * -1;
            }

            if (f < 10)
            {
                ret += " ";
            }
            if (f < 100)
            {
                ret += " ";
            }
            if (f < 1000)
            {
                ret += " ";
            }
            if (f < 10000)
            {
                ret += " ";
            }
            if (negative)
            {
                ret = ret.Substring(1);
                ret += "-" + f.ToString(doubleFormat);
            }
            else
            {
                ret += f.ToString(doubleFormat);

            }

            return ret;
        }

        
        public void printListListString(int stringLimit, string spacingString, List<List<string>> data)
        {
            stringLimit = 12;
            foreach (var d in data)
            {
                foreach(string s in d)
                {
                    string h = s;
                    if(s.Length > stringLimit)
                    {
                        h = s.Substring(0, stringLimit);
                    }
                    Console.Write(h);
                    string filler = "";
                    for(int i=s.Length; i<stringLimit; i++)
                    {
                        filler += " ";
                    }
                    Console.Write(filler);
                    Console.Write(spacingString);
                }
                Console.Write("\n");
            }
        }

        public static int Go()
        {
    
            Console.WriteLine("library2");
            return 4; 
        }

    }

}
