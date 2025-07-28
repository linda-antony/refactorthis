using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Domain.Services;
using RefactorThis.Domain.Repositories;
using RefactorThis.Domain.Entities;
using Moq;

namespace RefactorThis.Domain.Tests
{
    public class InvoicePaymentProcessorTests
    {
        private InvoiceService _invoiceService;
        private Mock<IInvoiceRepository> _mockInvoiceRepository;

        public InvoicePaymentProcessorTests()
        {
            _mockInvoiceRepository = new Mock<IInvoiceRepository>();
            _mockInvoiceRepository.Setup(x => x.GetInvoice(""))
                .Returns((Invoice)null);

            _invoiceService = new InvoiceService(_mockInvoiceRepository.Object, new CalculateTaxService());
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            // arrange
            var payment = new Payment();
            var failureMessage = "";

            // act
            try
            {
                var result = _invoiceService.ProcessPayment(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            // assert
            Assert.AreEqual("There is no invoice matching this payment", failureMessage);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            // arrange
            var invoice = new Invoice()
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            _mockInvoiceRepository.Setup(x => x.Add(invoice))
                .Returns(invoice);

            _mockInvoiceRepository.Setup(x => x.GetInvoice("reference"))
                .Returns(invoice);

            // act
            var payment = new Payment(0, "reference");
            var result = _invoiceService.ProcessPayment(payment);

            // assert
            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            // arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment(10, "reference")
                }
            };

            _mockInvoiceRepository.Setup(x => x.Add(invoice))
               .Returns(invoice);
            _mockInvoiceRepository.Setup(x => x.GetInvoice("reference"))
                .Returns(invoice);

            // act
            var payment = new Payment(10, "reference");
            var result = _invoiceService.ProcessPayment(payment);

            // assert
            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            // arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment(5, "reference")
                }
            };

            _mockInvoiceRepository.Setup(x => x.Add(invoice))
              .Returns(invoice);
            _mockInvoiceRepository.Setup(x => x.GetInvoice("reference"))
                .Returns(invoice);

            // act
            var payment = new Payment(6, "reference");
            var result = _invoiceService.ProcessPayment(payment);

            // assert
            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            // arrange
            var invoice = new Invoice()
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            _mockInvoiceRepository.Setup(x => x.Add(invoice))
             .Returns(invoice);
            _mockInvoiceRepository.Setup(x => x.GetInvoice("reference"))
                .Returns(invoice);

            // act
            var payment = new Payment(6, "reference");
            var result = _invoiceService.ProcessPayment(payment);

            // assert
            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            // arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment(5, "reference")
                }
            };

            _mockInvoiceRepository.Setup(x => x.Add(invoice))
             .Returns(invoice);
            _mockInvoiceRepository.Setup(x => x.GetInvoice("reference"))
                .Returns(invoice);

            // act
            var payment = new Payment(5, "reference");
            var result = _invoiceService.ProcessPayment(payment);

            // assert
            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            // arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>() { new Payment(10, "reference") }
            };

            _mockInvoiceRepository.Setup(x => x.Add(invoice))
             .Returns(invoice);
            _mockInvoiceRepository.Setup(x => x.GetInvoice("reference"))
                .Returns(invoice);

            // act
            var payment = new Payment(10, "reference");
            var result = _invoiceService.ProcessPayment(payment);

            // assert
            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            // arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment(5, "reference")
                }
            };

            _mockInvoiceRepository.Setup(x => x.Add(invoice))
             .Returns(invoice);
            _mockInvoiceRepository.Setup(x => x.GetInvoice("reference"))
                .Returns(invoice);

            // act
            var payment = new Payment(1, "reference");
            var result = _invoiceService.ProcessPayment(payment);

            // assert
            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            // arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            _mockInvoiceRepository.Setup(x => x.Add(invoice))
            .Returns(invoice);
            _mockInvoiceRepository.Setup(x => x.GetInvoice("reference"))
                .Returns(invoice);

            // act
            var payment = new Payment(1, "reference");
            var result = _invoiceService.ProcessPayment(payment);

            // assert
            Assert.AreEqual("invoice is now partially paid", result);
        }
    }
}
