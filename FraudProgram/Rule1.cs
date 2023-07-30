using System.Data;

public class Rule1 : IRule
{
    private readonly IEmailSender _emailSender;
    private readonly IDataProvider _dataProvider;
    private readonly IBoundaryIndexProvider _boundaryIndexProvider;
    public Rule1(IBoundaryIndexProvider boundaryIndexProvider, IEmailSender emailSender, IDataProvider dataProvider)
    {
        _emailSender = emailSender;
        _dataProvider = dataProvider;
        _boundaryIndexProvider = boundaryIndexProvider;
    }

    public void Execute()
    {
        DateTime timeLimit = DateTime.Now;
        int lastIndex = _boundaryIndexProvider.Read();
        
        var transactionsWithCountry = _dataProvider.ListTransactionsWithCountry(lastIndex);


        foreach (TransactionDataWitCountry transaction in transactionsWithCountry)
        {
            int transactionId = transaction.BusinessId;

            var totalTransaction = _dataProvider.ListTransactions(transactionId, timeLimit);
            int transactionCount = totalTransaction.Count;
            decimal totalMoney = 0;

            foreach (TransactionData row in totalTransaction)
            {
                decimal money = row.Payment_TotalAmount;
                totalMoney += money;
                
            }

            if (transactionCount > 10)
            {
                if (totalMoney > 1000)
                {
                    string subject = "Fraud Alert!/Tutar ve işlem limiti aşıldı";
                    string body = "Cariye para yükleme,\nBelirlenen işlem sayısı eşiğini ve toplam tutar miktarını aşmıştır,\n";
                    body += $"{transactionCount} kere işlem yapmaya çalışmış,\nYapılan toplam cariye para yükleme miktarı: {totalMoney} .";


                    _emailSender.SendEmailAsync("from@example.com", "your_password", "to@example.com", subject, body);
                    // CreateTicket();
                }
                else
                {
                    string subject = "Fraud Alert!/İşlem limiti aşıldı";
                    string body = "Cariye para yükleme,\nBelirlenen işlem sayısı eşiğini aşmıştır,\n";
                    body += $"{transactionCount} kere işlem yapmaya çalışmış,\nYapılan toplam cariye para yükleme miktarı: {totalMoney} .";


                    _emailSender.SendEmailAsync("from@example.com", "your_password", "to@example.com", subject, body);
                    // CreateTicket();
                }
            }
            else if (totalMoney >= 1000)
            {
                string subject = "Fraud Alert!/Tutar limiti aşıldı";
                string body = "Cariye para yükleme,\nBelirlenen toplam tutar miktarını aşmıştır,\n";
                body += $"Yapılan toplam cariye para yükleme miktarı: {totalMoney} .";


                _emailSender.SendEmailAsync("from@example.com", "your_password", "to@example.com", subject, body);
                // CreateTicket();
            }

        }
    }
}