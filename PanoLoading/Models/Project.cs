using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PanoLoading.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string Creator { get; set; }
        public int Levels { get; set; }
        public int Rooms { get; set; }
        public int ProjectStatus { get; set; }
        public string Fielder { get; set; }
        public string Drawer { get; set; }
        public string StatusName{ get; set; }
        public List<SelectListItem> ProjectStates { get; set; }
        public List<SelectListItem> Fielders { get; set; }
        public List<SelectListItem> Drawers { get; set; }
        public List<SelectListItem> StatusNameList { get; set; }
        public List<string> OutPicUrl { get; set; }
        public string Notes { get; set; }

        public string PlanId { get; set; }
        public Plan Plan { get; set; }
        public string url { get; set; }

        public string OutsidePictures { get; set; }
        public int Resolution { get; set; }
        public List<SelectListItem> Resolutions { get; set; }
        public string Outside3DPictures { get; set; }
    }
}