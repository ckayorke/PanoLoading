using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PanoLoading.Models
{
    public class Project1
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZIPCode { get; set; }
        public string State { get; set; }
        public int Status { get; set; }
        public string Status2 { get; set; }
        public string Notes { get; set; }
        public string Completed { get; set; }
        public string OutsidePictures { get; set; }
        public int Resolution { get; set; }
        public string Outside3DPictures { get; set; }
    }

    public class Room1
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int ProjectId { get; set; }
        public int LevelId { get; set; }
        public string Name { get; set; }
        public string LevelName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZIP { get; set; }
        public string PictureName { get; set; }
        public string RoomLength { get; set; }
        public string RoomWidth { get; set; }
        public string Connectors { get; set; }

        public string CenterX { get; set; }
        public string CenterY { get; set; }
        public string ScaleX { get; set; }
        public string ScaleY { get; set; }
        public string Rotation { get; set; }
        public string Shape { get; set; }
        public string Fliped { get; set; }
    }

    public class Level1
    {
        public int Id { get; set; }
        public int LevelId { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public string Status2 { get; set; }
        public string PicName { get; set; }
    }

   

    public class DB
    {
        public int Id { get; set; }
        public List<Level1> levels { get; set; }
        public string message { get; set; }        
        public string name { get; set; }
        public string pass { get; set; }        
        public List<Project1> projects { get; set; }
        public List<Room1> rooms { get; set; }   
       
    }

    public class IFrame
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Url { get; set; }
        public DateTime DateInserted { get; set; }
    }

    public class IRoomFrame
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int LevelId { get; set; }
        public int ProjectId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
    }

    public class IRoomFrame2
    {       
        public string Url { get; set; }
        public string Name { get; set; }        
    }
}