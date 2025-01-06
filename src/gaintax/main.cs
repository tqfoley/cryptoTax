using gaintaxlibrary;
using System.Dynamic;
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

        private static bool stringContainsIgnoreCase(this string source, string h)
        {
            return source.ToLower().Contains(h.ToLower());
        }

        private static List<Transaction> readFile20ColumnCSV(string filename = "../../../../../trans20Column.csv", DateTime? ignoreBeforeDate = null,
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

                                

            List<Transaction> ret = new List<Transaction>();

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
                    Console.WriteLine("bad did not expect");
                }
                else if (col[0] == "")
                {
                    //last line;
                    Console.WriteLine("bad did not expect");
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
                        else if (transType.stringMatchesIgnoreCase("wrap"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("multi_token_trade"))
                        {
                        }
                        else if (transType.stringMatchesIgnoreCase("Send")
                            || transType.stringMatchesIgnoreCase("Receive")
                            || transType.stringMatchesIgnoreCase("Transfer"))
                        {
                            /*Transfer t = new Transfer();
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

                                Transaction t = new Transaction();
                                t.dateTime = DateTime.Parse(col[columnDateTime]);
                                if (ignoreBeforeDate == null || t.dateTime.Date >= ignoreBeforeDate.Value.Date)
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
                                            Console.WriteLine("bad not expected");
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
                                            Console.WriteLine("bad did not expect");
                                        }
                                    }

                                    if (t.buySymbol.Contains("weth") && t.dateTime.Date.Day == 18)
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

                                    if (t.buySymbol.Contains("weth") && t.dateTime.Date.Day == 18 && t.feeAmount > 1111)
                                    {
                                        t.buySymbol = "weth";
                                    }


                                    if (t.buyAmount < 0.0000000001 || t.sellAmount < 0.0000000001)
                                    {
                                        Console.WriteLine("BAD " + t.dateTime + " " + t.sellSymbol + " " + t.buySymbol + " " + t.buyAmount);
                                    }
                                    else if (t.buyAmount / t.sellAmount > 1000111.0 || t.sellAmount / t.buyAmount > 1000111.0)
                                    {
                                        //pepe hits this
                                        Console.WriteLine("BAD " + t.dateTime + " " + t.sellSymbol + " " + t.buySymbol + " " + t.buyAmount);
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

        private static List<Transaction> readSimpleFile10ColumnCSV( string filename = "../../../../../transSimple10Column.csv", DateTime? ignoreBeforeDate = null,
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

            List<Transaction> ret = new List<Transaction>();
            string[] lines = File.ReadAllLines(filename);

            //double realizedgain = 0;
            int linenumber = 0;
            foreach (string line in lines)
            {
                string[] col = line.Split(",");
                linenumber++;

                if (col.Length < 9 || linenumber < 2)
                {
                    Console.WriteLine("bad not expected");
                }
                else
                {
                    
                    if (ignoreBeforeDate == null || DateTime.Parse(col[columnDate]) >= ignoreBeforeDate.Value.Date)
                    {
                    if (col[columnAction].Contains("Buy") ||
                        col[columnAction].Contains("BTO") || // buy to open
                        col[columnAction].Contains("BTC")) // buy to close
                    {
                        Transaction t = new Transaction();
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
                        Transaction t = new Transaction();
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

        private static List<Transaction> readFileLines( string filename = "../../../../../linetrades.txt")
        {

            List<Transaction> ret = new List<Transaction>();
            string[] lines = File.ReadAllLines(filename);

            //int linenumber = 0;
            int foundtransStep=0;
            string currentSymbol = "";
            bool currentbuy=false;
            float currAmount=0;
            float currCost = 0;
            
            string previousline = "";

            foreach (string line in lines)
            {
                if(line.Length > 0){
                    if(line.Contains("/") && line.ToLower().Contains("usd"))
                    {
                        foundtransStep = 1;
                        currentSymbol = line.Split("/").First();
                    }
                    if(line.ToLower().Contains("long"))
                    {
                        if(previousline.ToLower().Contains("perp")){
                            currentSymbol = previousline;
                            foundtransStep = 2;
                        }
                    }
                    
                    if(line.ToLower().Contains("short"))
                    {
                        if(previousline.ToLower().Contains("perp")){
                            currentSymbol = previousline;
                            foundtransStep = 2;
                        }
                    }
                }
                if(foundtransStep>0){
                    if(foundtransStep == 1){
                        foundtransStep = 2;
                    }
                    else if(foundtransStep==2){
                        if(line.ToLower().Contains("buy") || line.ToLower().Contains("long")){
                            currentbuy = true;
                        }
                        if(line.ToLower().Contains("sell") || line.ToLower().Contains("short")){
                            currentbuy = false;
                        }
                        foundtransStep++;
                    }
                    else if(foundtransStep==3)
                    {
                        string[] a = line.Split(" ");
                        currAmount = float.Parse(a[0]);
                        foundtransStep++;
                    }
                    else if(foundtransStep == 4){

                        string[] a = line.Split("$");
                        currCost = float.Parse(a[1]);
                        foundtransStep = 5;
                    }

                    else if(foundtransStep==5)
                    {
                        foundtransStep = 0;


                        Transaction t = new Transaction();
                        if(currentbuy)
                        {
                            t.buySymbol = currentSymbol;
                            t.sellSymbol = "usd";
                            t.exchangeRec = "drift";
                            t.exchangeSent = "drift";
                            t.buyAmount = currAmount;
                            t.sellAmount = currCost;
                        }
                        if(!currentbuy)
                        {
                            t.buySymbol = "usd";
                            t.sellSymbol = currentSymbol;
                            t.exchangeRec = "drift";
                            t.exchangeSent = "drift";
                            t.buyAmount = currCost;
                            t.sellAmount = currAmount;
                        
                        }
                        t.dateTime = new DateTime(2024,12,3);
                        t.feeSymbol = "";
                        t.feeAmount = 0;
                        t.combinedCount = 0;
                        ret.Add(t);
                    }
                }
                previousline = line;
            }
            return ret;
        }


        static private void addUsdBuyTransaction(List<Transaction> t, string symbol, DateTime d, double tokenBuyAmount, double dollars, string exchange="exchange")
        {
            t.Add(new Transaction
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

        static void Main(string[] args)
        {
            gaintaxlibrary.ClassGainTax.Go();
            
            var trans = readFile20ColumnCSV();

            var transFromLines = readFileLines();
            foreach(var ts in transFromLines){
                trans.Add(ts);
            }

            var transSimple = readSimpleFile10ColumnCSV();
            foreach(var ts in transSimple)
            {
                trans.Add(ts);
            }
            
            string initialBuyFileName = "../../../../../initialBuysDefault.xml";
            if(!File.Exists(initialBuyFileName))
            {
                List<Transaction> initialBTCBuys = [
                new Transaction
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

            var initialBTCBuysData = ReadFromXmlFile<List<Transaction> >(initialBuyFileName);

            foreach(var initialBuy in initialBTCBuysData)
            {
                trans.Add(initialBuy);
            }

            ClassGainTax gainTax = new ClassGainTax();
            gainTax.transactionsOriginal = trans;//new List<Transaction>();
            gainTax.transactionsOriginal= gainTax.transactionsOriginal.OrderBy(x => x.dateTime).ToList();

            List<Bucket> buckets = new List<Bucket>();

            gainTax.combineTransactionsInHourLongWindow_MODIFIES_transactions(gainTax.transactionsOriginal, 1, false, 0.5);

            List<string> uniqueSymbols = new List<string>();
            //List<string> uniqueSymbols1 = gainTax.transactionsOriginal.GroupBy(x => x.buySymbol).Select(x => x.First().buySymbol).ToList();
            foreach(var symbols in gainTax.transactionsOriginal)
            {
                if(!uniqueSymbols.Exists(x => x == symbols.buySymbol))
                {
                    if(!symbols.buySymbol.Contains("deleted"))
                    {
                        uniqueSymbols.Add(symbols.buySymbol);
                    }
                }
                if(!uniqueSymbols.Exists(x => x == symbols.sellSymbol))
                {
                    if(!symbols.sellSymbol.Contains("deleted"))
                    {
                        uniqueSymbols.Add(symbols.sellSymbol);
                    }
                }
            }

            List<string>  symbolsICareAbout;
            symbolsICareAbout = new List<string>();
            symbolsICareAbout.Add("btc");

            symbolsICareAbout = symbolsICareAbout.OrderBy(x => x).ToList();
            foreach(var f in symbolsICareAbout){
                Console.WriteLine("x != \"" + f + "\" &&");
            }

            foreach (var symbol in symbolsICareAbout)
            {
                var realizedToken = gainTax.computeGains(out buckets, true, symbol, "fiho", gainTax.transactionsOriginal);

                Console.WriteLine(symbol);

                // Count the number of transactions where x.trans is not null and the year contains "2024"
                var filteredTransactions = realizedToken.Where(x => x.trans != null && x.trans.dateTime.Year.ToString().Contains("2024"));
                Console.WriteLine(symbol + " Count: " + filteredTransactions.Count());

                Console.WriteLine(symbol);

                // If there are more than 5 transactions for the year 2024
                if (filteredTransactions.Count() > 5)
                {
                    List<List<KeyValuePair<string, string>>> g = gainTax.realizedTransToKeyValStringString(realizedToken);
                    gainTax.printListListString(gainTax.summerizeBucketsToStringList(buckets), "\t", 24);
                    gainTax.printListListKeyValueStringString(g);
                }
            }

            Console.WriteLine("End");
        }
    }
}
