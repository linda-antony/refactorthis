using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Services.Interfaces;
using System;

namespace RefactorThis.Domain.Services
{
    public class CalculateTaxService : ICalculateTaxService
    {
        public decimal CalculateTax(InvoiceType type, decimal amount, bool anyPreviousPayment)
        {
            switch (type)
            {
                case InvoiceType.Commercial:
                    return amount * 0.14m;

                case InvoiceType.Standard:
                    return anyPreviousPayment ? amount : amount * 0.14m;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unsupported invoice type");
            }
        }
    }
}
