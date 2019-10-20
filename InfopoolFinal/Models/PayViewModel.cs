using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfopoolFinal.Models
{
    public class PayViewModel
    {
        public string Trxid { get; set; }
        public int AdId { get; set; }
        public int Userid { get; set; }
        public double Amount { get; set; }
        public string EmailFromView { get; set; }

    }
}