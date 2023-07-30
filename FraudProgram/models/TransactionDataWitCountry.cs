public class TransactionDataWitCountry
{   
    public int Index {get;set; }
    public Guid Id { get; set; }
    public int ShoppingFileId {get; set;}
    public int BusinessId { get; set; }
    public string CC_CardNumber { get; set; }
    public string CC_CardHolder { get; set; }
    public bool CC_Is3DPayment { get; set; }
    public string Payment_Currency { get; set; }
    public decimal Payment_TotalAmount { get; set; }
    public bool If_LastCommandFailed { get; set; }
    public bool Has_PreAuth { get; set; }
    public bool Has_PostAuth { get; set; }
    public bool Has_Void { get; set; }
    public bool Has_Credit { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Nationality { get; set; }
    public string PassportCountry { get; set; }
    public string VATFlag { get; set; }
    public string Flight_TripType { get; set; }
    public string BankName { get; set; }
    public string BookingCode { get; set; }
    public string Name { get; set; }

}
