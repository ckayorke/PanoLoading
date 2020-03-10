using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PanoLoading.Models
{   
    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Range(0, 3)]
        [Display(Name = "Permission Level")]
        public int Permission { get; set; }

        [Display(Name = "Permission")]
        public string PermissionName { get; set; }
        public List<SelectListItem> PermissionNameList { get; set; }
    }   
}