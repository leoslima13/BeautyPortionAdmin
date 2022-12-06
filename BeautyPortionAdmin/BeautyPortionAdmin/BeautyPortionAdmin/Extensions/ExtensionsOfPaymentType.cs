using System;
using BeautyPortionAdmin.Models;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfPaymentType
    {
        public static string ToPaymentDescription(this PaymentType paymentType)
        {
            return paymentType switch
            {
                PaymentType.BankTransfer => "Transferência Bancáriaa",
                PaymentType.CreditCard => "Cartão de Crédito",
                PaymentType.Money => "Dinheiro",
                PaymentType.Pix => "Pix",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
