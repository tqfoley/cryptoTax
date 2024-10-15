using gaintaxlibrary;


class GainTaxClass
{

    static public void addMyTransactions(List<transaction> mytrans)
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

    static void Main(string[] args)
    {
        Console.WriteLine("GOOO");
        gaintaxlibrary.ClassGainTax.Go();



        //addMyTransactions();

    }
}
