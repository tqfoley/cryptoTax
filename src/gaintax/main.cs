using gaintaxlibrary;
using System.Xml.Serialization;

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

/// <summary>
/// Writes the given object instance to an XML file.
/// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
/// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
/// <para>Object type must have a parameterless constructor.</para>
/// </summary>
/// <typeparam name="T">The type of object being written to the file.</typeparam>
/// <param name="filePath">The file path to write the object instance to.</param>
/// <param name="objectToWrite">The object instance to write to the file.</param>
/// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
{
    TextWriter writer = null;
    try
    {
        var serializer = new XmlSerializer(typeof(T));
        writer = new StreamWriter(filePath, append);
        serializer.Serialize(writer, objectToWrite);
    }
    finally
    {
        if (writer != null)
            writer.Close();
    }
}

/// <summary>
/// Reads an object instance from an XML file.
/// <para>Object type must have a parameterless constructor.</para>
/// </summary>
/// <typeparam name="T">The type of object to read from the file.</typeparam>
/// <param name="filePath">The file path to read the object instance from.</param>
/// <returns>Returns a new instance of the object read from the XML file.</returns>
public static T ReadFromXmlFile<T>(string filePath) where T : new()
{
    TextReader reader = null;
    try
    {
        var serializer = new XmlSerializer(typeof(T));
        reader = new StreamReader(filePath);
        return (T)serializer.Deserialize(reader);
    }
    finally
    {
        if (reader != null)
            reader.Close();
    }
}

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

        private static List<transaction> readFile20ColumnCSV(string filename = "../../trans20Column.csv", DateTime? ignoreBeforeDate = null,
                                int columnDateTime = 0,
                                int columnBuyAmt = 3,
                                int columnBuySym = 4,
                                int columnSellAmt = 9,
                                int columnSellSymb = 10,
                                int columnExchangeReceive = 6,
                                int columnExchangeSent = 13,
                                int columnFee = 15)
        {
            //0 Date,
            //1 Type,
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
            //13 Sent Address,Sent Comment,
            //14 Fee Amount,
            //15 Fee Currency,
            //16 Fee Cost Basis (USD),
            //17 Realized Return (USD),
            //18 Fee Realized Return (USD),
            //19 Transaction Hash

                                

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
                        else if (transType.stringMatchesIgnoreCase("ADD_LIQUIDITY".ToLower()))
                        {

                        }
                        else if (transType.stringMatchesIgnoreCase("BRIDGE".ToLower()))
                        {

                        }
                        else if (transType.stringMatchesIgnoreCase("REMOVE_LIQUIDITY".ToLower()))
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
                                //1 Type,
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
                                t.dateTime = DateTime.Parse(col[columnDateTime]);
                                if (ignoreBeforeDate == null || t.dateTime.Value.Date >= ignoreBeforeDate.Value.Date)
                                {
                                    t.optionalSecondTansDate = null;

                                    if (double.TryParse(col[columnBuyAmt], out t.buyAmount))
                                    {
                                        t.buySymbol = col[columnBuySym].ToLower();
                                    }
                                    else
                                    {
                                        t.buySymbol = col[columnBuySym].ToLower() + "UNKNOWN AMOUNT";
                                        t.buyAmount = 0;
                                    }
                                    //t.buyAmount = double.Parse(col[3]);
                                    //t.buySymbol = col[4].ToLower();

                                    //t.sellAmount = double.Parse(col[9]);
                                    //t.sellSymbol = col[10].ToLower();
                                    if (double.TryParse(col[columnSellAmt], out t.sellAmount))
                                    {
                                        t.sellSymbol = col[columnSellSymb].ToLower();
                                    }
                                    else
                                    {
                                        t.sellSymbol = col[columnSellSymb].ToLower() + "UNKNOWN AMOUNT";
                                        t.sellAmount = 0;
                                    }
                                    

                                    t.exchangeRec = col[columnExchangeReceive].ToLower();
                                    t.exchangeSent = col[columnExchangeSent].ToLower();

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
                                    string test = col[columnFee];
                                    if (double.TryParse(col[columnFee], out amountfee))
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
                                        //pepe hits this
                                        Console.WriteLine("BAD " + t.dateTime.Value + " " + t.sellSymbol + " " + t.buySymbol + " " + t.buyAmount);
                                    }
                                    else
                                    {
                                        ret.Add(t);
                                    }
                                    
                                }
                            }
                        }
                        else
                        {
                            string g = line;
                            throw new Exception("Error Unexpcted string");
                        }
                    }
                }
            }
            return ret;
        }

        private static List<transaction> readSimpleFile10ColumnCSV( string filename = "../../transSimple10Column.csv", DateTime? ignoreBeforeDate = null,
                                int columnDate = 0,
                                int columnSymbol = 3,
                                int columnAction = 4,
                                int columnQty = 5,
                                int columnAmt = 7)
        {
            //date, 0
            //time, 1
            //type, 2
            //symbol, 3
            //action,  4
            //quantity, 5
            //price, 6 
            //amount, 7
            //fees,   8
            //details 9

            List<transaction> ret = new List<transaction>();
            string[] lines = File.ReadAllLines(filename);

            //double realizedgain = 0;
            int linenumber = 0;
            foreach (string line in lines)
            {
                string[] col = line.Split(",");
                linenumber++;

                if (col.Length < 9 || linenumber < 2)
                {
                    int g = 0;
                }
                else
                {
                    
                    if (ignoreBeforeDate == null || DateTime.Parse(col[columnDate]) >= ignoreBeforeDate.Value.Date)
                    {
                    if (col[columnAction].Contains("Buy") ||
                        col[columnAction].Contains("BTO") || // buy to open
                        col[columnAction].Contains("BTC")) // buy to close
                    {
                        transaction t = new transaction();
                        t.buySymbol = col[columnSymbol].ToLower();
                        t.sellSymbol = "usd";
                        t.exchangeRec = "robinhood";
                        t.exchangeSent = "robinhood";
                        t.buyAmount = double.Parse(col[columnQty]); //quantity
                        t.sellAmount = double.Parse(col[columnAmt]); //amount
                        t.dateTime = DateTime.Parse(col[columnDate]);
                        t.feeSymbol = "";
                        t.feeAmount = 0;
                        t.combinedCount = 0;
                        if (t.buySymbol.Contains("btc") || t.buySymbol.Contains("eth") || t.buySymbol.Contains("sol"))
                        {
                            t.feeSymbol = "usd";
                            t.feeAmount = 0.0045 * t.sellAmount;
                            t.altFeeSymbol = "";
                            t.altFeeAmount = 0;
                        }
                        ret.Add(t);
                    }

                    if (col[columnAction].Contains("Sell") ||
                        col[columnAction].Contains("STO") || // sell to open
                        col[columnAction].Contains("STC")) // sell to close
                    {
                        transaction t = new transaction();
                        t.buySymbol = "usd";
                        t.sellSymbol = col[columnSymbol].ToLower();
                        t.exchangeRec = "robinhood";
                        t.exchangeSent = "robinhood";
                        t.buyAmount = double.Parse(col[columnAmt]); //amount 
                        t.sellAmount = double.Parse(col[columnQty]); //quantity
                        t.dateTime = DateTime.Parse(col[columnDate]);
                        t.feeSymbol = "";
                        t.feeAmount = 0;
                        t.combinedCount = 0;
                        if(t.sellSymbol.Contains("btc") || t.sellSymbol.Contains("eth") || t.sellSymbol.Contains("sol"))
                        {
                            t.feeSymbol = "usd";
                            t.feeAmount = 0.0045 * t.buyAmount;
                            t.altFeeSymbol = "";
                            t.altFeeAmount = 0;
                        }
                        ret.Add(t);
                    }
                    }
                }
            }
            return ret;
        }

        static void Main(string[] args)
        {
            gaintaxlibrary.ClassGainTax.Go();
            var trans = readFile20ColumnCSV("../../trans20Column.csv", new DateTime(2020, 1, 1));
            var transSimple = readSimpleFile10ColumnCSV("../../transSimple10Column.csv", new DateTime(2020, 1, 1));
            foreach(var ts in transSimple)
            {
                trans.Add(ts);
            }
            
            string initialBuyFileName = "./initialBuys.xml";
            if(!File.Exists(initialBuyFileName))
            {
                List<transaction> initialBTCBuys = [
                 new transaction
                {
                    buyAmount = 1,
                    buySymbol = "btc",
                    sellAmount = 750.0 * 1,
                    sellSymbol = "usd",
                    dateTime = new DateTime(2013, 12, 2),
                    exchangeRec = "MtGox",
                    exchangeSent = "MtGox",
                    combinedCount = 0
                }
                ];
                WriteToXmlFile(initialBuyFileName, initialBTCBuys);
            }

            var initialBTCBuysData = ReadFromXmlFile<List<transaction> >(initialBuyFileName);

            foreach(var initialBuy in initialBTCBuysData)
            {
                trans.Add(initialBuy);
            }

            ClassGainTax gainTax = new ClassGainTax();
            gainTax.transactionsOriginal = trans;//new List<transaction>();
            gainTax.transactionsOriginal= gainTax.transactionsOriginal.OrderBy(x => x.dateTime.Value).ToList();

            List<bucket> buckets = new List<bucket>();

            gainTax.combineTransactionsInHourLongWindow_MODIFIES_transactions(gainTax.transactionsOriginal, 24, false, 0.5);
            gainTax.useAlternativeDateForSellSinceAfterBuy_MODIFIES_transactions(gainTax.transactionsOriginal);

            var realizedbtc = gainTax.computeGains(out buckets, true, "btc", "fiho", gainTax.transactionsOriginal);
            gainTax.printListListString(gainTax.summerizeBucketsToStringList(buckets), "\t", 24);
            gainTax.printListListString(gainTax.realizedTransToString(realizedbtc));

            Console.WriteLine("End");
        }
    }
}
