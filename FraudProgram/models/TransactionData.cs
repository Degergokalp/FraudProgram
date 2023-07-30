using System;

public class TransactionData
{
    // public int Index { get; set; }
     public int Id { get; set; }
     public Guid ShoppingFileId { get; set; }
     public string CC_CardNumber { get; set; }
     public string CC_CardHolder { get; set; }
     public bool CC_Is3DPayment { get; set; }
     public string Payment_Currency { get; set; }
     public decimal Payment_TotalAmount { get; set; }
     public int BusinessID { get; set; }
     public DateTime CreationDate { get; set; }
     public string Name { get; set; }
     public string BankName { get; set; }
}
