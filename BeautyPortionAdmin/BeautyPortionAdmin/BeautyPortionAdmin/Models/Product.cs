using System;
namespace BeautyPortionAdmin.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public double Price { get; set; }
        public byte[] ProductPhoto { get; set; }
    }
}
