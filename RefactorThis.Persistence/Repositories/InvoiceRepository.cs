using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Repositories;

namespace RefactorThis.Persistance.Repostories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        public void SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database
        }

        public Invoice Add(Invoice invoice)
        {
            _invoice = invoice;
            return _invoice;
        }
    }
}