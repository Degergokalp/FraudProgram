using System;
using System.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

class Program
{
    private IConfiguration configuration;


    public IConfiguration ConfigFile()
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddIniFile("config.ini")
            .Build();

        return configuration;
    }

    static void Main()
    {
        IEmailSender emailSender = new EmailSender();
        IDataProvider dataProvider = new SqlDataProvider();
        IBoundaryIndexProvider boundaryIndexProvider = new BoundaryIndexProviderFromConfig();

        var rule1 = new Rule1(boundaryIndexProvider, emailSender, dataProvider);
          // Get the current boundary index from the provider
        int currentBoundaryIndex = boundaryIndexProvider.Read();

        // Increment the boundary index
        int newBoundaryIndex = currentBoundaryIndex + 1;
        
        boundaryIndexProvider.Write(newBoundaryIndex);

        var rules = new List<IRule>();
        rules.Add(rule1);

        var processor = new RuleProcessor(rules);
        processor.ProcessRules();

    }
}
