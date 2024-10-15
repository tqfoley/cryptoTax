using gaintaxlibrary;

public static class StringExtensions
{
    public static bool stringContainsIgnoreCase(this string source, string h)
    {
        return source.ToLower().Contains(h.ToLower());
    }
    public static bool stringMatchesIgnoreCase(this string source, string h)
    {
        return source.ToLower().Contains(h.ToLower()) && h.ToLower().Contains(source.ToLower());
    }
}

namespace main
{
    static class GainTaxClass
    {
        private static void addMyTransactions(List<transaction> mytrans)
    {
        mytrans.Add(new transaction
        {
                buyAmount = 1,
                buySymbol = "btc",
                sellAmount = 1000,
                sellSymbol = "usd",
                dateTime = new DateTime(2017, 1, 1),
                exchangeRec = "",
                exchangeSent = "",
                combinedCount = 1
        });

        mytrans.Add(new transaction
        {
                buyAmount = 2000,
                buySymbol = "usd",
                sellAmount = 0.1,
                sellSymbol = "btc",
                dateTime = new DateTime(2024, 1, 1),
                exchangeRec = "",
                exchangeSent = "",
                combinedCount = 1
        });
    }

        private static bool stringContainsIgnoreCase(this string source, string h)
        {
            return source.ToLower().Contains(h.ToLower());
        }

        private static List<transaction> readFile20ColumnCSV(string filename = "../../trans20column.csv", DateTime? startdate = null)
        {
            List<transaction> ret = new List<transaction>();

            string[] linesProblemsNewLines = File.ReadAllLines(filename);
            string currentString;

            // some of the line exchange names have new lines in the string, readAllLines splits on a newline
            List<string> lines = new List<string>();
            for (int i = 0; i < linesProblemsNewLines.Length; i++)
            {
                currentString = linesProblemsNewLines[i];
                int countCommas = currentString.Count(f => f == ',');
                if (countCommas < 17)
                {
                    i++;
                    currentString += linesProblemsNewLines[i];
                    countCommas = currentString.Count(f => f == ',');
                    if (countCommas < 17)
                    {
                        i++;
                        currentString += linesProblemsNewLines[i];
                        countCommas = currentString.Count(f => f == ',');

                    }
                }
                if (countCommas != 20)
                {
                    throw new Exception("bad");
                }
                lines.Add(currentString);
            }


            int linenumber = 0;
            foreach (string line in lines)
            {
                string[] col = line.Split(',');
                linenumber++;

                if (col.Length < 18)
                {
                    int g = 0;
                }
                else if (col[0] == "")
                {
                    //last line;
                    int g = 0;
                }
                else if (linenumber == 1)
                {
                    Console.WriteLine(col[0].ToString());
                    Console.WriteLine(col[1].ToString());
                    Console.WriteLine(col[2].ToString());
                    Console.WriteLine(col[3].ToString());
                    Console.WriteLine(col[4].ToString());
                    Console.WriteLine(col[5].ToString());
                    Console.WriteLine(col[6].ToString());
                }
                else
                {
                    if (true)
                    {
                        string transType = col[1];

                        if (transType.stringMatchesIgnoreCase("Mint"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("margin_rebate"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("margin"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("margin_loss"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("margin_gain"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("income"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("spam"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("loan_repayment"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("lending_withdrawal"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("lending_deposit"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("borrow"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("other_income"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("interest_payment"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("stake"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("unstake"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("staking_reward"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("Send")
                            || transType.stringMatchesIgnoreCase("Receive")
                            || transType.stringMatchesIgnoreCase("Transfer"))
                        {
                            /*transfer t = new transfer();
                            t.dateTime = DateTime.Parse(col[0]);
                            if (transType.stringMatchesIgnoreCase("Send"))
                            {
                                t.receive = false;

                                if (double.TryParse(col[9], out t.amount))
                                {

                                    t.symbol = col[10].ToLower();
                                }
                                else
                                {
                                    t.symbol = col[10].ToLower() + "UNKNOWN AMOUNT";
                                    t.amount = 0;
                                }

                            }
                            else if (transType.stringMatchesIgnoreCase("Recieve"))
                            {
                                t.receive = true;
                                t.amount = double.Parse(col[3]);
                                t.symbol = col[4].ToLower();
                            }
                            else if (transType.stringMatchesIgnoreCase("Transfer"))
                            {
                                if (false)
                                {
                                    t.receive = true;
                                    t.amount = double.Parse(col[3]);
                                    t.symbol = col[4].ToLower();
                                    if (double.TryParse(col[9], out t.amount))
                                    {

                                        t.symbol = col[10].ToLower();
                                    }
                                    else
                                    {
                                        t.symbol = col[10].ToLower() + "UNKNOWN AMOUNT";
                                        t.amount = 0;
                                    }

                                    if (double.TryParse(col[9], out t.amount))
                                    {

                                        t.symbol = col[10].ToLower();
                                    }
                                    else
                                    {
                                        t.symbol = col[10].ToLower() + "UNKNOWN AMOUNT";
                                        t.amount = 0;
                                    }
                                }

                            }
                            if (t.symbol == null)
                            {

                            }
                            else if (t.symbol.stringMatchesIgnoreCase("wbtc"))
                            {
                                t.symbol = "wbwtwcw"; // since we just look for btc in a string make wbtc unique such as wbitc
                            }
                            transfers1.Add(t);*/
                        }
                        else if (transType.stringMatchesIgnoreCase("Buy") ||
                            transType.stringMatchesIgnoreCase("Sell") ||
                            transType.stringMatchesIgnoreCase("Trade"))
                        {
                            if (true//!col[6].Contains("ptimism") && !col[13].Contains("ptimism") &&
                                    //!col[6].Contains("Ethereum Wallet ...48Cdf3 - ETH") &&
                                    //!col[13].Contains("Ethereum Wallet ...48Cdf3 - ETH") &&
                                    //!col[6].Contains("Ethereum Wallet ...AbC856 - ETH") &&
                                    //!col[13].Contains("Ethereum Wallet ...AbC856 - ETH") &&
                                    //col[3] != "" && col[10] != "" // amounts
                                )
                            {

                                //0 Date,
                                //1Type,
                                //2 Transaction ID,
                                //3 Received Quantity,
                                //4 Received Currency,
                                //5 Received Cost Basis (USD),
                                //6 Received Wallet,
                                //7 Received Address,
                                //8 Received Comment,
                                //9 Sent Quantity,
                                //10 Sent Currency,
                                //11 Sent Cost Basis (USD),
                                //12 Sent Wallet,
                                //13 Sent Address,
                                //14 Sent Comment,
                                //15 Fee Amount,
                                //16 Fee Currency,
                                //17 Fee Cost Basis (USD),
                                //18 Realized Return (USD),
                                //19 Fee Realized Return (USD)

                                transaction t = new transaction();
                                t.dateTime = DateTime.Parse(col[0]);
                                if (startdate == null || t.dateTime.Value.Date >= startdate.Value.Date)
                                {
                                    t.optionalSecondTansDate = null;

                                    if (double.TryParse(col[3], out t.buyAmount))
                                    {
                                        t.buySymbol = col[4].ToLower();
                                    }
                                    else
                                    {
                                        t.buySymbol = col[4].ToLower() + "UNKNOWN AMOUNT";
                                        t.buyAmount = 0;
                                    }
                                    //t.buyAmount = double.Parse(col[3]);
                                    //t.buySymbol = col[4].ToLower();

                                    //t.sellAmount = double.Parse(col[9]);
                                    //t.sellSymbol = col[10].ToLower();
                                    if (double.TryParse(col[9], out t.sellAmount))
                                    {
                                        t.sellSymbol = col[10].ToLower();
                                    }
                                    else
                                    {
                                        t.sellSymbol = col[10].ToLower() + "UNKNOWN AMOUNT";
                                        t.sellAmount = 0;
                                    }
                                    

                                    t.exchangeRec = col[6].ToLower();
                                    t.exchangeSent = col[13].ToLower();

                                    if (linenumber % 1000 == 0)
                                    {
                                        Console.WriteLine(t.buySymbol.ToString() + "  " + t.sellAmount.ToString());
                                    }

                                    if (t.sellSymbol.stringMatchesIgnoreCase("wbtc"))
                                    {
                                        t.sellSymbol = "wbwtwcw";
                                    }

                                    if (t.buySymbol.stringMatchesIgnoreCase("wbtc"))
                                    {
                                        t.buySymbol = "wbwtwcw";// since we just look for btc in a string make wbtc unique such as wbitc
                                                                // addTrans = false;
                                    }
                                    if (t.sellSymbol.ToLower().stringContainsIgnoreCase("usd"))
                                    {
                                        if (t.sellSymbol.ToLower().stringContainsIgnoreCase("usd") == true ||
                                            t.sellSymbol.ToLower().stringContainsIgnoreCase("usdt") == true ||
                                            t.sellSymbol.ToLower().stringContainsIgnoreCase("busd") == true ||
                                            t.sellSymbol.ToLower().stringContainsIgnoreCase("usdc") == true
                                            )
                                        {
                                            t.sellSymbol = "usd";
                                        }
                                        else
                                        {
                                            int h = 0;
                                        }
                                    }

                                    if (t.buySymbol.ToLower().stringContainsIgnoreCase("usd"))
                                    {
                                        if (t.buySymbol.ToLower().stringContainsIgnoreCase("usd") == true ||
                                            t.buySymbol.ToLower().stringContainsIgnoreCase("usdt") == true ||
                                            t.buySymbol.ToLower().stringContainsIgnoreCase("busd") == true ||
                                            t.buySymbol.ToLower().stringContainsIgnoreCase("usdc") == true
                                            )
                                        {
                                            t.buySymbol = "usd";
                                        }
                                        else
                                        {
                                            int h = 0;
                                        }
                                    }

                                    if (t.buySymbol.Contains("weth") && t.dateTime.Value.Date.Day == 18)
                                    {
                                        t.buySymbol = "weth";
                                    }

                                    double amountfee = 0; 
                                    string symbolfee = "";
                                    if (double.TryParse(col[15], out amountfee))
                                    {

                                        symbolfee = col[16].ToLower();

                                        if (symbolfee.ToLower().stringContainsIgnoreCase("usd") == true ||
                                            symbolfee.ToLower().stringContainsIgnoreCase("usdt") == true ||
                                            symbolfee.ToLower().stringContainsIgnoreCase("busd") == true ||
                                            symbolfee.ToLower().stringContainsIgnoreCase("usdc") == true
                                            )
                                        {
                                            symbolfee = "usd";
                                        }
                                    }
                                    if (symbolfee.Contains("usd") 
                                        )
                                    {
                                        t.feeSymbol = symbolfee;
                                        t.feeAmount = amountfee;
                                        t.altFeeAmount = 0;
                                        t.altFeeSymbol = "";

                                    }
                                    else if (t.exchangeRec.Contains("ethereum wallet ...") ||
                                        t.exchangeSent.Contains("ethereum wallet ..."))
                                    {
                                        t.feeSymbol = symbolfee;
                                        t.feeAmount = amountfee;
                                        t.altFeeAmount = 0;
                                        t.altFeeSymbol = "";

                                    }
                                    else if (amountfee == 0)
                                    {

                                        t.feeSymbol = symbolfee;
                                        t.feeAmount = amountfee;
                                        t.altFeeAmount = 0;
                                        t.altFeeSymbol = "";
                                    }
                                    else if (t.buySymbol.Contains("usd") && t.sellSymbol.Contains(symbolfee))
                                    {

                                        t.altFeeAmount = amountfee;
                                        t.altFeeSymbol = symbolfee;
                                        if(t.altFeeSymbol.Contains("usd"))
                                        {
                                            Console.WriteLine("issue");
                                        }

                                        t.feeAmount = amountfee * t.buyAmount / t.sellAmount;
                                        t.feeSymbol = t.buySymbol.ToLower();

                                    }
                                    else if (t.sellSymbol.Contains("usd") && t.buySymbol.Contains(symbolfee))
                                    {

                                        t.altFeeAmount = amountfee;
                                        t.altFeeSymbol = symbolfee;
                                        if (t.altFeeSymbol.Contains("usd"))
                                        {
                                            Console.WriteLine("issue");
                                        }
                                        t.feeAmount = amountfee * t.sellAmount / t.buyAmount;
                                        t.feeSymbol = t.sellSymbol.ToLower();

                                    }
                                    else
                                    {
                                        t.altFeeAmount = amountfee ;
                                        t.altFeeSymbol = symbolfee;

                                        t.feeSymbol = "";
                                        t.feeAmount = 0;
                                    }

                                    if(t.feeSymbol == null)
                                    {
                                        Console.WriteLine("bad");
                                    }

                                    if (t.buySymbol.Contains("weth") && t.dateTime.Value.Date.Day == 18 && t.feeAmount > 1111)
                                    {
                                        t.buySymbol = "weth";
                                    }


                                    if (t.buyAmount < 0.0000000001 || t.sellAmount < 0.0000000001)
                                    {
                                        Console.WriteLine("BAD " + t.dateTime.Value + " " + t.sellSymbol + " " + t.buySymbol + " " + t.buyAmount);
                                    }
                                    else if (t.buyAmount / t.sellAmount > 1000111.0 || t.sellAmount / t.buyAmount > 1000111.0)
                                    {
                                        Console.WriteLine("BAD " + t.dateTime.Value + " " + t.sellSymbol + " " + t.buySymbol + " " + t.buyAmount);
                                    }
                                    else
                                    {
                                        ret.Add(t);
                                    }
                                    //}
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Error Unexpcted string");
                        }
                    }
                }
            }
            return ret;
        }

        static void Main(string[] args)
        {
            gaintaxlibrary.ClassGainTax.Go();

            var trans = readFile20ColumnCSV();

            ClassGainTax t = new ClassGainTax();
            t.transactionsOriginal = trans;//new List<transaction>();
            t.transactionsOriginal= t.transactionsOriginal.OrderBy(x => x.dateTime.Value).ToList();

            List<bucket> buckets = new List<bucket>();
            var realizedbtc = t.computeGains(out buckets, false, "btc", "fiho", t.transactionsOriginal);

            //addMyTransactions());

        }
    }
}
