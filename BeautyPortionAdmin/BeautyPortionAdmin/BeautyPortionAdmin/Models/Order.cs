using System;
using System.Collections.Generic;

namespace BeautyPortionAdmin.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; }
        public IEnumerable<Guid> ProductIds { get; set; }
        public IEnumerable<FractionInOrder> Fractions { get; set; }
        public double Freight { get; set; }
        public PaymentType PaymentType { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class FractionInOrder
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public double Price { get; set; }
        public double Weight { get; set; }
        public int Quantity { get; set; }
    }

    public enum DeliveryMode
    {
        Withdrawal,
        Delivery
    }

    public enum PaymentType
    {
        CreditCard,
        Money,
        Pix,
        BankTransfer
    }

    public enum OrderStatus
    {
        Pending,
        InProgress,
        Done,
        Delivered
    }

    public enum PaymentStatus
    {
        Pending,
        Approved
    }
}
