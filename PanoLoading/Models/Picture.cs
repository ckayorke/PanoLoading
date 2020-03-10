using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PanoLoading.Models
{
    public class Picture
    {
        public string Id { get; set; }
        public int ProjectID { get; set; }
        public int LevelID { get; set; }
        public int RoomID { get; set; }
        public string Name { get; set; }
        public string URLToViewer { get; set; }
        public string URLToDelete { get; set; }
    }
    public class Plan
    {
        public string Id { get; set; }
        public int ProjectID { get; set; }
        public int LevelID { get; set; }
        public int RoomID { get; set; }
        public string Name { get; set; }
        public string URLToViewer { get; set; }
        public string URLToDelete { get; set; }
        public string URLToDownload { get; set; }
        public bool PlanAdded { get; set; }
    }
}