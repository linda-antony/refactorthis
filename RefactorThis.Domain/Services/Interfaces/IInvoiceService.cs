using RefactorThis.Domain.Entities;
using System;

namespace RefactorThis.Domain.Services
{
    public interface IInvoiceService
    {
        string ProcessPayment(Payment payment);
    }
}
