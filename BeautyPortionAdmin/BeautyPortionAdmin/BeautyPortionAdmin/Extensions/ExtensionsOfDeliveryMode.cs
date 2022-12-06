using System;
using BeautyPortionAdmin.Models;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfDeliveryMode
    {
        public static string ToDeliveryDescription(this DeliveryMode deliveryMode)
        {
            return deliveryMode switch
            {
                DeliveryMode.Delivery => "Entrega",
                DeliveryMode.Withdrawal => "Retirada",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
