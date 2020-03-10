using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PanoLoading.Models
{
    public class Room
    {
        public int Id { get; set; }       
        public int ProjectID { get; set; }
        public int LevelID { get; set; }
        public int Status { get; set; }
        public string PlanId { get; set; }
        public string Name { get; set; }
        public string DeleteURL { get; set; }
        public string ViewURL { get; set; }
        public string LevelName { get; set; }
        public List<Picture> Pictures { get; set; }
        public Plan PlanName { get; set; }
        public string PictureList { get; set; }

        public List<string> ConnectorList { get; set; }
        public string Connectors { get; set; }

        public string RoomLength { get; set; }
        public string RoomWidth { get; set; }

        public string CenterX { get; set; }
        public string CenterY { get; set; }
        public string ScaleX { get; set; }
        public string ScaleY { get; set; }
        public string Rotation { get; set; }
        public string Shape { get; set; }
        public string Fliped { get; set; }
    }
}