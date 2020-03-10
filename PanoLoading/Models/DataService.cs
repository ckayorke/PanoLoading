using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PanoLoading.Models
{
    public static class DataService
    {
        private static string cs = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;       
        public static List<Project> ReadAllProject()
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                return db.Query<Project> ("Select * From Project").ToList();
            }
        }
        public static List<Project> ReadAllProjectForUserFielder(string user)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                return db.Query<Project>("Select * From Project").ToList().Where(x => x.Fielder.Trim().ToUpper() == user.Trim().ToUpper()).ToList<Project>();
            }
        }
        public static List<Project> ReadAllProjectForUserDrawer(string user)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                return db.Query<Project>("Select * From Project").ToList().Where(x => x.Drawer.Trim().ToUpper() == user.Trim().ToUpper()).ToList<Project>();
            }
        }
        public static Project FindProject(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
               Project project = db.Query<Project>("Select * From Project WHERE Id = @Id", new { id }).SingleOrDefault();
                project.Levels = ReadAllLevels(project.Id, "").Count;
                project.Rooms = ReadAllRooms(project.Id, "").Count;
                return project;
            }
        }
        public static int UpdateProject(Project project)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {               
                string sqlQuery = "UPDATE Project SET Address = @Address, " +
                "City = @City, " + "Zip = @Zip, " + "State = @State, " + " Creator = @Creator, " + "Levels = @Levels, " + "Rooms = @Rooms, " + " ProjectStatus = @ProjectStatus, " + " Fielder = @Fielder, " + " Drawer = @Drawer, " + "PlanId = @PlanId, " + "Notes = @Notes, " + "OutsidePictures = @OutsidePictures, " + "Resolution = @Resolution, " + "Outside3DPictures = @Outside3DPictures " +
                "WHERE Id = @Id";
                int rowsAffected = db.Execute(sqlQuery, project);
                return rowsAffected;
            }
        }
        public static int CreateProject(Project project)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                Project k = db.Query<Project>("Select * From Project WHERE Address = @Address And City = @City And Zip = @Zip And State = @State And Creator = @Creator  And Creator = @Creator And Drawer = @Drawer", project).SingleOrDefault();

                if (k==null) {
                    string sqlQuery = "INSERT INTO Project(Address, City, Zip, State, Creator, Levels, Rooms, ProjectStatus, Fielder, Drawer, PlanId, Notes, OutsidePictures, Resolution, Outside3DPictures) Values(@Address, @City, @Zip, @State, @Creator, @Levels, @Rooms, @ProjectStatus, @Fielder, @Drawer, @PlanId, @Notes, @OutsidePictures, @Resolution, @Outside3DPictures);";
                    int rowsAffected = db.Execute(sqlQuery, project);
                    return rowsAffected;
                }
                return 0;               
            }  
        }
        public static Project FindProject(Project project)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
               return db.Query<Project>("Select * From Project WHERE Address = @Address And City = @City And Zip = @Zip And State = @State;", project).SingleOrDefault(); 
            }
        }
        public static int DeleteProject(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                string sqlQuery = "Delete from Project WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, new { id });
                return rowsAffected;
            }
        }



        public static List<SelectListItem> GetProjectStates() {
            using (IDbConnection db = new SqlConnection(cs))
            {
                List<Project> projects =  db.Query<Project>("Select * From Project").ToList();
                List<SelectListItem> states = new List<SelectListItem>();
                List<string> names = new List<string>();
                foreach (var project in projects) {
                    names.Add(project.State.ToUpper());                   
                }
                List<string> names2 = names.Distinct().ToList();
                foreach (var name in names2)
                {
                    states.Add(new SelectListItem() { Value = name, Text =name });
                }                
                return states;                
            }
        }
        public static List<SelectListItem> GetProjectCites(string state)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                List<Project> projects = db.Query<Project>("Select * From Project").ToList();
                List<SelectListItem> cities = new List<SelectListItem>();
                List<string> names = new List<string>();
                foreach (var project in projects)
                {
                    if (project.State.ToUpper().Trim() == state.ToUpper().Trim()) {
                        names.Add(project.City.ToUpper());
                    }                   
                }
                List<string> names2 = names.Distinct().ToList();
                foreach (var name in names2)
                {
                    cities.Add(new SelectListItem() { Value = name, Text = name });
                }
                return cities;
            }
        }
        public static List<SelectListItem> GetProjectZIPS(string state, string city)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                List<Project> projects = db.Query<Project>("Select * From Project").ToList();
                List<SelectListItem> zips = new List<SelectListItem>();
                List<string> names = new List<string>();
                foreach (var project in projects)
                {
                    if ((project.State.ToUpper().Trim() == state.ToUpper().Trim()) &&(project.City.ToUpper().Trim() == city.ToUpper().Trim()))
                    {
                        names.Add(project.Zip.Trim());
                    }
                }
                List<string> names2 = names.Distinct().ToList();
                foreach (var name in names2)
                {
                    zips.Add(new SelectListItem() { Value = name, Text = name });
                }
                return zips;
            }
        }
        public static List<SelectListItem> GetProjectAddress(string state, string city, string zip)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                List<Project> projects = db.Query<Project>("Select * From Project").ToList();
                List<SelectListItem> address = new List<SelectListItem>();
                List<string> names = new List<string>();
                foreach (var project in projects)
                {
                    if ((project.State.ToUpper().Trim() == state.ToUpper().Trim()) && (project.City.ToUpper().Trim() == city.ToUpper().Trim()) && (project.Zip.Trim() == zip.Trim()))
                    {
                        names.Add(project.Address.Trim());
                    }
                }
                List<string> names2 = names.Distinct().ToList();
                foreach (var name in names2)
                {
                    address.Add(new SelectListItem() { Value = name, Text = name });
                }
                return address;
            }
        }
        public static List<Level> ReadAllLevels(int projectId, string url)
        {

           

            using (IDbConnection db = new SqlConnection(cs))
            {
                List<Level> levels = db.Query<Level>("Select * From Level WHERE ProjectId = @ProjectId", new { projectId }).ToList();
                foreach(var level in levels)
                {
                    level.DeleteURL = url + "Home/DeleteLevel/" + level.Id;
                    level.ViewURL = url + "Home/GetLevel/" + level.Id;
                }
                return levels;
            }
        }
        public static int CreateLevel(Level level)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                Level k = db.Query<Level>("Select * From Level WHERE Name = @Name And ProjectID = @ProjectID", level).SingleOrDefault();
                if (k == null)
                {
                    string sqlQuery = "INSERT INTO Level(Name, ProjectID, Status, PlanId, PicName) Values(@Name, @ProjectID, @Status, @PlanId, @PicName);";
                    int rowsAffected = db.Execute(sqlQuery, level);
                    return rowsAffected;
                }
                return 0;
            }
        }
        public static Level FindLevel(int id, int projectId)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                return db.Query<Level>("Select * From Level WHERE Id = @Id", new { id, projectId }).SingleOrDefault();
            }
        }
        public static int UpdateLevel(Level level)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                string sqlQuery = "UPDATE Level SET Name = @Name, " +
                "Status = @Status, " + "PlanId = @PlanId, " + "ProjectID = @ProjectID, " + "PicName = @PicName "

                +  "WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, level);
                return rowsAffected;
            }
        }     
        public static Level FindLevel(Level level)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                return db.Query<Level>("Select * From Level WHERE Name = @Name And ProjectID = @ProjectID", level).SingleOrDefault();
            }
        }
        public static int DeleteLevel(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                var sqlQuery = "Delete From Level WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, new { id });
                return rowsAffected;
            }
        }
        public static List<Room> ReadAllRooms(int projectId, string url)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                List<Room> rooms = db.Query<Room>("Select * From Room WHERE ProjectId = @ProjectId", new { projectId }).ToList();
                foreach (var room in rooms)
                {
                    room.DeleteURL = url+ "Home/DeleteRoom/" + room.Id;
                    room.ViewURL = url + "Home/GetRoom/" + room.Id;

                    Level level = FindLevel(room.LevelID, room.ProjectID);
                    room.LevelName = level.Name;                   
                }
                return rooms;
            }
        }
        public static int CreateRoom(Room room)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                Room k = db.Query<Room>("Select * From Room WHERE Name = @Name And ProjectID = @ProjectID And LevelID = @LevelID", room).SingleOrDefault();
                if (k == null)
                {
                    string sqlQuery = "INSERT INTO Room(Name, ProjectID, LevelID, Status, PlanId, PictureList, Connectors, RoomLength, RoomWidth, CenterX, CenterY, ScaleX, ScaleY, Rotation, Shape, Fliped) Values(@Name, @ProjectID, @LevelID, @Status, @PlanId, @PictureList, @Connectors, @RoomLength, @RoomWidth, @CenterX, @CenterY, @ScaleX, @ScaleY, @Rotation, @Shape, @Fliped);";
                    int rowsAffected = db.Execute(sqlQuery, room);
                    return rowsAffected;
                }
                return 0;
            }
        }
        public static Room FindRoom(int id, int projectId)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                var room = db.Query<Room>("Select * From Room WHERE Id = @Id", new { id, projectId }).SingleOrDefault();
                Level level = FindLevel(room.LevelID, room.ProjectID);
                room.LevelName = level.Name;
                return room;
            }
        }
        public static int UpdateRoom(Room room)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                string sqlQuery = "UPDATE Room SET Name = @Name, " +
                "Status = @Status, " + "PlanId = @PlanId, " + "ProjectID = @ProjectID, " + "LevelID = @LevelID, " + "PictureList = @PictureList, " + "Connectors = @Connectors, "  + "RoomLength = @RoomLength, " + "RoomWidth = @RoomWidth, " +
                "CenterX = @CenterX, " + "CenterY = @CenterY, " + "ScaleX = @ScaleX, " + "ScaleY = @ScaleY, " + "Rotation = @Rotation, " + "Shape = @Shape, " + "Fliped = @Fliped " +
                "WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, room);
                return rowsAffected;
            }
        }
        public static Room FindRoom(Room room)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                return db.Query<Room>("Select * From Room WHERE Name = @Name And ProjectID = @ProjectID And LevelID = @LevelID", room).SingleOrDefault();
            }
        }
        public static int DeleteRoom(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                var sqlQuery = "Delete From Room WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, new { id });
                return rowsAffected;
            }
        }


        public static List<AppUser> ReadAllUsers()
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                return db.Query<AppUser>("Select * From AppUser").ToList();
            }
        }
        public static AppUser GetUser(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                AppUser appUser = db.Query<AppUser>("Select * From AppUser WHERE Id = @Id", new { id }).SingleOrDefault();               
                return appUser;
            }
        }

        public static AppUser GetUser(string name)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                AppUser appUser = db.Query<AppUser>("Select * From AppUser WHERE Name = @Name", new { name }).SingleOrDefault();
                return appUser;
            }
        }


        public static int UpdateAppUser(AppUser appUser)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                string sqlQuery = "UPDATE AppUser SET Permission = @Permission " + "WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, appUser);
                return rowsAffected;
            }
        }

        public static int DeleteAppUser(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                string sqlQuery = "Delete from AppUser WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, new { id });
                return rowsAffected;
            }
        }

        public static int CreateAddIFrame(IFrame iframe)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                if (iframe.Id == -1)
                {
                    string sqlQuery = "INSERT INTO IFrame(ProjectId, Url) Values(@ProjectId, @Url);";
                    int rowsAffected = db.Execute(sqlQuery, iframe);
                    return rowsAffected;
                }
                else
                {
                    string sqlQuery = "UPDATE IFrame SET Url = @Url,  DateInserted = GETDATE() WHERE Id = @Id;";
                    int rowsAffected = db.Execute(sqlQuery, iframe);
                    return rowsAffected;
                }
            }
        }
        public static IFrame FindIFrame(int projectId)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                var iframe = db.Query<IFrame>("Select * From IFrame WHERE ProjectId = @ProjectId", new { projectId }).SingleOrDefault();
                return iframe;
            }
        }
        public static int DeleteIFrame(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                string sqlQuery = "Delete from IFrame WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, new { id });
                return rowsAffected;
            }
        }



        public static int CreateAddRoomIFrame(IRoomFrame iframe)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                if (iframe.Id == -1)
                {
                    string sqlQuery = "INSERT INTO RoomIFrame(RoomId, LevelId, ProjectId, Url, Name) Values(@RoomId, @LevelId, @ProjectId, @Url, @Name);";
                    int rowsAffected = db.Execute(sqlQuery, iframe);
                    return rowsAffected;
                }
                else
                {
                    string sqlQuery = "UPDATE RoomIFrame SET Url = @Url WHERE Id = @Id;";
                    int rowsAffected = db.Execute(sqlQuery, iframe);
                    return rowsAffected;
                }
            }
        }
        public static IRoomFrame FindRoomIFrame(IRoomFrame frame)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                var iframe = db.Query<IRoomFrame>("Select * From RoomIFrame WHERE RoomId = @RoomId AND LevelId = @LevelId AND ProjectId = @ProjectId", frame).SingleOrDefault();
                return iframe;
            }
        }

        public static IRoomFrame FindRoomIFrame(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                var iframe = db.Query<IRoomFrame>("Select * From RoomIFrame WHERE Id = @Id", new { id }).SingleOrDefault();
                return iframe;
            }
        }
        public static int DeleteRoomIFrame(int id)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                string sqlQuery = "Delete from RoomIFrame WHERE Id = @Id;";
                int rowsAffected = db.Execute(sqlQuery, new { id });
                return rowsAffected;
            }
        }

        public static List<IRoomFrame> ReadAllIRoomFrame(int projectId)
        {
            using (IDbConnection db = new SqlConnection(cs))
            {
                return db.Query<IRoomFrame>("Select * From RoomIFrame WHERE ProjectId = @ProjectId;", new { projectId }).ToList();
            }
        }
    }
}
