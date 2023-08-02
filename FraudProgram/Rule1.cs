using System.Data;
using Microsoft.Extensions.Configuration;

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

    private IConfiguration configuration;


    public IConfiguration ConfigFile()
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddIniFile("config.ini")
            .Build();

        return configuration;
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
            int transactionLimit = int.Parse(configuration["Inputs:transaction_limit"]);
            int moneyLimit = int.Parse(configuration["Inputs:money_limit"]);
            
            string email_user=configuration["Mail Settings:email_user"];
            string email_send=configuration["Mail Settings:email_send"];

            if (transactionCount > transactionLimit )
            {
                if (totalMoney > moneyLimit)
                {
                    string subject = "Fraud Alert!/Tutar ve işlem limiti aşıldı";
                    string body = "Cariye para yükleme,\nBelirlenen işlem sayısı eşiğini ve toplam tutar miktarını aşmıştır,\n";
                    body += $"{transactionCount} kere işlem yapmaya çalışmış,\nYapılan toplam cariye para yükleme miktarı: {totalMoney}. BusinessId: {transactionId}";


                    _emailSender.SendEmailAsync(email_user, "your_password", email_send, subject, body);
                    // CreateTicket();
                }
                else
                {
                    string subject = "Fraud Alert!/İşlem limiti aşıldı";
                    string body = "Cariye para yükleme,\nBelirlenen işlem sayısı eşiğini aşmıştır,\n";
                    body += $"{transactionCount} kere işlem yapmaya çalışmış,\nYapılan toplam cariye para yükleme miktarı: {totalMoney}.  BusinessId: {transactionId}";


                    _emailSender.SendEmailAsync(email_user, "your_password", email_send, subject, body);
                    // CreateTicket();
                }
            }
            else if (totalMoney >= moneyLimit)
            {
                string subject = "Fraud Alert!/Tutar limiti aşıldı";
                string body = "Cariye para yükleme,\nBelirlenen toplam tutar miktarını aşmıştır,\n";
                body += $"Yapılan toplam cariye para yükleme miktarı: {totalMoney}.  BusinessId: {transactionId}";


                _emailSender.SendEmailAsync(email_user, "your_password",email_send, subject, body);
                // CreateTicket();
            }
        

        // Increment the boundary index
        int newBoundaryIndex = lastIndex + 1;

        // Write the new boundary index to the provider
        _boundaryIndexProvider.Write(newBoundaryIndex);
        }
    }
}