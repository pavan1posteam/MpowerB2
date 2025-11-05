using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPower.Models
{
    public class ExportProduct
    {
        public int storeid { set; get; }
        public string upc { set; get; }
        public long qty { set; get; }
        public string sku { set; get; }
        public Int32 pack { set; get; }
        public string uom { get; set; }
        public string StoreProductName { set; get; }
        public string Storedescription { set; get; }
        public decimal price { set; get; }
        public decimal sprice { set; get; }
        public string start { set; get; }
        public string end { set; get; }
        public decimal tax { set; get; }
        public string altupc1 { set; get; }
        public string altupc2 { set; get; }
        public string altupc3 { set; get; }
        public string altupc4 { set; get; }
        public string altupc5 { set; get; }
        public string deposit { get; set; }
    }
}
