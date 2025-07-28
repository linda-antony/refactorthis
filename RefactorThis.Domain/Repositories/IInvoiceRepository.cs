using RefactorThis.Domain.Entities;

namespace RefactorThis.Domain.Repositories
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string reference);
        void SaveInvoice(Invoice invoice);
        Invoice Add(Invoice invoice);
    }
}
