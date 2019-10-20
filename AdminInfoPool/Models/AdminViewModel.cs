using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace InfopoolFinal.Models
{
    public class AdminViewModel
    {
        [Required]
        [Display(Name = "adminUserName")]
        
        public string adminUserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "adminPass")]
        public string adminPass { get; set; }
       


        //[Display(Name = "view")]
        //public string view { get; set; }
    }
    public class AddLinkViewModel
    {

        
        
        public string newsHeadline { get; set; }

        public string newsLink { get; set; }

        public int newsSource { get; set; }
        public int newsType { get; set; }
        public string AdminNameFromView { get; set; }




    }
    public class AddEventViewModel
    {

        public AddEventViewModel()
        {
            file = new List<HttpPostedFileBase>();
        }
        public string eventTitle { get; set; }


        
        [DataType(DataType.Date)]
        public DateTime? eventDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime? eventTime { get; set; }


        public string eventVenue { get; set; }
        
        public string eventDetail { get; set; }
        public string AdminNameFromView { get; set; }
        public List<HttpPostedFileBase> file { get; set; }

    }
    public class RemoveEventViewModel
    {

        public int eventId { get; set; }
        public string eventTitle { get; set; }

       
        public DateTime eventDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime eventTime { get; set; }


        public string eventVenue { get; set; }

        public string eventDetail { get; set; }
        
    }
 

}