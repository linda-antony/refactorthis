using RefactorThis.Domain.Repositories;
using RefactorThis.Domain.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using RefactorThis.Domain.Services.Interfaces;

namespace RefactorThis.Domain.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICalculateTaxService _taxService;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            ICalculateTaxService calculateTaxService)
        {
            _invoiceRepository = invoiceRepository;
            _taxService = calculateTaxService;
        }

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            if (invoice.Amount == 0)
            {
                if (invoice.Payments == null || !invoice.Payments.Any())
                    return "no payment needed";
                else
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            var totalPaid = invoice.Payments?.Sum(p => p.Amount) ?? 0m;
            var amountRemaining = invoice.Amount - totalPaid;

            if (totalPaid != 0 && totalPaid == invoice.Amount)
                return "invoice was already fully paid";

            if (payment.Amount > amountRemaining)
                return totalPaid != 0
                    ? "the payment is greater than the partial amount remaining"
                    : "the payment is greater than the invoice amount";

            bool isFinalPayment = payment.Amount == amountRemaining;
            bool anyPreviousPayment = invoice.Payments.Any();

            ApplyPayment(invoice, payment, anyPreviousPayment);

            string paymentStatus = totalPaid == 0
                ? (isFinalPayment ? "invoice is now fully paid" : "invoice is now partially paid")
                : (isFinalPayment ? "final partial payment received, invoice is now fully paid" : "another partial payment received, still not fully paid");

            _invoiceRepository.SaveInvoice(invoice);

            return paymentStatus;
        }

        private void ApplyPayment(Invoice invoice, Payment payment, bool anyPreviousPayment)
        {
            if (invoice.Payments == null)
                invoice.Payments = new List<Payment>();

            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);

            invoice.TaxAmount += _taxService.CalculateTax(invoice.Type, payment.Amount, anyPreviousPayment);
        }
    }
}