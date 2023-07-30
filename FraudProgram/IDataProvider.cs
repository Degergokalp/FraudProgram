using System;
using System.Data;

// Define the interface
public interface IDataProvider
{
  
    List<TransactionData> ListTransactions(int id, DateTime timeLimit);
    List<TransactionDataWitCountry> ListTransactionsWithCountry(int boundaryIndex);
}
