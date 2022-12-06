using System;
namespace BeautyPortionAdmin.Models
{
    public class Fraction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public double Weight { get; set; }
        public double Price { get; set; }
    }
}
