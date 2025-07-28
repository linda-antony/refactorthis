using RefactorThis.Domain.Entities;

namespace RefactorThis.Domain.Services.Interfaces
{
    public interface ICalculateTaxService
    {
        decimal CalculateTax(InvoiceType type, decimal amount, bool anyPreviousPayment);
    }
}
