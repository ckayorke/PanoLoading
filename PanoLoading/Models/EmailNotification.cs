using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PanoLoading.Models
{   
    public class EmailNotification
    {
        public int Id { get; set; }
        public string Email { get; set; }        
        public string Login { get; set; }        
    }   
}