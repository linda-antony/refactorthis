
namespace RefactorThis.Domain.Entities
{
    public class Payment
    {
        public Payment() { }

        public Payment(decimal amount, string reference)
        {
            Amount = amount;
            Reference = reference;
        }

        public decimal Amount { get; private set; }
        public string Reference { get; private set; }
    }
}