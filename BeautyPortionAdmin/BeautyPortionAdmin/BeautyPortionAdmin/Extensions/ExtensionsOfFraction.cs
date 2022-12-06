using System;
using BeautyPortionAdmin.Models;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfFraction
    {
        public static FractionInOrder ToFractionInOrder(this Fraction fraction, int quantity)
        {
            return new FractionInOrder
            {
                Id = fraction.Id,
                Price = fraction.Price,
                ProductId = fraction.ProductId,
                Weight = fraction.Weight,
                Quantity = quantity
            };
        }
    }
}
