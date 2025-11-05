using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPower.Models
{
    internal class FrequentModel
    {
        public string Storeid { get; set; }
        public decimal currentfs { get; set; }
        public decimal cdollars { get; set; }
        public decimal points2c { get; set; }
        public string loyaltyno { get; set; }
    }
    public class FrequentResult
    {
        public decimal Points { get; set; }
        public List<PhoneNumber1> PhoneNumbers { get; set; }
    }
    public class PhoneNumber1
    {
        public string Number { get; set; }
    }
}
