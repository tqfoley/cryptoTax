//using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;


namespace gaintaxlibrary
{
    
    public class Transaction
    {
        public DateTime dateTime;
        public int combinedCount = 0;
        public DateTime? optionalSecondTansDate;

        public string buySymbol;
        public double buyAmount;
        public string exchangeRec;

        public string sellSymbol;
        public double sellAmount;
        public string exchangeSent;

        public string? feeSymbol;
        public double feeAmount;
        public string? altFeeSymbol;
        public double? altFeeAmount;

        // Constructor
        public Transaction()
        {
            this.buySymbol = "";
            this.sellSymbol = "";
            dateTime = new DateTime(2000,1,1);
            this.exchangeRec = "";
            this.exchangeSent = "";
        }

        public Transaction deepCopy()
        {
            Transaction ret = new Transaction();
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

    public class Transfer
    {
        public DateTime? dateTime;
        public bool receive;
        public string? symbol;
        public double amount;
        public string? toAccount;
        public string? fromAccount;
    }

    public class Bucket // buckets for selling, can reorder for FIFO, LIFO, MaxCost
    {
        public string? symbol;
        public double amount;
        public double price;
        public Transaction? trans;
    }

    public class splitTransaction
    {
        public Transaction trans;
        public double portion; // aka weight, when selling a buy might need to by split into multiple portions

        public splitTransaction()
        {
            trans = new Transaction();
        }
    }

    public class RealizedTransaction
    {
        public double gain;
        public Transaction? trans;
        public double amount;
        public double sellAmountReceivedUsuallyDollars; // usually in dollars
        public List<splitTransaction> initialEntryBuySplitTrans;

        public RealizedTransaction()
        {
            initialEntryBuySplitTrans = new List<splitTransaction>();
        }

        public RealizedTransaction deepCopy()
        {  
            RealizedTransaction b = new RealizedTransaction();
            b.amount = this.amount;
            b.gain = this.gain;
            b.initialEntryBuySplitTrans = new List<splitTransaction>();
            if (this != null)
            {
                if (this.initialEntryBuySplitTrans != null && this.trans != null)
                {
                    foreach (var t in this.initialEntryBuySplitTrans)
                    {
                        var r = new splitTransaction { portion = t.portion, trans = new Transaction() };
                        r.trans = t.trans.deepCopy();
                        b.initialEntryBuySplitTrans.Add(r);
                    }
                    b.sellAmountReceivedUsuallyDollars = this.sellAmountReceivedUsuallyDollars;
                    b.trans = this.trans.deepCopy();
                }
            }
            else
            {
                throw new Exception("Null error, expected not null.");
            }
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
        public class Total
        {
            public string? buySymbol;
            public string? sellSymbol;
            public double Amount;
            public double Average;

        }

        public List<Transaction> transactionsOriginal = new List<Transaction>();
        public List<Transfer> transfers1 = new List<Transfer>();
        public List<historicDayPriceUSD> historicPrices = new List<historicDayPriceUSD>();

        public bool isCloseToCombine(double a, double b, double combineSensetivity)
        {
            if (a * (1.0 + combineSensetivity) > b && a * (1.0 - combineSensetivity) < b)
                return true;
            return false;
        }

        public bool IsTransactionMarkedDeleted(Transaction t)
        {
            if(t==null || t.buySymbol == null || t.sellSymbol == null)
            {
                throw new Exception("null error");
            }
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
        
        public void MarkTransactionInsignificant(Transaction t)
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

        public void combineTwoTransactions(Transaction a, Transaction b, int timeSpanHours, List<Transaction> transactionsCheck, double combineSensetivity)
        {

            //double originalBuyAmount = 0;
            //double originalSellAmount = 0;


            if (a.buySymbol == b.buySymbol &&
                a.sellSymbol == b.sellSymbol &&
                a.dateTime.Add(new TimeSpan(timeSpanHours, 0, 0)) > b.dateTime &&
                a.dateTime.Subtract(new TimeSpan(timeSpanHours, 0, 0)) < b.dateTime &&
                isCloseToCombine(a.sellAmount / a.buyAmount,
                    b.sellAmount / b.buyAmount, combineSensetivity) &&
                a.dateTime.Year == b.dateTime.Year
                )
            {
                

                //originalBuyAmount = a.buyAmount;
                //originalSellAmount = a.sellAmount;
                a.buyAmount += b.buyAmount;
                a.sellAmount += b.sellAmount;
                //a.exchangeRec = a.exchangeRec;
                //a.exchangeSent = a.exchangeSent;

                if(a.exchangeRec == null || a.exchangeSent == null)
                {
                    throw new Exception("null error");
                }
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
                    a.optionalSecondTansDate = b.dateTime;
                }
                else
                {
                    if (a.optionalSecondTansDate.Value < b.dateTime)
                    {
                        a.optionalSecondTansDate = b.dateTime;
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
                /*if(a.dateTime == null || b.dateTime == null)
                {
                    throw new Exception("null error");
                }*/
                if (a.dateTime.Year != b.dateTime.Year)
                {
                    throw new Exception("bad date");

                }
                //Console.WriteLine("bad trying to combine but not purchased near same amount");
            }
        }

        public void useAlternativeDateForSellSinceAfterBuy_MODIFIES_transactions(List<Transaction> transactions)
        {
            foreach(var t in transactions)
            {
                if(t.buySymbol.Contains("usd")) // this is a sell
                {
                    if(t.optionalSecondTansDate != null)
                    {
                        DateTime saveDateTime = t.optionalSecondTansDate.Value;

                    Console.WriteLine("sells should be after buys but still in the same year");
                    if(t.dateTime < t.optionalSecondTansDate)
                    {
                        if(t.dateTime.Year == t.optionalSecondTansDate.Value.Year)
                        {
                            t.dateTime = t.optionalSecondTansDate.Value;
                            t.optionalSecondTansDate = saveDateTime;
                        }
                        else{
                            t.dateTime = new DateTime(t.dateTime.Year, 12,31).AddSeconds(-5); // 5 seconds before midnight
                            t.optionalSecondTansDate = saveDateTime;
                        }
                    }
                    }
                }
            }
        }

        public int combineTransactionsInHourLongWindow_MODIFIES_transactions(List<Transaction> transactions, int timeSpanHours, bool limitOnlyUSDTransactions = true, double combineSensitivityPercentDiffOver100 = 0.05)
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
                Transaction first = transactions.ElementAt(i);

                //if (first.sellSymbol.Contains("bnb") && first.transDate.ToString().Contains("5/6/2024") && 
                //    first.transDate.ToString().Contains("8:15")
                //    )
                //{//5 / 6 / 2024 8:15:55 AM btc bnb 0.00051778 0.056 ratio 0.009246071428571428)
                //first.sellSymbol = "bnb";
                //}
                if(first.buySymbol == null || first.sellSymbol == null)
                {
                    throw new Exception("null error");
                }
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
                        if (first.dateTime.Year == 2024)
                        {
                            //Console.WriteLine("sdf");
                        }
                            //var exchangeone = transactions.ElementAt(j).exchangeRec;
                            //var exchangetwo = transactions.ElementAt(j).exchangeSent;
                            
                        if (first.buySymbol == transactions.ElementAt(j).buySymbol &&
                           first.sellSymbol.ToLower() == transactions.ElementAt(j).sellSymbol.ToLower() &&
                           first.dateTime.Add(new TimeSpan(timeSpanHours, 0, 0)) > transactions.ElementAt(j).dateTime &&
                           first.dateTime.Subtract(new TimeSpan(timeSpanHours, 0, 0)) < transactions.ElementAt(j).dateTime &&
                           first.buyAmount > 0.00000007 &&
                           //first.exchangeRec == transactions.ElementAt(j).exchangeRec &&
                           //first.exchangeSent == transactions.ElementAt(j).exchangeSent &&
                           first.dateTime.Year == transactions.ElementAt(j).dateTime.Year 
                           
                           )
                        {
                            if (isCloseToCombine(first.sellAmount / first.buyAmount,
                                transactions.ElementAt(j).sellAmount / transactions.ElementAt(j).buyAmount, combineSensitivityPercentDiffOver100) ||
                                isCloseToCombine(first.buyAmount / first.sellAmount,
                                transactions.ElementAt(j).buyAmount / transactions.ElementAt(j).sellAmount, combineSensitivityPercentDiffOver100)
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
                                    combineTwoTransactions(first, transactions.ElementAt(j), timeSpanHours, transactions, combineSensitivityPercentDiffOver100);
                                    combined++;
                                    //currentcombined++;
                                }
                            }
                            else
                            {
                                if (combineSensitivityPercentDiffOver100 < 0.15)
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

            List<Transaction> ignoreInsignificantTrans = new List<Transaction>();
            foreach (Transaction f in transactions)
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
        public void deleteSingleLastZeroAmountInList(List<Bucket> t)
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

        public List<RealizedTransaction> computeGains(out List<Bucket> buckets, bool combineRealizedTransactionOnSameDay, string symbol, string mode, List<Transaction> transactions, DateTime? endDate = null, bool printIndividualTrans = false)
        {
            /*if(combineRealizedTransactionOnSameDay)
            {
                throw new Exception("This will combine tranasctions on different exchanges");
                throw new Exception("it should not do this");
            }*/

            Console.WriteLine("gains for " + symbol);
            transactions = transactions.OrderBy(x => x.dateTime).ToList();
            
            List<RealizedTransaction> RealizedTransactions = new List<RealizedTransaction>();
            
            if (endDate == null)
            {
                Console.WriteLine("END DATE Dec 31 2024!");
                endDate = new DateTime(2024, 12, 31);
            }
            buckets = new List<Bucket>();
            
            List<KeyValuePair<string, string>> ValidPairSymbols = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>(symbol, "usd")
            };

            DateOnly prevDate = new DateOnly(1970, 1, 1);
            double lastExchangePrice = 0;

            int countTrans = 0;
            bool Inverted = false; // used for selling first
            bool firstTransaction = false;  // set inverted based on first Transaction

            var filteredTrans = transactions.Where(x => x.buySymbol.Contains(symbol) || x.sellSymbol.Contains(symbol));
            double sellAmount = transactions.Where(x => x.sellSymbol.Contains(symbol)).Sum(x => x.sellAmount);
            double buyAmount = transactions.Where(x => x.buySymbol.Contains(symbol)).Sum(x => x.buyAmount);

            foreach (Transaction currentTransaction in filteredTrans)
            {
                if(currentTransaction.buySymbol == null || currentTransaction.sellSymbol == null)
                {
                    throw new Exception("null error");
                }
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
                DateOnly currentDate = new DateOnly(currentTransaction.dateTime.Date.Year,
                    currentTransaction.dateTime.Date.Month,
                    currentTransaction.dateTime.Date.Day);

                if (currentDate < prevDate)
                {
                    Console.WriteLine("Exception transactions not ordered properly");
                    throw new Exception("bad");
                }
                prevDate = new DateOnly(currentTransaction.dateTime.Date.Year,
                    currentTransaction.dateTime.Date.Month,
                    currentTransaction.dateTime.Date.Day);

                Transaction? prevTrans = null;
                if (currentTransaction.dateTime < endDate.Value)
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
                                buckets.Add(new Bucket
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
                                buckets.Add(new Bucket
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
                            throw new Exception("unexpected last Bucket");
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
                                //rearrange List last Transaction At Back

                                buckets = buckets.OrderBy(x => x.trans?.dateTime).ToList();
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
                        Transaction copyCurrentTransaction = currentTransaction.deepCopy();
                        bool keepGoing = true;
                        while (keepGoing)//need to loop untill all the sell amount is matched with enough buckets to value amount of sell
                        {
                            if (!Inverted)
                            {
                                if (buckets.Last().amount > copyCurrentTransaction.sellAmount)
                                {
                                    double realizedGain = (copyCurrentTransaction.buyAmount / copyCurrentTransaction.sellAmount - buckets.Last().price) * copyCurrentTransaction.sellAmount;
                                    //Transaction t = currentTransaction;
                                    //double a = copyCurrentTransaction.sellAmount;
                                    //double g = realizedGain;
                                    lastExchangePrice = copyCurrentTransaction.buyAmount / copyCurrentTransaction.sellAmount;

                                    var newRealizedGain = new RealizedTransaction
                                    {
                                        trans = currentTransaction,  // sell trans
                                        amount = copyCurrentTransaction.sellAmount,
                                        sellAmountReceivedUsuallyDollars = copyCurrentTransaction.buyAmount,
                                        gain = realizedGain,
                                        initialEntryBuySplitTrans = new List<splitTransaction>()
                                    };
                                    splitTransaction splittrans = new splitTransaction();
                                    splittrans.portion = copyCurrentTransaction.sellAmount / buckets.Last().amount; // portion or weight
                                                                                                                    // Get the last bucket safely
                                    var lastBucket = buckets?.LastOrDefault();

                                    if (lastBucket == null || lastBucket.trans == null)
                                    {
                                        throw new Exception("bad null: Either the list is empty or trans is null in the last bucket");
                                    }
                                    else
                                    {
                                        splittrans.trans = lastBucket.trans;
                                    }

                                    newRealizedGain.initialEntryBuySplitTrans.Add(splittrans);
                                    RealizedTransactions.Add(newRealizedGain);

                                    lastBucket.amount -= copyCurrentTransaction.sellAmount;
                                    if (lastBucket.amount < 0.000001)
                                    {
                                        if (lastBucket.amount < -0.0001)
                                        {
                                            throw new Exception("Bad selling more than exists");
                                        }
                                        if(buckets == null)
                                        {
                                            throw new Exception("bad");
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

                                    var temp = new RealizedTransaction
                                    {
                                        trans = currentTransaction,
                                        amount = buckets.Last().amount,
                                        gain = realizedGain,
                                        sellAmountReceivedUsuallyDollars = volume,
                                        initialEntryBuySplitTrans = new List<splitTransaction>()
                                    };

                                    splitTransaction splittrans = new splitTransaction();
                                    splittrans.portion = 1.0; // portion or weight

                                    var lastBucket = buckets.LastOrDefault();
                                    if (lastBucket == null || lastBucket.trans == null)
                                    {
                                        splittrans.trans = new Transaction(); // or assign a default value for trans if necessary
                                        throw new Exception("bad should not use new Transaction");
                                    }
                                    else
                                    {
                                        splittrans.trans = lastBucket.trans;
                                    }



                                    temp.initialEntryBuySplitTrans.Add(splittrans);
                                    RealizedTransactions.Add(temp);

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

                                    var temp = new RealizedTransaction
                                    {
                                        trans = currentTransaction,  // sell trans
                                        amount = copyCurrentTransaction.buyAmount,//  fix aug 28th   was dividing sell amount by  buckets.Last().amount),
                                        sellAmountReceivedUsuallyDollars = copyCurrentTransaction.sellAmount,
                                        gain = realizedGain,
                                        initialEntryBuySplitTrans = new List<splitTransaction>()
                                    };
                                    splitTransaction splittrans = new splitTransaction();
                                    splittrans.portion = copyCurrentTransaction.buyAmount / buckets.Last().amount; // portion or weight
                                    
                                    //splittrans.trans = buckets.Last().trans;
                                    splittrans.trans = buckets.Last()?.trans ?? new Transaction(); // Replace 'new Trans()' with your default value

                                    temp.initialEntryBuySplitTrans.Add(splittrans);
                                    RealizedTransactions.Add(temp);

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

                                    var temp = new RealizedTransaction
                                    {
                                        trans = currentTransaction,
                                        amount = buckets.Last().amount,
                                        gain = realizedGain,
                                        sellAmountReceivedUsuallyDollars = volume,// not sure what to put here
                                        initialEntryBuySplitTrans = new List<splitTransaction>()
                                    };

                                    splitTransaction splittrans = new splitTransaction();
                                    splittrans.portion = 1.0; // portion or weight

                                    var lastBucket = buckets.LastOrDefault(); // Get the last element, or null if the list is empty

                                    if (lastBucket == null || lastBucket.trans == null)
                                    {
                                        // Handle the case where trans is null (e.g., assign a default value or throw an exception)
                                        splittrans.trans = new Transaction();
                                        throw new Exception("bad");
                                    }
                                    else
                                    {
                                        splittrans.trans = lastBucket.trans;
                                    }



                                    temp.initialEntryBuySplitTrans.Add(splittrans);
                                    RealizedTransactions.Add(temp);

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
                    foreach (RealizedTransaction rt in RealizedTransactions)
                    {
                        if (rt.trans.dateTime.ToString().Contains(currentTaxYear))
                        {
                            totalGain3 += rt.gain;
                            totalVolume += rt.sellAmountReceivedUsuallyDollars;
                            totalAllYears += rt.gain;
                            totalAllYearsVolume += rt.sellAmountReceivedUsuallyDollars;
                        }
                        if (printIndividualTrans && rt.trans.dateTime.Year >= 2016)
                        {
                            Console.WriteLine(rt.trans.sellSymbol.ToString() + " " + rt.trans.dateTime.ToString() + " " + rt.amount.ToString(doubleFormat) + " price " + rt.gain.ToString(doubleFormat));
                        }
                    }
                }
                if (totalGain3 != 0)
                {
                    Console.WriteLine("TOTAL GAIN " + currentTaxYear + "\t" + symbol + "\tprofit:" + (totalGain3 / 1000).ToString("0.###") + "K\tSell Volume:" + (totalVolume / 1000).ToString("0.###") + "K");
                }
                //string doubleFormat2 = "0.####";
                //if (symbol.Contains("eth"))
                {
                    foreach (Bucket bucket in buckets)
                    {
                        if (bucket.amount > 89999990.05)
                        {
                            //Console.WriteLine(Bucket.amount.ToString(doubleFormat) + " Bucket " + Bucket.price.ToString(doubleFormat));
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

            bool NoSALES = true;
            // COMBINE on same day
            // COMBINE on same day
            // COMBINE on same day
            if (combineRealizedTransactionOnSameDay)
            {
                Console.WriteLine("combine realized");
                List<RealizedTransaction> combineRealizedTrans = new List<RealizedTransaction>();
                RealizedTransaction? currentCombineTrans = null;
                DateOnly currentDate = new DateOnly(1970,1,1);
                foreach (var g in RealizedTransactions)
                {
                    DateOnly thisDate = new DateOnly(g.trans.dateTime.Year, g.trans.dateTime.Month, g.trans.dateTime.Day);
                    
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
                        NoSALES = false;
                        currentCombineTrans.gain += g.gain;
                        currentCombineTrans.amount += g.amount;
                        currentCombineTrans.sellAmountReceivedUsuallyDollars += g.sellAmountReceivedUsuallyDollars;
                        
                        if(currentCombineTrans.trans?.sellSymbol != g.trans.sellSymbol)
                            throw new Exception("different sell symbol");
                        if(currentCombineTrans.trans.sellSymbol != g.trans.sellSymbol)
                            throw new Exception("different sell symbol");

                        currentCombineTrans.trans.buyAmount += g.trans.buyAmount;
                        currentCombineTrans.trans.sellAmount += g.trans.sellAmount;
                        if(currentCombineTrans.trans.altFeeAmount != null && g.trans.altFeeAmount != null)
                        {
                            currentCombineTrans.trans.altFeeAmount += g.trans.altFeeAmount;
                        }
                        currentCombineTrans.trans.feeAmount += g.trans.feeAmount;
                        currentCombineTrans.trans.combinedCount += g.trans.combinedCount;

                        foreach(var initialEntryBuySplitTran in g.initialEntryBuySplitTrans)
                        {
                            currentCombineTrans.initialEntryBuySplitTrans.Add(initialEntryBuySplitTran);
                        }
                    }

                }
                if (currentDate !=  new DateOnly())
                {
                    if(currentCombineTrans == null && NoSALES == false)
                    {
                        throw new Exception("bad");
                    }
                    if (currentCombineTrans != null)
                    {
                        combineRealizedTrans.Add(currentCombineTrans);
                    }
                }
                return combineRealizedTrans;
            }
            // END COMBINE on same day
            // END COMBINE on same day
            // END COMBINE on same day

            return RealizedTransactions;
        }

        public void summerizeRealized(string year, List<RealizedTransaction> RealizedTransactions)
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
            foreach (RealizedTransaction realized in RealizedTransactions)
            {
                if (count++ == 5000)
                {
                    Console.WriteLine("bad");
                }
                if(realized.trans == null)
                {
                    throw new Exception("bad");
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
                        TimeSpan diff = realized.trans.dateTime.Subtract(realized.initialEntryBuySplitTrans.First().trans.dateTime);
                        var days = diff.TotalDays;
                        if (days > 365)
                        {
                            longterm = "  LT";
                        }
                        double avgsell = realized.trans.buyAmount / realized.trans.sellAmount;
                        double avgbuy = realized.initialEntryBuySplitTrans.First().trans.sellAmount / realized.initialEntryBuySplitTrans.First().trans.buyAmount;
                        Console.WriteLine(sellExchangeAbriviation + "\t" + realized.gain.ToString(doubleFormatdollar) + "  \t   basis " + realized.trans.sellSymbol + ":" + realized.amount.ToString(doubleFormat) + "\t" +
                            realized.trans.dateTime.ToString("yyyy-MM-dd") + "\t" + realized.initialEntryBuySplitTrans.First().trans.dateTime.ToString("yyyy-MM-dd") + " " +
                            //" buy:" + realized.trans.buySymbol + 
                            "\tcost basis:" + (avgsell * realized.amount).ToString(doubleFormatdollar) + " " + (avgbuy * realized.amount).ToString(doubleFormatdollar) + " " +
                            //" Total sell:" + realized.trans.sellAmount.ToString(doubleFormat) + " avg:" + avgsell.ToString(doubleFormat) +
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

            Console.WriteLine("realizedBU gain loss Total:" + totalRealizedGainBinance.ToString(doubleFormat) + " " + totalRealizedLossBinance.ToString(doubleFormat) + " " + (totalRealizedGainBinance + totalRealizedLossBinance).ToString(doubleFormat));
            Console.WriteLine("realizedCB gain loss Total:" + totalRealizedGainCoinbase.ToString(doubleFormat) + " " + totalRealizedLossCoinbase.ToString(doubleFormat) + " " + (totalRealizedGainCoinbase + totalRealizedLossCoinbase).ToString(doubleFormat));
            Console.WriteLine("realizedKR gain loss Total:" + totalRealizedGainKraken.ToString(doubleFormat) + " " + totalRealizedLossKraken.ToString(doubleFormat) + " " + (totalRealizedGainKraken + totalRealizedLossKraken).ToString(doubleFormat));


        }

        public void convertBitcoinPairToTwoTransInUSD(List<Transaction> transactions, double averageFactorPercentZeroToOne = 0.5)
        {
            if (averageFactorPercentZeroToOne > 1 || averageFactorPercentZeroToOne < 0)
            {
                throw new Exception("bad averageFactorPercentZeroToOne");
            }

            List<Transaction> transToAdd = new List<Transaction>();
            Dictionary<string, int> skippedPairs = new Dictionary<string, int>();

            foreach (Transaction t in transactions)
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
                        if (t.dateTime.Date.Year == hp.dateOnly.Year &&
                            t.dateTime.Date.Month == hp.dateOnly.Month &&
                            t.dateTime.Date.Day == hp.dateOnly.Day
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
                            Transaction newTrans = new Transaction();
                            newTrans.dateTime = t.dateTime;
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
                            Transaction newTrans = new Transaction();
                            newTrans.dateTime = t.dateTime;
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
            transactions.Sort((x, y) => x.dateTime.CompareTo(y.dateTime));

            foreach (var d in skippedPairs)
            {
                if(d.Key.Contains("usd"))
                {
                    Console.WriteLine(d.Key + " " + d.Value.ToString());
                }
                else
                {
                    Console.WriteLine(d.Key + " " + d.Value.ToString());
                    //throw new Exception("bad averageFactorPercentZeroToOne");
                }
            }
        }

        public List<List<KeyValuePair<string,string>>> realizedTransToKeyValStringString(List<RealizedTransaction> realizedTrans)
        {
            List<List<KeyValuePair<string,string>>> ret = new List<List<KeyValuePair<string,string>>>();

            string doubleFormat = "0.#####";
            string doubleFormatMoney = "0.####";
            
            if(!realizedTrans.First().initialEntryBuySplitTrans.First().trans.sellSymbol.Contains("usd"))
            {
                // sold first so buy to close realizes profit
                Console.WriteLine("sdf");
                foreach (RealizedTransaction f in realizedTrans)
                {
                    double basis = 0;
                    double amt = 0;
                    double avgbuy = 0;
                    //double totalcost = 0;
                    int count = 0;
                    if(f.initialEntryBuySplitTrans == null)
                    {
                        throw new Exception("null error");
                    }
                    foreach (var t in f.initialEntryBuySplitTrans)
                    {
                        if(t.trans.sellSymbol == null)
                        {
                            throw new Exception("null error");
                        }
                        count++;
                        basis += t.trans.sellAmount * t.portion;
                        if(t.trans.sellSymbol.ToLower().Contains("usd"))
                        {
                            throw new Exception("bad");
                        }
                        amt += t.trans.buyAmount * t.portion;
                    }
                    avgbuy = basis / amt;

                    if(f.trans == null || f.trans.exchangeRec == null)
                    {
                        throw new Exception("null error");
                    }
                    List<KeyValuePair<string,string>> outline = new List<KeyValuePair<string,string>>();
                    outline.Add(new KeyValuePair<string, string>("date", f.trans.dateTime.ToString()));
                    //outline.Add(f.trans.sellSymbol + "amt:");
                    outline.Add(new KeyValuePair<string, string>("sell amount","" + f.trans.sellAmount.ToString(doubleFormat)));
                    outline.Add(new KeyValuePair<string, string>("exchange",f.trans.exchangeRec));
                    outline.Add(new KeyValuePair<string, string>("gain","" + f.gain.ToString(doubleFormatMoney)));
                    //outline.Add("$" + f.sellAmountReceivedUsuallyDollars.ToString());
                    outline.Add(new KeyValuePair<string, string>("basis","$" + basis.ToString(doubleFormatMoney)));
                    outline.Add(new KeyValuePair<string, string>("buy avgerage","avg" + avgbuy.ToString(doubleFormatMoney)));
                    outline.Add(new KeyValuePair<string, string>("sell average","avs" + (f.trans.buyAmount/f.trans.sellAmount).ToString(doubleFormatMoney)));
                    //outline.Add(count.ToString());
                    //outline.Add(f.gain.ToString());
                    //outline.Add(f.gain.ToString());

                    ret.Add(outline);
                }
            
            return ret;

            }

            foreach (RealizedTransaction f in realizedTrans)
            {
                double basis = 0;
                double amt = 0;
                double avgbuy = 0;
                //double totalcost = 0;
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

                List<KeyValuePair<string,string>> outline = new List<KeyValuePair<string, string>>();
                outline.Add(new KeyValuePair<string, string>("date",f.trans.dateTime.ToString()));
                //outline.Add(f.trans.sellSymbol + "amt:");
                outline.Add(new KeyValuePair<string, string>("sell amount","" + f.trans.sellAmount.ToString(doubleFormat)));
                outline.Add(new KeyValuePair<string, string>("exchange",f.trans.exchangeRec));
                outline.Add(new KeyValuePair<string, string>("gain","" + f.gain.ToString(doubleFormatMoney)));
                //outline.Add("$" + f.sellAmountReceivedUsuallyDollars.ToString());
                outline.Add(new KeyValuePair<string, string>("basis","$" + basis.ToString(doubleFormatMoney)));
                outline.Add(new KeyValuePair<string, string>("buy average","avg" + avgbuy.ToString(doubleFormatMoney)));
                outline.Add(new KeyValuePair<string, string>("sell average","avs" + (f.trans.buyAmount/f.trans.sellAmount).ToString(doubleFormatMoney)));
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

        public void printListListString(List<List<string>> data, string spacingString = " \t",  int stringLimit=12)
        {
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

        public void printListListKeyValueStringString(List<List<KeyValuePair<string,string>>> data, string spacingString = " \t",  int stringLimit=12)
        {
            foreach (var d in data)
            {
                foreach(KeyValuePair<string,string> s in d)
                {
                    string h = s.Value;
                    if(s.Value.Length > stringLimit)
                    {
                        h = s.Value.Substring(0, stringLimit);
                    }
                    Console.Write(h);
                    string filler = "";
                    for(int i=s.Value.Length; i<stringLimit; i++)
                    {
                        filler += " ";
                    }
                    Console.Write(filler);
                    Console.Write(spacingString);
                }
                Console.Write("\n");
            }
        }

        public List<List<string>> summerizeBucketsToStringList(List<Bucket> buckets)
        {
            List<List<string>> ret = new List<List<string>>();
            if(buckets.Count() ==0)
            {
                return ret;
            }
            string doubleFormat = "0.####";

            double totalAmount = 0;
            double totalPrice = 0;
            int count = 0;

            int numberToCombine = 20;
            foreach(var b in buckets)
            {
                count++;
                if(buckets.Count < count + (5+numberToCombine) && buckets.Count >= count + 5 )
                {
                    totalAmount += b.amount;
                    totalPrice += (b.price * b.amount);
                }
            }
            List<string> bucketStrings = [
                        "Buckets " + numberToCombine + " Avg: " + (totalPrice / totalAmount).ToString(doubleFormat),
                        "Amt: " + (totalAmount).ToString(doubleFormat)];
            ret.Add(bucketStrings);

            count = 0;
            foreach(var b in buckets)
            {
                count++;
                totalAmount += b.amount;
                totalPrice += (b.price * b.amount);
                if(buckets.Count < count + 5)
                {
                    bucketStrings = [
                        "Bucket Price: " + b.price.ToString(doubleFormat), 
                        "Amt: " + b.amount.ToString(doubleFormat)];
                    ret.Add(bucketStrings);
                }
            }

            double avgPrice = totalPrice / totalAmount;

            List<string> summary =
            [
                buckets.First().symbol + " Avg Price: " + avgPrice.ToString(doubleFormat),
                "Amt: " + totalAmount.ToString(doubleFormat),
            ];
            ret.Add(summary);
            return ret;
        }

        public static void Go()
        {
            Console.WriteLine("Tax Calculator Library");
        }

        
        public List<KeyValuePair<string, double>> computeVolumeExchanges(List<Transaction> transactions, bool includeOtherSymbol = true, bool onlyCurrentYear = true)
        {
            List<KeyValuePair<string, double>> ret = new List<KeyValuePair<string, double>>();

            foreach(Transaction t in transactions)
            {
                if(t.dateTime.Year == DateTime.Now.Year || onlyCurrentYear == false)
                {
                    string currtKeyBuy = t.buySymbol + "_" + t.exchangeRec + "_b_" + t.dateTime.Year;
                    string currtKeySell = t.sellSymbol + "_" + t.exchangeSent + "_s_" + t.dateTime.Year;

                    if(includeOtherSymbol)
                    {
                        currtKeyBuy = t.buySymbol + "_" + t.exchangeRec + "_b_sell_" + t.sellSymbol + "_" + t.dateTime.Year;
                        currtKeySell = t.sellSymbol + "_" + t.exchangeSent + "_s_buy__" + t.buySymbol + "_" + t.dateTime.Year;
                    }

                    KeyValuePair<string, double> curr = ret.FirstOrDefault(x => x.Key == currtKeyBuy);
                    //if(curr == KeyValuePair.<string,double>.d)
                    if(curr.Key == null)
                    {
                        ret.Add(new KeyValuePair<string, double>(currtKeyBuy, t.buyAmount));
                    }
                    else
                    {
                        var t2 = ret.First(x => x.Key == currtKeyBuy);
                        KeyValuePair<string, double> toremove = new KeyValuePair<string, double>(t2.Key, t2.Value);
                        ret.Remove(toremove);
                        KeyValuePair<string, double> toreplacewith = new KeyValuePair<string, double>(t2.Key, t2.Value + t.buyAmount);
                        ret.Add(toreplacewith);
                    }

                    KeyValuePair<string, double> curr2 = ret.FirstOrDefault(x => x.Key == currtKeySell);
                    if(curr2.Key == null)
                    {
                        ret.Add(new KeyValuePair<string, double>(currtKeySell, t.sellAmount));
                    }
                    else
                    {
                        var t2 = ret.First(x => x.Key == currtKeySell);
                        KeyValuePair<string, double> toremove = new KeyValuePair<string, double>(t2.Key, t2.Value);
                        ret.Remove(toremove);
                        KeyValuePair<string, double> toreplacewith = new KeyValuePair<string, double>(t2.Key, t2.Value + t.sellAmount);
                        ret.Add(toreplacewith);

                    }
                }

            }
            ret = ret.OrderBy(kvp => kvp.Key).ToList();
            return ret;

        }

        public List<string> lastNtrans( string symb, List<Transaction> trans, bool ytd, bool pytd = false, int n=0)
        {
            int testCount = trans.Count();
            List<string> ret = new List<string>();
            double total = 0;
            double volumeDollars = 0;
            double lastTransYear = 0;
            double totalAssetBuy = 0;
            double totalAssetSell = 0;
            double totalBuyInDollars = 0;
            double totalSellInDollars = 0;

            if(ytd && pytd){
                throw new Exception("Error: Cant use year to date and previous year to date");
            }
            if(ytd)
            {
                var lastTrans = trans.LastOrDefault(x => x?.dateTime != null);
                if (lastTrans != null)
                {
                    lastTransYear = lastTrans.dateTime.Year;
                }
                else
                {
                    // Handle the case where no valid transaction is found
                    lastTransYear = 0; // or some default value
                }


            }
            
            if(pytd)
            {
                lastTransYear = trans.FindLast(x=> true).dateTime.Year;
                lastTransYear-=1;
            }

            if (n == 0)
            {
                n=99999999;
            }

            
            int curr=0;
            int startIndex = trans.Count(x => 
            (x.buySymbol.ToLower() == symb.ToLower() && x.sellSymbol.ToLower().Contains("usd")) ||
            (x.sellSymbol.ToLower() == symb.ToLower() && x.buySymbol.ToLower().Contains("usd"))
            ) - n;

            List<Transaction> reversed = trans.ToList();
            reversed.Reverse();

            foreach(Transaction t in reversed)
            {
                if(
                    (t.buySymbol.ToLower() == symb.ToLower() && t.sellSymbol.ToLower().Contains("usd")) ||
                    (t.sellSymbol.ToLower() == symb.ToLower() && t.buySymbol.ToLower().Contains("usd"))
                ){
                    curr++;
                }

                if(curr >= startIndex && (!ytd || (ytd && t.dateTime.Year == lastTransYear)))
                {
                    if(t.buySymbol.ToLower() == symb.ToLower() && t.sellSymbol.ToLower().Contains("usd"))
                    {
                        //buying the asset with dollars
                        string line = "";
                        total += t.buyAmount;
                        volumeDollars += t.sellAmount;
                        line += "$-" + t.sellAmount.ToString("######.00") + "       " + t.dateTime.ToString("yy/MM/dd") + "   " + t.buyAmount.ToString("####.0000") +
                        "   @$" + (t.sellAmount / t.buyAmount).ToString("####.00") + "  " + symb + total.ToString("####.0000") + " vol$" + volumeDollars.ToString("####.00");
                        ret.Add(line);

                        totalAssetBuy += t.buyAmount;
                        totalBuyInDollars += t.sellAmount; // selling dollars
                    }
                    if(t.sellSymbol.ToLower() == symb.ToLower() && t.buySymbol.ToLower().Contains("usd"))
                    {
                        //selling the asset getting dollars
                        string line = "";
                        total += t.sellAmount;
                        volumeDollars += t.buyAmount;
                        line += "     $+" + t.buyAmount.ToString("######.00") + "  " + t.dateTime.ToString("yy/MM/dd") + "   " + t.sellAmount.ToString("####.0000") +
                        "   @$" + (t.buyAmount / t.sellAmount).ToString("####.00") + "  " + symb + total.ToString("####.0000") + " vol$" + volumeDollars.ToString("####.00");
                        ret.Add(line);

                        totalAssetSell += t.sellAmount;
                        totalSellInDollars += t.buyAmount; // buying dollars
                    }
                }
                
            }

            double avgbuy = totalBuyInDollars/totalAssetBuy;
            double avgsell = totalSellInDollars/totalAssetSell;
            string summary = 
            "buy  " + totalAssetBuy.ToString("00000.000") +
            "    " + (avgbuy).ToString() + "    " +
            "sell " + totalAssetSell.ToString("00000.000") 
            + "    " + (avgsell).ToString() + "    ";
            ret.Add(summary);
            ret.Insert(0, summary);

            if(totalAssetBuy > totalAssetSell)
            {
                string summary2 = 
                "profit:  $" + ((avgsell - avgbuy)*totalAssetSell).ToString("00000.000");
                ret.Add(summary2);
                ret.Insert(0, summary2);
            }


            return ret;

        }

        public List<string> CreateTurboTaxImportCSV(List<RealizedTransaction> realizedTransactions, string exchangeFilter, out double totalGain, out double totalProceeds, out double totalBasis)
        {
            List<string> ret = new List<string>();
            ret.Add("Transaction Type,Transaction ID,Tax lot ID,Asset name,Amount,Date Acquired,Cost basis (USD),Date of Disposition,Proceeds (USD),Gains (Losses) (USD),Holding period (Days),Data source");
            //Transaction Type,   sell
            //Transaction ID,   guid
            //Tax lot ID,     guid
            //Asset name,    BTC
            //Amount,        0.1
            //Date Acquired,     12/28/2016
            //Cost basis (USD),    103.7390314068148
            //Date of Disposition,  12/28/2016
            //Proceeds (USD),       103.00000000000
            //Gains (Losses) (USD), 0.7390314068148
            //Holding period (Days),  0
            //Data source            Mt.Gox
            //based on coinbase csv

            totalBasis = 0;
            totalGain = 0;
            totalProceeds = 0;
            foreach(RealizedTransaction rt in realizedTransactions)
            {
                DateOnly acquired = new DateOnly(rt.trans.dateTime.Year, rt.trans.dateTime.Month, rt.trans.dateTime.Day);
                DateOnly disposition = new DateOnly(rt.trans.dateTime.Year, rt.trans.dateTime.Month, rt.trans.dateTime.Day);
                int daysHeld = disposition.DayNumber - acquired.DayNumber;
                string dataSource = "Ethereum";
                if(rt.trans.exchangeSent.Contains("oinbase") || rt.trans.exchangeRec.Contains("oinbase"))
                {
                    dataSource = "Coinbase";
                }
                if(rt.trans.exchangeSent.Contains("raken") || rt.trans.exchangeRec.Contains("raken"))
                {
                    dataSource = "Kraken";
                }
                if(rt.trans.exchangeSent.Contains("inance") || rt.trans.exchangeRec.Contains("inance"))
                {
                    dataSource = "Binance";
                }
                if(rt.trans.exchangeSent.Contains("ox") || rt.trans.exchangeRec.Contains("ox"))
                {
                    dataSource = "Mt.Gox";
                }

                string line = "Sell, " + Guid.NewGuid().ToString() + ", " + Guid.NewGuid().ToString() + ", "
                + rt.trans.sellSymbol.ToString() + ", " +  rt.amount.ToString("#.00000000") + ", "
                + acquired.ToString("MM/dd/yyyy") + ", " + (rt.sellAmountReceivedUsuallyDollars - rt.gain).ToString("#.0000") + ", "
                + disposition.ToString("MM/dd/yyyy") + ", " + rt.sellAmountReceivedUsuallyDollars.ToString("#.0000") + ", " + rt.gain.ToString("#.0000") + ", "
                + daysHeld + ", " + dataSource;

                if(exchangeFilter == dataSource) // only include items for this exchange
                {
                    ret.Add(line);
                    totalGain += rt.gain;
                    totalProceeds += rt.sellAmountReceivedUsuallyDollars;
                    totalBasis += rt.sellAmountReceivedUsuallyDollars - rt.gain;
                }

                double sensativityCheck = 0.0001;
                
                double one = rt.trans.buyAmount / rt.trans.sellAmount - sensativityCheck;
                double two = rt.sellAmountReceivedUsuallyDollars / rt.amount;
                if (rt.trans.buyAmount / rt.trans.sellAmount + sensativityCheck < rt.sellAmountReceivedUsuallyDollars / rt.amount)
                {
                    throw new Exception("bad");
                }
                one = rt.trans.buyAmount / rt.trans.sellAmount - sensativityCheck;
                two = rt.sellAmountReceivedUsuallyDollars / rt.amount;
                if (rt.trans.buyAmount / rt.trans.sellAmount - sensativityCheck > rt.sellAmountReceivedUsuallyDollars / rt.amount)
                {
                    throw new Exception("bad");
                }
                if(!rt.trans.buySymbol.Contains("usd"))
                {
                    throw new Exception("bad");
                }
            }

            return ret;
        }

        public static List<historicDayPriceUSD> ReadHistoric(string pathAndFileName)
        {
            List<historicDayPriceUSD> ret = new List<historicDayPriceUSD>();

            List<string> lines = new List<string>(File.ReadAllLines(pathAndFileName));

            int count = 0;
            foreach (var line in lines)
            {
                if (count++ > 0)
                {
                    List<DateTime> dateTimes = new List<DateTime>();
                    List<double> numbers = new List<double>();
                    List<string> others = new List<string>();

                    string[] items = line.Split(';');
                    foreach (var item in items)
                    {
                        string trimmed = item.Trim('"'); // Remove quotes if present

                        if (DateTime.TryParseExact(trimmed, "yyyy-MM-ddTHH:mm:ss.fffZ",
                                                   CultureInfo.InvariantCulture,
                                                   DateTimeStyles.AssumeUniversal,
                                                   out DateTime dateTime))
                        {
                            dateTimes.Add(dateTime);
                        }
                        else if (double.TryParse(trimmed, out double number))
                        {
                            numbers.Add(number);
                        }
                        else
                        {
                            others.Add(trimmed);
                        }

                    }

                    historicDayPriceUSD hp = new historicDayPriceUSD();
                    hp.firstPrice = numbers[0];
                    hp.high = numbers[1];
                    hp.low = numbers[2];
                    hp.lastPrice = numbers[3];
                    hp.dateOnly = new DateOnly(dateTimes.First().Year, dateTimes.First().Month, dateTimes.First().Day);
                    hp.volume = numbers[4];

                    ret.Add(hp);
                }
            }

            return ret;

        }

    }

}


        /*private RealizedTransaction deepCopy(RealizedTransaction a)
        { delete later in class now
            RealizedTransaction b = new RealizedTransaction();
            b.amount = a.amount;
            b.gain = a.gain;
            b.initialEntryBuySplitTrans = new List<splitTransaction>();
            foreach(var t in a.initialEntryBuySplitTrans){
                var r = new splitTransaction{portion = t.portion, trans = new Transaction()};
                r.trans = t.trans.deepCopy();
                b.initialEntryBuySplitTrans.Add(r);
            }
            b.sellAmountReceivedUsuallyDollars = a.sellAmountReceivedUsuallyDollars;
            b.trans = a.trans.deepCopy();
            
            return b;
        }*/

        /*
        public void computeBuysSellsSymbol(List<Transaction> transactions, string symbol)
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

                List<Total> totalBuy = new List<Total>();
                List<Total> totalSell = new List<Total>();
                //List<Total> totalBoth = new List<Total>();

                //buys
                foreach (Transaction t in transactions)
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
                                Total r = new Total();
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
                foreach (Transaction t in transactions)
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
                                Total r = new Total();
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

                            //Total u = totalsSell.First(s => s.buySymbol == t.buySymbol && s.sellSymbol == t.sellSymbol);

                            //if (u is null)
                            {
                            //    throw new Exception("bad");
                            }
                            //else
                            {
                                // t is current Transaction
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

                foreach (Total mysell in totalSell)
                {
                    if (mysell.sellSymbol == "eth")
                    {
                        //Console.WriteLine(mysell.sellSymbol + " sell amt:" + mysell.Amount + " avg:" + mysell.Average);

                    }
                }

                foreach (Total mybuy in totalBuy)
                {
                    if (mybuy.buySymbol == "eth")
                    {
                        //Console.WriteLine(mybuy.sellSymbol + " buy amt:" + mybuy.Amount + " avg:" + mybuy.Average);

                    }
                }
                Console.WriteLine(year + " totalAmount " + totalAmount);
            }

        }*/

