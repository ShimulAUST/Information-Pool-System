using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfopoolFinal.Models
{
    
    public class BlogViewModel
    {
        
        public BlogViewModel()
        {
            file = new List<HttpPostedFileBase>();
        }
        public List<HttpPostedFileBase> file { get; set; }
        public string BlogTitle { get; set; }
        public string BlogBody { get; set; }
        public int BlogType { get; set; }
        public string EmailFromView { get; set; }
        public int BlogId { get; set; }
        public int BloggerId { get; set; }

        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public string Tag3 { get; set; }
        public string Tag4 { get; set; }
        

        //public byte ApprovalStatus { get; set; }
    }
   

}