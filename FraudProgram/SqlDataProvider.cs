using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class SqlDataProvider : IDataProvider
{
    private IConfiguration configuration;
    private string connectionString;

    public SqlDataProvider()
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddIniFile("config.ini")
            .Build();

        connectionString = GetConnectionString();
    }

    public string GetConnectionString()
    {
        return configuration["Database Connection:connection_string"];
    }


    private static String SQL_ListTransactionsWithCountry = @"
SELECT TOP(100) p1.[Index], p1.[Id], p1.[ShoppingFileId], p1.[CC_CardNumber], p1.[CC_CardHolder], p1.[CC_Is3DPayment], p1.[Payment_Currency], p1.[Payment_TotalAmount],
       p1.If_LastCommandFailed, p1.Has_PreAuth, p1.Has_PostAuth, p1.Has_Void, p1.Has_Credit, p1.BusinessId
INTO #temppayments
FROM [Sale].[ShoppingFile_Payments] p1 WITH (NOLOCK)
WHERE p1.[index] > @BoundaryIndex AND p1.PaymentType = 'CC' AND EXISTS (
    SELECT TOP 1 1
    FROM [Sale].[ShoppingFile_Products] pr WITH (NOLOCK)
    WHERE pr.ShoppingFileId = p1.ShoppingFileId AND pr.ProductType IN (1, 5) AND pr.ImportedDate IS NULL
)
ORDER BY p1.[Index] ASC;

SELECT DISTINCT pr.VATFlag, pr.Flight_TripType, pr.ShoppingFileId, pr.BookingCode
INTO #tempproductInfo
FROM [Sale].[ShoppingFile_Products] pr WITH (NOLOCK)
INNER JOIN #temppayments p WITH (NOLOCK) ON p.ShoppingFileId = pr.ShoppingFileId;

SELECT p.*, ps.FirstName, ps.LastName, ps.Nationality, ps.PassportCountry,
       (
           SELECT STUFF((
               SELECT fs.Origin + '-' + fs.Destination + ';'
               FROM [Sale].[ShoppingFile_FlightSegments] fs WITH (NOLOCK)
               WHERE p.ShoppingFileId = fs.ShoppingFileId
               FOR XML PATH('')
           ), 1, 0, '') AS [SegmentInfo]
       ),
       (
           SELECT TOP 1 pr.VATFlag
           FROM #tempproductInfo pr WITH (NOLOCK)
           WHERE pr.ShoppingFileId = p.ShoppingFileId
       ) AS VATFlag,
       (
           SELECT TOP 1 pr.Flight_TripType
           FROM #tempproductInfo pr WITH (NOLOCK)
           WHERE pr.ShoppingFileId = p.ShoppingFileId
       ) AS Flight_TripType,
       bin.[BankName] AS BankName,
       (
           SELECT TOP 1 pr.BookingCode
           FROM #tempproductInfo pr WITH (NOLOCK)
           WHERE pr.ShoppingFileId = p.ShoppingFileId
       ) AS BookingCode,
       (SELECT TOP 1 b.Name FROM [Accounts].[Businesses] b WITH (NOLOCK) WHERE b.Id = p.BusinessId) AS Name
FROM #temppayments p WITH (NOLOCK)
INNER JOIN [Sale].[ShoppingFile_Passengers] ps WITH (NOLOCK) ON p.ShoppingFileId = ps.ShoppingFileId
LEFT JOIN [Trevoo].[dbo].[CC_BinList_TR] bin WITH (NOLOCK) ON SUBSTRING(p.CC_CardNumber, 1, 4) + SUBSTRING(p.CC_CardNumber, 6, 2) = bin.[BinNumber];

DROP TABLE #temppayments;
DROP TABLE #tempproductInfo;
";

    public List<TransactionDataWitCountry> ListTransactionsWithCountry(int boundaryIndex)
    {
        SqlParameter[] parms = new SqlParameter[]{
                new SqlParameter("BoundaryIndex", SqlDbType.Int),
            };
        parms[0].Value = boundaryIndex;


        List<TransactionDataWitCountry> liste = new List<TransactionDataWitCountry>();
        using (SqlDataReader reader = SQLHelper.ExecuteReader(connectionString, CommandType.Text, SQL_ListTransactionsWithCountry, parms))
        {
            while (reader.Read())
            {
                var item = new TransactionDataWitCountry();
                item.Index = reader.GetInt32("Index");
                item.Id = reader.GetGuid("Id");
                item.ShoppingFileId = reader.GetInt32("ShoppingFileId");
                item.CC_CardNumber = reader.GetString("CC_CardNumber");
                item.CC_CardHolder = reader.GetString("CC_CardHolder");
                item.CC_Is3DPayment = reader.GetBoolean("CC_Is3DPayment"); // Assuming it's a boolean
                item.Payment_Currency = reader.GetString("Payment_Currency");
                item.Payment_TotalAmount = reader.GetDecimal("Payment_TotalAmount"); // Assuming it's a decimal
                item.If_LastCommandFailed = reader.GetBoolean("If_LastCommandFailed"); // Assuming it's a boolean
                item.Has_PreAuth = reader.GetBoolean("Has_PreAuth"); // Assuming it's a boolean
                item.Has_PostAuth = reader.GetBoolean("Has_PostAuth"); // Assuming it's a boolean
                item.Has_Void = reader.GetBoolean("Has_Void"); // Assuming it's a boolean
                item.Has_Credit = reader.GetBoolean("Has_Credit"); // Assuming it's a boolean
                item.BusinessId = reader.GetInt32("BusinessId");
                item.FirstName = reader.GetString("FirstName");
                item.LastName = reader.GetString("LastName");
                item.Nationality = reader.GetString("Nationality");
                item.PassportCountry = reader.GetString("PassportCountry");
                item.VATFlag = reader.GetString("VATFlag");
                item.Flight_TripType = reader.GetString("Flight_TripType");
                item.BankName = reader.GetString("BankName");
                item.BookingCode = reader.GetString("BookingCode");
                item.Name = reader.GetString("Name");

                liste.Add(item);
            }
            return liste;
        }
    }


    private static string SQL_ListTransactions = @"
SELECT p.[Index], p.Id, p.ShoppingFileId, p.BusinessID, p.CC_CardHolder, p.CC_CardNumber, p.CC_Is3DPayment, p.Payment_Currency, p.Payment_TotalAmount, pr.CreationDate, 
       (SELECT TOP 1 b.Name FROM [Accounts].[Businesses] b WITH (NOLOCK) WHERE b.Id = p.BusinessId) AS Name 
INTO #cariye 
FROM [Trevoo].[Sale].[ShoppingFile_Payments] p (NOLOCK) 
INNER JOIN [Sale].[ShoppingFile_Products] pr (NOLOCK) ON p.ShoppingFileId = pr.ShoppingFileId 
WHERE pr.ProductType = 5 AND pr.CreationDate > @TimeLimit
  AND p.BusinessId = @Id

SELECT p.Id, p.ShoppingFileId, p.BusinessID, p.CC_CardHolder, p.CC_CardNumber, p.CC_Is3DPayment, p.Payment_Currency, p.Payment_TotalAmount, p.CreationDate, p.[Name], bin.[BankName] AS BankName 
FROM #cariye AS p WITH (NOLOCK) 
LEFT JOIN [Trevoo].[dbo].[CC_BinList_TR] bin WITH (NOLOCK) ON SUBSTRING(p.CC_CardNumber, 1, 4) + SUBSTRING(p.CC_CardNumber, 6, 2) = bin.[BinNumber] 
ORDER BY p.[Index] DESC

DROP TABLE #cariye";

    public List<TransactionData> ListTransactions(int id, DateTime timeLimit)
    {
        SqlParameter[] parms = new SqlParameter[]{
                new SqlParameter("Id", SqlDbType.Int),
                new SqlParameter("TimeLimit", SqlDbType.DateTime),
            };
        parms[0].Value = id;
        parms[1].Value = timeLimit;


        List<TransactionData> liste = new List<TransactionData>();
        using (SqlDataReader reader = SQLHelper.ExecuteReader(connectionString, CommandType.Text, SQL_ListTransactions, parms))
        {
            while (reader.Read())
            {
                var item = new TransactionData();
                item.Id = reader.GetInt32("Id");
                item.ShoppingFileId = reader.GetGuid("ShoppingFileId");
                item.BusinessID = reader.GetInt32("BusinessID");
                item.CC_CardHolder = reader.GetString("CC_CardHolder");
                item.CC_CardNumber = reader.GetString("CC_CardNumber");
                item.CC_Is3DPayment = reader.GetBoolean("CC_Is3DPayment");
                item.Payment_Currency = reader.GetString("Payment_Currency");
                item.Payment_TotalAmount = reader.GetDecimal("Payment_TotalAmount");
                item.CreationDate = reader.GetDateTime("CreationDate");
                item.Name = reader.GetString("Name");
                item.BankName = reader.GetString("BankName");
                liste.Add(item);
            }
            return liste;
        }
    }
}
