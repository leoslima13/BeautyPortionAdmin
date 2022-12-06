using System;
using System.Collections.Generic;
using System.Text;

namespace BeautyPortionAdmin.Models
{
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public byte[] Photo { get; set; }
        public ClientIsFrom ClientIsFrom { get; set; }
    }

    public enum ClientIsFrom
    {
        Instagram,
        Facebook,
        Whatsapp,
        Indication,
    }
}
