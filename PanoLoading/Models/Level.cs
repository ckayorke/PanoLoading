using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PanoLoading.Models
{
    public class Level
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DeleteURL { get; set; }
        public string ViewURL { get; set; }
        public int ProjectID { get; set; }
        public int Status { get; set; }      
        public string PlanId { get; set; }
        public Plan PlanName { get; set; }
        public string PicName { get; set; }
    }
}