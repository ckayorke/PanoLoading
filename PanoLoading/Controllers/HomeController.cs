using HtmlAgilityPack;
using System.Collections.Specialized;
using System.Net;

using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using PanoLoading.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Net.Mail;
using Microsoft.AspNet.Identity;
using System.Security.Cryptography;

namespace PanoLoading.Controllers
{
    public class HomeController : Controller
    {
        string pathUnZipfilesContainer = "";
        string pathZipfilesContainer = "";
        string pathFTP = "";
        List<Project> OldDB = new List<Project>();
        public HomeController()
        {
            OldDB.Clear();
            string fpath = WebConfigurationManager.AppSettings["myFilePath"].ToString();
            string[] subdirectoryEntries = Directory.GetDirectories(fpath);
            foreach (var item in subdirectoryEntries)
            {

                if (item.Contains("UnZipfilesContainer"))
                {
                    pathUnZipfilesContainer = item;
                }
                if (item.Contains("ZipfilesContainer"))
                {
                    pathZipfilesContainer = item;
                }
            }

            string fpath2 = WebConfigurationManager.AppSettings["myFilePath2"].ToString();
            string[] subdirectoryEntries2 = Directory.GetDirectories(fpath2);
            foreach (var item in subdirectoryEntries2)
            {
                if (item.Contains("NVMS-FTP"))
                {
                    pathFTP = item;
                    break;
                }
            }
            // TestLoad();
        }

        private void TestLoad()
        {
            DB db = new DB();
            db.message = "Good";
            db.Id = -1;
            db.name = "";
            db.pass = "";
            db.projects = new List<Project1>();
            db.levels = new List<Level1>();
            db.rooms = new List<Room1>();
            string goodFileName = "";

            var folderDate = "_20191008143600363";
            var filePath = System.IO.Path.Combine("C:\\PanoLoadingData\\UnZipfilesContainer\\_20191008143600363", "db.txt");
            string value = System.IO.File.ReadAllText(filePath);
            DirectoryInfo d = new DirectoryInfo("C:\\PanoLoadingData\\UnZipfilesContainer\\_20191008143600363");
            FileInfo[] Files = d.GetFiles();
            List<FileInfo> images = new List<FileInfo>();
            foreach (FileInfo item in Files)
            {
                if (item.Name.Trim().ToLower() == "db.txt")
                {
                }
                else if (item.Name.Trim().ToLower().Contains(".jpg") || item.Name.Trim().ToLower().Contains(".jpeg"))
                {
                    images.Add(item);
                }
            }

            getObjects(value, images, folderDate, db);
            Directory.Delete(System.IO.Path.Combine(pathUnZipfilesContainer, folderDate), true);
            var k = Json(new JavaScriptSerializer().Serialize(db), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("Decision", "Home");
            }
            DataService.DeleteAppUser(id);
            return RedirectToAction("Administration", "Home");
        }
        public ActionResult Edit(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("Decision", "Home");
            }
            AppUser user2 = DataService.GetUser(id);
            user2.PermissionNameList = GetPermissionList();
            user2.PermissionName = GetPermissionName(user2.Permission);
            return View(user2);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AppUser user)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var log = DataService.ReadAllEmailNotification();
                if (log.Count==0)
                {
                    return RedirectToAction("EditUserError/1", "Home");
                }
                if (log.Count > 0)
                {
                    if(Decrypt(log[0].Login) == "")
                    {
                        return RedirectToAction("EditUserError/2", "Home");
                    }
                }
 
            }
            catch(Exception tx)
            {               
                return RedirectToAction("EditUserError/0", "Home");
            }

            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user2 = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user2 == null || user2.Permission != 3)
            {
                return RedirectToAction("Decision", "Home");
            }
            user.Permission = Convert.ToInt32(user.PermissionName);
            DataService.UpdateAppUser(user);
            try
            {
                SendAsync(user.Name);
            }
            catch (Exception tx)
            {
                return RedirectToAction("EditUserError/3", "Home");
            }
            return RedirectToAction("Administration", "Home");
        }

        public ActionResult EditUserError(int id =4)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }  
            if(id== 0)
            {
                ViewBag.Message = "Email notification table does not exist.";
            }
            else if (id == 1)
            {
                ViewBag.Message = "Email notification table is empty.";
            }
            else if (id == 2)
            {
                ViewBag.Message = "Password Decryption Error.";
            }
            else if (id == 3)
            {
                ViewBag.Message = "Email Authentication Errror.";
            }
            else
            {
                ViewBag.Message = "Unknown Email Notification Error.";
            }

            return View();
        }
        public ActionResult Administration()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("Decision", "Home");
            }
            List<AppUser> list = DataService.ReadAllUsers();
            foreach (var vUser in list)
            {
                vUser.PermissionName = GetPermissionName(vUser.Permission);
            }
            return View(list);
        }
        public ActionResult Index()
        {
            return RedirectToAction("Login", "Account");
            //return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult IOSSupport()
        {
            return View();
        }

        public ActionResult AndroidSupport()
        {
            return View();
        }

        public ActionResult Decision()
        {
            var request = this.HttpContext.Request;
            if (User.Identity.IsAuthenticated)
            {
                DecisionViewModel model = new DecisionViewModel();
                AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
                model.AppUser = user;
                if (model.AppUser == null || user.Permission < 3)
                {
                    return RedirectToAction("DecisionList", "Home");
                }
                else
                {
                    model.HasUser = true;
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult CreateProject()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("Decision", "Home");
            }
            CreateProjectViewModel model = new CreateProjectViewModel();
            try
            {
                model.Valid = (int)TempData["ProjectIssue"];
            }
            catch (Exception e)
            {
                model.Valid = 0;
            }
            model.Email = User.Identity.Name;
            model.ProjectStates = getProjectStates();
            model.Fielders = GetAppUsers();
            model.Resolutions = GetResolutions();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProject(CreateProjectViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("Decision", "Home");
            }
            var email = User.Identity.Name;
            var address = model.Address;
            var city = model.City;
            var state = model.ProjectState;
            var zipCode = model.ZIPCode;
            var fielder = model.Fielder;
            var drawer = model.Drawer;
            var resolution = Int32.Parse(model.Resolution);
            string[] data = { email, address, city, state, zipCode, fielder, drawer };
            int count = CheckInputs(data);
            if (count == 7 && User.Identity.IsAuthenticated && user.Permission == 3)
            {

                Plan plan = new Plan();
                plan.ProjectID = -1;
                plan.LevelID = -1;
                plan.RoomID = -1;
                plan.Id = "";
                plan.Name = "";
                plan.URLToDelete = "";
                plan.URLToViewer = "";
                plan.URLToDownload = "";
                plan.PlanAdded = false;
                string pId = new JavaScriptSerializer().Serialize(plan);
                var note = model.Notes;
                if (note == null || note.Trim().Length == 0)
                {
                    note = "No Comment";
                }
                Project project = new Project()
                {
                    Address = address.Trim().ToUpper(),
                    City = city.Trim().ToUpper(),
                    State = state.Trim().ToUpper(),
                    Zip = zipCode.Trim().ToUpper(),
                    Creator = email.Trim().ToUpper(),
                    Levels = 0,
                    Rooms = 0,
                    ProjectStatus = 0,
                    Fielder = fielder.Trim().ToUpper(),
                    Drawer = drawer.Trim().ToUpper(),
                    PlanId = pId,
                    Notes = note,
                    OutsidePictures = "",
                    Resolution = resolution,
                    Outside3DPictures = ""
                };
                int k = DataService.CreateProject(project);
                if (k > 0)
                {
                    return RedirectToAction("Decision", "Home");
                }
                else
                {
                    TempData["ProjectIssue"] = 1;
                    return RedirectToAction("CreateProject", "Home");
                }
            }
            TempData["ProjectIssue"] = 2;
            return RedirectToAction("CreateProject", "Home");
        }

        public ActionResult ProjectList()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            ProjectListViewModel model = new ProjectListViewModel();
            model.HasUser = true;
            model.AppUser = user;
            model.Projects = DataService.ReadAllProject();
            foreach (var p in model.Projects)
            {
                p.StatusName = GetStatusName(p.ProjectStatus);
            }
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("Decision", "Home");
            }
            return View(model);
        }
        public ActionResult ProjectEdit(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("DecisionList", "Home");
            }
            Project project = DataService.FindProject(id);
            project.ProjectStates = getProjectStates();
            project.Fielders = GetAppUsers();
            project.Drawers = GetAppUsers();
            project.StatusNameList = GetStatusList();
            project.Resolutions = GetResolutions();

            foreach (SelectListItem it in project.ProjectStates)
            {
                if (it.Value.Trim().ToUpper() == project.State.ToUpper().Trim())
                {
                    it.Selected = true;
                }
            }
            foreach (SelectListItem it in project.Fielders)
            {
                if (it.Value.Trim().ToUpper() == project.Fielder.ToUpper().Trim())
                {
                    it.Selected = true;
                }
            }

            foreach (SelectListItem it in project.Drawers)
            {
                if (it.Value.Trim().ToUpper() == project.Drawer.ToUpper().Trim())
                {
                    it.Selected = true;
                }
            }

            foreach (SelectListItem it in project.Resolutions)
            {
                if (it.Value.Trim().ToUpper() == ("" + project.Resolution).Trim())
                {
                    it.Selected = true;
                }
            }

            return View(project);
        }

        public ActionResult ProjectSlides(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("DecisionList", "Home");
            }
            Project project = DataService.FindProject(id);
            project.ProjectStates = getProjectStates();
            project.Fielders = GetAppUsers();
            project.Drawers = GetAppUsers();
            project.StatusNameList = GetStatusList();

            foreach (SelectListItem it in project.ProjectStates)
            {
                if (it.Value.Trim().ToUpper() == project.State.ToUpper().Trim())
                {
                    it.Selected = true;
                }
            }
            foreach (SelectListItem it in project.Fielders)
            {
                if (it.Value.Trim().ToUpper() == project.Fielder.ToUpper().Trim())
                {
                    it.Selected = true;
                }
            }

            foreach (SelectListItem it in project.Drawers)
            {
                if (it.Value.Trim().ToUpper() == project.Drawer.ToUpper().Trim())
                {
                    it.Selected = true;
                }
            }

            List<string> found = new List<string>();
            if (project.OutsidePictures.Trim().Length > 0)
            {
                found = JsonConvert.DeserializeObject<List<string>>(project.OutsidePictures);
            }
            List<string> found2 = new List<string>();

            string url = Request.Url.ToString();
            var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data[0];
            foreach (string item in found)
            {
                string URLToViewer = url + "UploadedPictureFiles/" + item.Trim();
                found2.Add(URLToViewer);
            }
            project.OutPicUrl = found2;

            if (found2.Count < 1)
            {
                return RedirectToAction("ProjectList", "Home");
            }
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProjectEdit(Project project)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("DecisionList", "Home");
            }

            var note = project.Notes;
            if (note == null || note.Trim().Length == 0)
            {
                note = "No Comment";
            }

            var p = DataService.FindProject(project.Id);
            p.Address = project.Address;
            p.City = project.City;
            p.Drawer = project.Drawer;
            p.Fielder = project.Fielder;
            p.Notes = note;
            p.State = project.State;
            p.ProjectStatus = Convert.ToInt32(project.StatusName);
            p.Zip = project.Zip;
            p.Resolution = project.Resolution;
            DataService.UpdateProject(p);
            return RedirectToAction("ProjectList", "Home");
        }
        public ActionResult ProjectDelete(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission != 3)
            {
                return RedirectToAction("DecisionList", "Home");
            }
            DataService.DeleteProject(id);
            CleanServer2(id);
            return RedirectToAction("ProjectList", "Home");
        }
        public ActionResult DecisionList()
        {
            if (User == null)
            {
                return RedirectToAction("Register", "Account");
            }


            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            ProjectListViewModel model = new ProjectListViewModel();
            if (user == null)
            {
                model.Projects = new List<Project>();
            }
            else if (user.Permission == 1)
            {
                model.Projects = DataService.ReadAllProjectForUserDrawer(User.Identity.Name.Trim().ToUpper());
            }
            else if (user.Permission == 2)
            {
                model.Projects = DataService.ReadAllProjectForUserFielder(User.Identity.Name.Trim().ToUpper());
            }
            else if (user.Permission == 3)
            {
                model.Projects = DataService.ReadAllProject();
            }
            else
            {
                model.Projects = new List<Project>();
            }

            string url = Request.Url.ToString();
            var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data[0];

            foreach (var p in model.Projects)
            {
                p.StatusName = GetStatusName(p.ProjectStatus);
                p.url = url + "Home/UpdateSelectedProject";
                //if (p.PlanId != null)
                //{
                p.Plan = JsonConvert.DeserializeObject<Plan>(p.PlanId);
                //}

                List<string> found = new List<string>();
                if (p.OutsidePictures.Trim().Length > 0)
                {
                    found = JsonConvert.DeserializeObject<List<string>>(p.OutsidePictures);
                }
                List<string> found2 = new List<string>();

                string urlTo = Request.Url.ToString();
                var dataTo = urlTo.Split(new string[] { "Home" }, StringSplitOptions.None);
                urlTo = dataTo[0];
                foreach (string item in found)
                {
                    string URLToViewer = urlTo + "UploadedPictureFiles/" + item.Trim();
                    found2.Add(URLToViewer);
                }
                p.OutPicUrl = found2;

            }
            model.HasUser = true;
            model.AppUser = user;


            return View(model);
        }

        public ActionResult CustomerViewer(string id)
        {
            CustomerViewerViewModel model = new CustomerViewerViewModel();
            id = id.Trim();
            int projectId = Int32.Parse(id);
            var iframe = DataService.FindIFrame(projectId);
            if (iframe == null)
            {
                model.URL = "#";
                model.Id = projectId;
            }
            else
            {
                model.URL = iframe.Url;
                model.Id = projectId;
            }

            var proData = DataService.FindProject(projectId);
            proData.Plan = JsonConvert.DeserializeObject<Plan>(proData.PlanId);
            if (proData.Plan.PlanAdded)
            {
                model.FloorURL = proData.Plan.URLToDownload;
            }
            else
            {
                model.FloorURL =  "#";

            }

            string url = Request.Url.ToString();
            var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data[0];

            var levels = DataService.ReadAllLevels(projectId, url);
            var rooms = DataService.ReadAllRooms(projectId, url);
            model.RoomURLs = new List<IRoomFrame2>();

            foreach (var level in levels)
            {                
                    foreach (var room in rooms)
                    {
                        if(room.LevelID == level.Id)
                        {
                           
                           room.Pictures = JsonConvert.DeserializeObject<List<Picture>>(room.PictureList);
                            foreach (var p in room.Pictures)
                            {
                                  IRoomFrame2 frame = new IRoomFrame2();
                                  frame.Name = room.LevelName + ": " + room.Name;
                                  frame.Url = url + "Home/CustomerRoomViewer/" +room.Id + "?p=" + room.ProjectID;
                                  model.RoomURLs.Add(frame);
                                  break;
                            }
                        }
                    }
            }
                   
            return View(model);
        }

        public ActionResult CustomerRoomViewer(string id)
        {
            id = id.Trim();
            int roomId = Int32.Parse(id);
            int pId = Int32.Parse(Request.QueryString["p"]);
            var room = DataService.FindRoom(roomId, pId);
            room.Pictures = JsonConvert.DeserializeObject<List<Picture>>(room.PictureList);

            string urlTo = Request.Url.ToString();
            var dataTo = urlTo.Split(new string[] { "Home" }, StringSplitOptions.None);
            urlTo = dataTo[0];
            ViewBag.MyString = urlTo + "UploadedPictureFiles/"  + room.Pictures[0].Id;
            string startName = room.LevelName + "_" + room.Name;
            startName = startName.Replace(" ", "");
            ViewBag.Name = startName;

            return View();
        }
        public ActionResult Levels(int id = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == 0)
            {
                return RedirectToAction("Decision", "Home");
            }

            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1)
            {
                return RedirectToAction("Decision", "Home");
            }


            Project project = DataService.FindProject(id);
            if (project == null)
            {
                return RedirectToAction("ModifyProject", "Home");
            }

            ViewBag.Title = project.Address + ", " + project.City + ", " + project.State + ", " + project.Zip;

            TempData["SelectedProject"] = project.Id;
            LevelViewModel model = new LevelViewModel();
            model.HasUser = true;
            model.AppUser = user;
            project.Plan = JsonConvert.DeserializeObject<Plan>(project.PlanId);
            model._Project = project;

            string url = Request.Url.ToString();
            var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data[0];

            model.Levels = DataService.ReadAllLevels(project.Id, url);
            foreach (var level in model.Levels)
            {
                level.PlanName = JsonConvert.DeserializeObject<Plan>(level.PlanId);
            }
            model.LevelNames = GetLevelNames();
            model.AllImages = url + "Home/GetPhotos?projectId=" + project.Id;
            model.CustomerViewerUrl = "";

            var iframe = DataService.FindIFrame(id);
            if (iframe == null)
            {               
            }
            else
            {
                model.CustomerViewerUrl = url +  "Home/CustomerViewer/" +  id;
            }
            model.EntranceUrls = new List<string>();
            var paths = project.Outside3DPictures;
            if(paths == "[]" || paths.Trim().Length == 0)
            {
            }
            else
            {
               var k = JsonConvert.DeserializeObject<List<string>>(paths);
                foreach(string s in k)
                {
                    string urlTo = Request.Url.ToString();
                    var dataTo = urlTo.Split(new string[] { "Home" }, StringSplitOptions.None);
                    urlTo = dataTo[0];
                    model.EntranceUrls.Add(s);
                }               
            }

            return View(model);
        }
        public ActionResult DeleteLevel(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            int projectId = (int)TempData["SelectedProject"];
            TempData["SelectedProject"] = projectId;
            Level level = DataService.FindLevel(id, projectId);
            level.PlanName = JsonConvert.DeserializeObject<Plan>(level.PlanId);
            if (level.PlanName.Id == null)
            {
            }
            else
            {
                DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), level.PlanName.Id));
            }
            DeleteLevelProperties(id, projectId);
            DataService.DeleteLevel(id);
            return RedirectToAction("Levels/" + projectId, "Home");
        }
        public ActionResult ProjectPlanUploadFile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1 || user.Permission == 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            return View();
        }
        [HttpPost]
        public ActionResult ProjectPlanUploadFile(HttpPostedFileBase file)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1 || user.Permission == 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            int projectId = (int)TempData["SelectedProject"];
            TempData["SelectedProject"] = projectId;
            Plan plan = new Plan();
            plan.ProjectID = projectId;
            plan.PlanAdded = true;
            Project project = DataService.FindProject(projectId);
            plan.Id = "Project_" + projectId + "_Plan_" + 1;
            try
            {

                string url = Request.Url.ToString();
                var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
                url = data[0];

                if (file.ContentLength > 0)
                {
                    string entx = file.FileName.Substring(file.FileName.Length - 4);
                    string _FileName = plan.Id + entx;
                    plan.Id = _FileName;
                    DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), _FileName));
                    string _path = Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), _FileName);
                    file.SaveAs(_path);
                }
                plan.Name = "Plan " + 1;
                plan.URLToDownload = url + "Home/DownloadFile?fileName=" + plan.Id;
                project.PlanId = new JavaScriptSerializer().Serialize(plan);
                project.ProjectStatus = 2;
                DataService.UpdateProject(project);
                return RedirectToAction("DecisionList", "Home");
            }
            catch
            {
                return RedirectToAction("DecisionList", "Home");
            }
        }
        public ActionResult LevelPlanUploadFile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1 || user.Permission == 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            return View();
        }
        [HttpPost]
        public ActionResult LevelPlanUploadFile(HttpPostedFileBase file)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1 || user.Permission == 2)
            {
                return RedirectToAction("Decision", "Home");
            }

            int projectId = (int)TempData["SelectedProject"];
            int levelId = (int)TempData["SelectedLevel"];
            TempData["SelectedProject"] = projectId;
            TempData["SelectedLevel"] = levelId;
            Plan plan = new Plan();
            plan.ProjectID = projectId;
            plan.LevelID = levelId;
            plan.PlanAdded = true;
            Level level = DataService.FindLevel(levelId, projectId);
            plan.Id = "Project_" + projectId + "_Level_" + levelId + "_Plan_" + 1;
            try
            {
                if (file.ContentLength > 0)
                {
                    string entx = file.FileName.Substring(file.FileName.Length - 4);
                    string _FileName = plan.Id + entx;
                    plan.Id = _FileName;
                    DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), _FileName));
                    string _path = Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), _FileName);
                    file.SaveAs(_path);
                }

                string url = Request.Url.ToString();
                var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
                url = data[0];

                plan.Name = "Plan " + 1;
                plan.URLToDelete = url + "Home/DeleteLevelPlan/_" + projectId + "_" + levelId;
                plan.URLToViewer = "~/UploadedPlanFiles/" + plan.Id;
                plan.URLToDownload = url + "Home/DownloadFile?fileName=" + plan.Id;
                level.PlanId = new JavaScriptSerializer().Serialize(plan);
                level.Status = 2;
                DataService.UpdateLevel(level);
                return RedirectToAction("Levels/" + projectId, "Home");
            }
            catch
            {
                return RedirectToAction("Levels/" + projectId, "Home");
            }
        }
        public ActionResult GetLevel(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1)
            {
                return RedirectToAction("Decision", "Home");
            }

            int data = (int)TempData["SelectedProject"];
            Project project = DataService.FindProject(data);
            if (project == null)
            {
                return RedirectToAction("ModifyProject", "Home");
            }
            TempData["SelectedProject"] = project.Id;
            TempData["SelectedLevel"] = id;
            RoomViewModel model = new RoomViewModel();
            model.HasUser = true;
            model.AppUser = user;
            model.Level = DataService.FindLevel(id, data);
            model.Level.PlanName = JsonConvert.DeserializeObject<Plan>(model.Level.PlanId);





            ViewBag.Title = project.Address + ", " + project.City + ", " + project.State + ", "
                + project.Zip + ">> " + model.Level.Name;

            string url = Request.Url.ToString();
            var data2 = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data2[0];

            model.Rooms = DataService.ReadAllRooms(project.Id, url).Where(x => x.LevelID == id).ToList();
            model.RoomNames = GetRoomNames();
            return View(model);
        }
        public ActionResult GetRoom(int id)
        {
            RoomDetailsViewModel model = new RoomDetailsViewModel();

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1)
            {
                return RedirectToAction("Decision", "Home");
            }

            int data = (int)TempData["SelectedProject"];
            TempData["SelectedProject"] = data;
            TempData["SelectedRoom"] = id;

            model.HasUser = true;
            model.AppUser = user;
            model.Id = id;
            model.Room = DataService.FindRoom(id, data);
            model.Room.Pictures = JsonConvert.DeserializeObject<List<Picture>>(model.Room.PictureList);
            model.Room.PlanName = JsonConvert.DeserializeObject<Plan>(model.Room.PlanId);
            model.Room.ConnectorList = JsonConvert.DeserializeObject<List<string>>(model.Room.Connectors);

            string url = Request.Url.ToString();
            var data2 = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data2[0];


            model.Room.PlanName.URLToDownload = url + "Home/DownloadFile?fileName=" + model.Room.PlanName.Id;
            model.RoomNamesList = GetRoomNames();
            string pName = model.Room.PlanName.URLToViewer;
            if (pName == null)
            {
                //model.Room.PlanName.URLToViewer = "~/UploadedPlanFiles/backup.jpg";
                model.Room.PlanName.URLToViewer = url + "Home/GetPlan/" + "backup.jpg";
            }

            Project project = DataService.FindProject(data);
            ViewBag.Title = project.Address + ", " + project.City + ", " + project.State + ", "
                + project.Zip + ">> " + model.Room.LevelName + ">> " + model.Room.Name;


            return View(model);
        }
        public ActionResult RoomViewer(int id, string item)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            int data = (int)TempData["SelectedProject"];
            TempData["SelectedProject"] = data;
            TempData["SelectedRoom"] = id;
            Room r = DataService.FindRoom(id, data);
            ViewerViewModel model = new ViewerViewModel();
            model.ha3D = "false";
            model.URL = "";
            string url = Request.Url.ToString();
            var data2 = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data2[0];
            model.ha3D = "true";
            model.URL = url + "/UploadedPictureFiles/" + item.Trim();
            Project p = DataService.FindProject(data);
            model.Info = p.Address + ", " + p.City + " -" + r.Name + "- " + r.LevelName + "- " + getShape(r.Shape) + "- " + r.RoomLength;
            model.FloorURL = "";
            Level l = DataService.FindLevel(r.LevelID, r.ProjectID);
            string[] names = l.PicName.Split(';');
            foreach (string n in names)
            {
                string n1 = n.Trim();
                if (n1.Length > 0)
                {
                    model.FloorURL = model.FloorURL + ";" + url + "/UploadedPictureFiles/" + n1;
                }
            }
            model.FloorURL = model.FloorURL.Trim();
            model.FloorURL = model.FloorURL.Substring(1);
            return View(model);
        }
        public ActionResult EntranceViewer(string item)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            EntranceViewerViewModel model = new EntranceViewerViewModel();
            string url = Request.Url.ToString();
            var data2 = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data2[0];
            model.URL = url + "/UploadedPictureFiles/" + item.Trim();
            return View(model);
        }

        private string getShape(string shape)
        {
            string x = "";
            if (shape.Trim() == "1")
            {
                x = "Square";
            }
            if (shape.Trim() == "2")
            {
                x = "Rectangle";
            }
            if (shape.Trim() == "3")
            {
                x = "Hexagon";
            }
            if (shape.Trim() == "4")
            {
                x = "Diamond Shape";
            }
            if (shape.Trim() == "5")
            {
                x = "L Shape";
            }
            return x;
        }
        public ActionResult GetPlan(string id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1 || user.Permission == 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            if (id.Trim() == "backup.jpg")
            {
                int id2 = (int)TempData["SelectedRoom"];
                TempData["SelectedRoom"] = id2;
                return RedirectToAction("GetRoom/" + id2, "Home");
            }
            string[] ids = id.Trim().Split('_');
            if (ids.Length != 5)
            {
                int id2 = (int)TempData["SelectedRoom"];
                TempData["SelectedRoom"] = id2;
                return RedirectToAction("GetRoom/" + id2, "Home");
            }
            int projectId = Convert.ToInt32(ids[1]);
            int levelId = Convert.ToInt32(ids[2]);
            int roomId = Convert.ToInt32(ids[3]);

            string file2 = Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), "Project_" + projectId + "_Level_" + levelId + "_Room_" + roomId + "_Plan_1.jpg");

            string url = Request.Url.ToString();
            var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            string file = data[0];
            try
            {
                if (System.IO.File.Exists(file2))
                {
                    file = file + "UploadedPlanFiles/Project_" + projectId + "_Level_" + levelId + "_Room_" + roomId + "_Plan_1.jpg";
                }
                else
                {
                    file = file + "UploadedPlanFiles/Project_" + projectId + "_Level_" + levelId + "_Room_" + roomId + "_Plan_1.png";
                }
            }
            catch (IOException ex)
            {
                file = file + "UploadedPlanFiles/Project_" + projectId + "_Level_" + levelId + "_Room_" + roomId + "_Plan_1.png";
            }


            PlanViewModel model = new PlanViewModel();
            model.Id = file;
            model.Room = DataService.FindRoom(roomId, projectId);
            return View(model);
        }
        [HttpGet]
        public ActionResult PictureUploadFile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            return View();
        }
        [HttpPost]
        public ActionResult PictureUploadFile(HttpPostedFileBase file)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            int projectId = (int)TempData["SelectedProject"];
            int levelId = (int)TempData["SelectedLevel"];
            int roomId = (int)TempData["SelectedRoom"];
            TempData["SelectedProject"] = projectId;
            TempData["SelectedLevel"] = levelId;
            TempData["SelectedRoom"] = roomId;
            Picture pic = new Picture();
            pic.ProjectID = projectId;
            pic.LevelID = levelId;
            pic.RoomID = roomId;
            Room room = DataService.FindRoom(roomId, projectId);
            room.Status = 1;
            List<Picture> pictures = JsonConvert.DeserializeObject<List<Picture>>(room.PictureList);
            pictures.Add(pic);
            pic.Id = "Project_" + projectId + "_Level_" + levelId + "_Room_" + roomId + "_Picture_" + pictures.Count;
            try
            {
                if (file.ContentLength > 0)
                {
                    string ext = file.FileName.Substring(file.FileName.Length - 4);
                    string _FileName = pic.Id + ext;
                    string _path = Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), _FileName);
                    file.SaveAs(_path);
                    pic.Id = _FileName;
                }

                string url = Request.Url.ToString();
                var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
                url = data[0];

                pic.Name = "Picture " + pictures.Count;
                pic.URLToDelete = projectId + "_" + levelId + "_" + roomId + "_" + pictures.Count;
                pic.URLToViewer = "https://www.yahoo.com/" + pic.Id;
                room.PictureList = new JavaScriptSerializer().Serialize(pictures);
                room.Status = 1;
                DataService.UpdateRoom(room);
                return RedirectToAction("GetRoom/" + roomId, "Home");
            }
            catch (Exception ex)
            {
                return RedirectToAction("GetRoom/" + roomId, "Home");
            }
        }
        [HttpGet]
        public ActionResult RoomPlanUploadFile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1 || user.Permission == 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            return View();
        }
        [HttpPost]
        public ActionResult RoomPlanUploadFile(HttpPostedFileBase file)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1 || user.Permission == 2)
            {
                return RedirectToAction("Decision", "Home");
            }
            int projectId = (int)TempData["SelectedProject"];
            int levelId = (int)TempData["SelectedLevel"];
            int roomId = (int)TempData["SelectedRoom"];
            TempData["SelectedProject"] = projectId;
            TempData["SelectedLevel"] = levelId;
            TempData["SelectedRoom"] = roomId;
            Plan plan = new Plan();
            plan.ProjectID = projectId;
            plan.LevelID = levelId;
            plan.RoomID = roomId;
            plan.PlanAdded = true;
            Room room = DataService.FindRoom(roomId, projectId);
            room.Status = 2;
            plan.Id = "Project_" + projectId + "_Level_" + levelId + "_Room_" + roomId + "_Plan_" + 1;

            try
            {
                if (file.ContentLength > 0)
                {
                    string entx = file.FileName.Substring(file.FileName.Length - 4);
                    string _FileName = plan.Id + entx;
                    plan.Id = _FileName;
                    DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), _FileName));
                    string _path = Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), _FileName);
                    file.SaveAs(_path);
                }


                string url = Request.Url.ToString();
                var data = url.Split(new string[] { "Home" }, StringSplitOptions.None);
                url = data[0];


                plan.Name = "Plan " + 1;
                plan.URLToDelete = url + "Home/DeletePlan/_" + projectId + "_" + levelId + "_" + roomId + "_" + 1;
                plan.URLToViewer = url + "Home/GetPlan/_" + projectId + "_" + levelId + "_" + roomId + "_" + 1;
                plan.URLToDownload = url + "Home/DownloadFile?fileName=" + plan.Id;
                room.PlanId = new JavaScriptSerializer().Serialize(plan);
                room.Status = 2;
                DataService.UpdateRoom(room);
                return RedirectToAction("GetRoom/" + roomId, "Home");
            }
            catch
            {
                return RedirectToAction("GetRoom/" + roomId, "Home");
            }
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult DeletePicture(string info)
        {
            //var url = Request.Url.LocalPath.Substring(21);
            //var path = Request.Url.LocalPath.Trim();
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 2)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            string[] ids = info.Trim().Split('_');
            if (ids.Length != 4)
            {
                int id = (int)TempData["SelectedRoom"];
                TempData["SelectedRoom"] = id;
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            int projectId = Convert.ToInt32(ids[0]);
            int levelId = Convert.ToInt32(ids[1]);
            int roomId = Convert.ToInt32(ids[2]);
            int picId = Convert.ToInt32(ids[2]);
            Room room = DataService.FindRoom(roomId, projectId);
            List<Picture> pictures = JsonConvert.DeserializeObject<List<Picture>>(room.PictureList);

            var check = "Project_" + projectId + "_Level_" + levelId + "_Room_" + roomId + "_Picture_";
            for (var i = 0; i < pictures.Count; i++)
            {
                var pict = pictures[i];
                if (pict.Id.Contains(check))
                {
                    DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), pict.Id));
                    pictures.RemoveAt(i);
                    break;
                }
            }
            room.PictureList = new JavaScriptSerializer().Serialize(pictures);
            DataService.UpdateRoom(room);
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadCities(string state)
        {
            List<SelectListItem> cities = new List<SelectListItem>();
            if (!User.Identity.IsAuthenticated)
            {
                return Json(cities, JsonRequestBehavior.AllowGet);
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1)
            {
                return Json(cities, JsonRequestBehavior.AllowGet);
            }
            cities = DataService.GetProjectCites(state);
            return Json(cities, JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadZIPS(string state, string city)
        {
            List<SelectListItem> cities = new List<SelectListItem>();
            if (!User.Identity.IsAuthenticated)
            {
                return Json(cities, JsonRequestBehavior.AllowGet);
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1)
            {
                return Json(cities, JsonRequestBehavior.AllowGet);
            }
            cities = DataService.GetProjectZIPS(state, city);
            return Json(cities, JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadAddresses(string state, string city, string zip)
        {
            List<SelectListItem> cities = new List<SelectListItem>();
            if (!User.Identity.IsAuthenticated)
            {
                return Json(cities, JsonRequestBehavior.AllowGet);
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1)
            {
                return Json(cities, JsonRequestBehavior.AllowGet);
            }
            cities = DataService.GetProjectAddress(state, city, zip);
            return Json(cities, JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadAppUsers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (!User.Identity.IsAuthenticated)
            {
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            list = GetAppUsers();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult DeleteRoom(int room)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            int projectId = (int)TempData["SelectedProject"];
            TempData["SelectedProject"] = projectId;
            Room r = DataService.FindRoom(room, projectId);
            DeleteRoomProperties(r);
            DataService.DeleteRoom(room);
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult UpdateSelectedLevel(int level)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            TempData["SelectedLevel"] = level;
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateSelectedProject(int projectId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            TempData["SelectedProject"] = projectId;
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
        public FileResult DownloadFile(string fileName)
        {
            byte[] fileBytes2 = new byte[0];
            if (!User.Identity.IsAuthenticated)
            {
                return File(fileBytes2, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            List<AppUser> users = DataService.ReadAllUsers();
            AppUser user = users.Where(x => x.Name.Trim().ToUpper() == User.Identity.Name.Trim().ToUpper()).SingleOrDefault<AppUser>();
            if (user == null || user.Permission < 1)
            {
                return File(fileBytes2, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            string name = Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), fileName);
            try
            {
                if (System.IO.File.Exists(name))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(name);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (IOException ex)
            {
            }
            return File(fileBytes2, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult AddLevel(string level)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            int data = (int)TempData["SelectedProject"];


            string url = Request.Url.ToString();
            var data2 = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data2[0];


            List<Level> levels = DataService.ReadAllLevels(data, url).Where(x => x.Name.Trim().ToUpper() == level.Trim().ToUpper()).ToList<Level>();
            if (levels.Count > 0)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            Level levelInfo = new Level();
            levelInfo.Name = level;
            levelInfo.Status = 0;
            levelInfo.ProjectID = data;
            levelInfo.Status = 0;
            levelInfo.PicName = "";
            Plan plan = new Models.Plan();
            plan.PlanAdded = false;
            levelInfo.PlanId = new JavaScriptSerializer().Serialize(plan);
            TempData["SelectedProject"] = data;
            int k = DataService.CreateLevel(levelInfo);
            if (k < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult AddRoom(string room)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            int data = (int)TempData["SelectedProject"];
            TempData["SelectedProject"] = data;

            string url = Request.Url.ToString();
            var data2 = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data2[0];

            List<Room> rooms = DataService.ReadAllRooms(data, url).Where(x => x.Name.Trim().ToUpper() == room.Trim().ToUpper()).ToList<Room>();
            if (rooms.Count > 0)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            Room roomInfo = new Room();
            roomInfo.Name = room;
            roomInfo.ProjectID = data;
            roomInfo.LevelID = (int)TempData["SelectedLevel"];
            roomInfo.Status = 0;
            roomInfo.RoomLength = new JavaScriptSerializer().Serialize(new List<string>());
            roomInfo.RoomWidth = "-1";


            roomInfo.CenterX = "0";
            roomInfo.CenterY = "0";
            roomInfo.ScaleX = "0";
            roomInfo.ScaleY = "0";
            roomInfo.Rotation = "0";
            roomInfo.Shape = "0";
            roomInfo.Fliped = "0";




            Plan plan = new Plan();
            plan.ProjectID = roomInfo.ProjectID;
            plan.LevelID = roomInfo.LevelID;
            plan.RoomID = -1;
            plan.Id = "";
            plan.Name = "";
            plan.URLToDelete = "";
            plan.URLToViewer = "";
            plan.URLToDownload = "";
            roomInfo.PlanId = new JavaScriptSerializer().Serialize(plan); ;
            List<Picture> pictures = new List<Picture>();
            roomInfo.PictureList = new JavaScriptSerializer().Serialize(pictures);
            roomInfo.ConnectorList = new List<string>();
            roomInfo.Connectors = new JavaScriptSerializer().Serialize(roomInfo.ConnectorList);
            TempData["SelectedLevel"] = roomInfo.LevelID;
            int k = DataService.CreateRoom(roomInfo);
            if (k < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddNeighbor(string neighbor)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }

            int projectId = (int)TempData["SelectedProject"];
            int levelId = (int)TempData["SelectedLevel"];
            int roomId = (int)TempData["SelectedRoom"];
            TempData["SelectedProject"] = projectId;
            TempData["SelectedLevel"] = levelId;
            TempData["SelectedRoom"] = roomId;
            Room room = DataService.FindRoom(roomId, projectId);
            if (room == null)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            room.ConnectorList = JsonConvert.DeserializeObject<List<string>>(room.Connectors);

            foreach (var name in room.ConnectorList)
            {
                if (name.ToUpper().Trim() == neighbor.Trim().ToUpper())
                {
                    return Json("Bad", JsonRequestBehavior.AllowGet);
                }
            }
            room.ConnectorList.Add(neighbor);
            room.Connectors = new JavaScriptSerializer().Serialize(room.ConnectorList);
            DataService.UpdateRoom(room);
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteNeighbor(string neighbor)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission < 1)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }

            int projectId = (int)TempData["SelectedProject"];
            int levelId = (int)TempData["SelectedLevel"];
            int roomId = (int)TempData["SelectedRoom"];
            TempData["SelectedProject"] = projectId;
            TempData["SelectedLevel"] = levelId;
            TempData["SelectedRoom"] = roomId;
            Room room = DataService.FindRoom(roomId, projectId);
            if (room == null)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            room.ConnectorList = JsonConvert.DeserializeObject<List<string>>(room.Connectors);
            List<string> names = new List<string>();
            foreach (var name in room.ConnectorList)
            {
                if (name.ToUpper().Trim() == neighbor.Trim().ToUpper())
                {
                }
                else
                {
                    names.Add(name);
                }
            }
            room.Connectors = new JavaScriptSerializer().Serialize(names);
            DataService.UpdateRoom(room);
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
        public int CheckInputs(string[] data)
        {
            int counter = 7;
            for (var i = 0; i < data.Length; i++)
            {
                if (string.IsNullOrEmpty(data[i]))
                {
                    counter = counter - 1;
                }
            }
            return counter;
        }
        private bool HasPermission(string email)
        {
            return true;
        }
        private bool DeleteFiles(string name)
        {
            try
            {
                if (System.IO.File.Exists(name))
                {
                    System.IO.File.Delete(name);
                    var k = 0;
                }
            }
            catch (IOException ex)
            {
            }
            return true;
        }
        private void DeleteLevelProperties(int levelId, int projectId)
        {
            string url = Request.Url.ToString();
            var data2 = url.Split(new string[] { "Home" }, StringSplitOptions.None);
            url = data2[0];


            List<Room> rooms = DataService.ReadAllRooms(projectId, url);
            foreach (Room r in rooms)
            {
                if (r.LevelID == levelId)
                {
                    DeleteRoomProperties(r);
                }
            }
        }
        private void DeleteRoomProperties(Room r)
        {
            r.PlanName = JsonConvert.DeserializeObject<Plan>(r.PlanId);
            DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPlanFiles"), r.PlanName.Id));
            List<Picture> pictures = JsonConvert.DeserializeObject<List<Picture>>(r.PictureList);
            foreach (Picture p in pictures)
            {
                DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), p.Id));
            }
        }
        public List<SelectListItem> GetLevelNames()
        {
            List<SelectListItem> names = new List<SelectListItem>();
            names.Add(new SelectListItem() { Value = "Basement", Text = "Basement" });
            names.Add(new SelectListItem() { Value = "First Floor", Text = "First Floor" });
            names.Add(new SelectListItem() { Value = "Second Floor", Text = "Second Floor" });
            names.Add(new SelectListItem() { Value = "Third Floor", Text = "Third Floor" });
            names.Add(new SelectListItem() { Value = "Fourth Floor", Text = "Fourth Floor" });
            return names;
        }
        public List<SelectListItem> GetAppUsers()
        {
            List<SelectListItem> names = new List<SelectListItem>();
            List<AppUser> list = DataService.ReadAllUsers();

            foreach (AppUser user in list)
            {
                names.Add(new SelectListItem() { Value = user.Name, Text = user.Name });
            }
            return names;
        }
        public List<SelectListItem> GetRoomNames()
        {
            List<SelectListItem> names = new List<SelectListItem>();
            names.Add(new SelectListItem() { Value = "Attic", Text = "Attic" });
            names.Add(new SelectListItem() { Value = "Basement", Text = "Basement" });
            names.Add(new SelectListItem() { Value = "Master Bathroom", Text = "Master Bathroom" });
            names.Add(new SelectListItem() { Value = "Bathroom 1", Text = "Bathroom 1" });
            names.Add(new SelectListItem() { Value = "Bathroom 2", Text = "Bathroom 2" });
            names.Add(new SelectListItem() { Value = "Bathroom 3", Text = "Bathroom 3" });
            names.Add(new SelectListItem() { Value = "Bathroom 4", Text = "Bathroom 4" });
            names.Add(new SelectListItem() { Value = "Bathroom 5", Text = "Bathroom 5" });
            names.Add(new SelectListItem() { Value = "Bathroom 6", Text = "Bathroom 6" });
            names.Add(new SelectListItem() { Value = "Master Bedroom", Text = "Master Bedroom" });
            names.Add(new SelectListItem() { Value = "Bedroom 1", Text = "Bedroom 1" });
            names.Add(new SelectListItem() { Value = "Bedroom 2", Text = "Bedroom 2" });
            names.Add(new SelectListItem() { Value = "Bedroom 3", Text = "Bedroom 3" });
            names.Add(new SelectListItem() { Value = "Bedroom 4", Text = "Bedroom 4" });
            names.Add(new SelectListItem() { Value = "Bedroom 5", Text = "Bedroom 5" });
            names.Add(new SelectListItem() { Value = "Bedroom 6", Text = "Bedroom 6" });
            names.Add(new SelectListItem() { Value = "Bedroom 7", Text = "Bedroom 7" });
            names.Add(new SelectListItem() { Value = "Bedroom 8", Text = "Bedroom 8" });
            names.Add(new SelectListItem() { Value = "Deck", Text = "Deck" });
            names.Add(new SelectListItem() { Value = "Den", Text = "Den" });
            names.Add(new SelectListItem() { Value = "Dining Room", Text = "Dining Room" });
            names.Add(new SelectListItem() { Value = "Front Yard", Text = "Front Yard" });
            names.Add(new SelectListItem() { Value = "Back Yard", Text = "Back Yard" });
            names.Add(new SelectListItem() { Value = "Right Side Yard", Text = "Right Side Yard" });
            names.Add(new SelectListItem() { Value = "Left Side Yard", Text = "Left Side Yard" });
            names.Add(new SelectListItem() { Value = "Garage", Text = "Garage" });
            names.Add(new SelectListItem() { Value = "Hallway", Text = "Hallway" });
            names.Add(new SelectListItem() { Value = "Kitchen", Text = "Kitchen" });
            names.Add(new SelectListItem() { Value = "Laundry", Text = "Laundry" });
            names.Add(new SelectListItem() { Value = "Porch", Text = "Porch" });
            names.Add(new SelectListItem() { Value = "Play Room", Text = "Play Room" });
            names.Add(new SelectListItem() { Value = "Patio", Text = "Patio" });
            names.Add(new SelectListItem() { Value = "Pantry", Text = "Pantry" });
            names.Add(new SelectListItem() { Value = "Office", Text = "Office" });
            names.Add(new SelectListItem() { Value = "Living Room", Text = "Living Room" });
            names.Add(new SelectListItem() { Value = "Family Room", Text = "Family Room" });
            names.Add(new SelectListItem() { Value = "Staircase", Text = "Staircase" });
            names.Add(new SelectListItem() { Value = "Study", Text = "Study" });
            names.Add(new SelectListItem() { Value = "Sun Room", Text = "Sun Room" });
            names.Add(new SelectListItem() { Value = "TV Room", Text = "TV Room" });
            names.Add(new SelectListItem() { Value = "Workshop", Text = "Workshope" });
            names.Add(new SelectListItem() { Value = "Craft Room", Text = "Craft Room" });
            names.Add(new SelectListItem() { Value = "Classroom", Text = "Classroom" });
            names.Sort((x, y) => x.Value.CompareTo(y.Value));
            return names;

        }
        public List<SelectListItem> getProjectStates()
        {
            List<SelectListItem> states = new List<SelectListItem>();
            states.Add(new SelectListItem() { Value = "Alabama", Text = "Alabama" });
            states.Add(new SelectListItem() { Value = "Alaska", Text = "Alaska" });
            states.Add(new SelectListItem() { Value = "Arizona", Text = "Arizona" });
            states.Add(new SelectListItem() { Value = "Arkansas", Text = "Arkansas" });
            states.Add(new SelectListItem() { Value = "California", Text = "California" });
            states.Add(new SelectListItem() { Value = "Colorado", Text = "Colorado" });
            states.Add(new SelectListItem() { Value = "Connecticut", Text = "Connecticut" });
            states.Add(new SelectListItem() { Value = "Delaware", Text = "Delaware" });
            states.Add(new SelectListItem() { Value = "District Of Columbia", Text = "District Of Columbia" });
            states.Add(new SelectListItem() { Value = "Florida", Text = "Florida" });
            states.Add(new SelectListItem() { Value = "Georgia", Text = "Georgia" });
            states.Add(new SelectListItem() { Value = "Hawaii", Text = "Hawaii" });
            states.Add(new SelectListItem() { Value = "Idaho", Text = "Idaho" });
            states.Add(new SelectListItem() { Value = "Illinois", Text = "Illinois" });
            states.Add(new SelectListItem() { Value = "Indiana", Text = "Indiana" });
            states.Add(new SelectListItem() { Value = "Iowa", Text = "Iowa" });
            states.Add(new SelectListItem() { Value = "Kansas", Text = "Kansas" });
            states.Add(new SelectListItem() { Value = "Kentucky", Text = "Kentucky" });
            states.Add(new SelectListItem() { Value = "Louisiana", Text = "Louisiana" });
            states.Add(new SelectListItem() { Value = "Maine", Text = "Maine" });
            states.Add(new SelectListItem() { Value = "Maryland", Text = "Maryland" });
            states.Add(new SelectListItem() { Value = "Massachusetts", Text = "Massachusetts" });
            states.Add(new SelectListItem() { Value = "Michigan", Text = "Michigan" });
            states.Add(new SelectListItem() { Value = "Minnesota", Text = "Minnesota" });
            states.Add(new SelectListItem() { Value = "Mississippi", Text = "Mississippi" });
            states.Add(new SelectListItem() { Value = "Missouri", Text = "Missouri" });
            states.Add(new SelectListItem() { Value = "Montana", Text = "Montana" });
            states.Add(new SelectListItem() { Value = "Nebraska", Text = "Nebraska" });
            states.Add(new SelectListItem() { Value = "Nevada", Text = "Nevada" });
            states.Add(new SelectListItem() { Value = "New Hampshire", Text = "New Hampshire" });
            states.Add(new SelectListItem() { Value = "New Jersey", Text = "New Jersey" });
            states.Add(new SelectListItem() { Value = "New Mexico", Text = "New Mexico" });
            states.Add(new SelectListItem() { Value = "New York", Text = "New York" });
            states.Add(new SelectListItem() { Value = "North Carolina", Text = "North Carolina" });
            states.Add(new SelectListItem() { Value = "North Dakota", Text = "North Dakota" });
            states.Add(new SelectListItem() { Value = "Ohio", Text = "Ohio" });
            states.Add(new SelectListItem() { Value = "Oklahoma", Text = "Oklahoma" });
            states.Add(new SelectListItem() { Value = "Oregon", Text = "Oregon" });
            states.Add(new SelectListItem() { Value = "Pennsylvania", Text = "Pennsylvania" });
            states.Add(new SelectListItem() { Value = "Rhode Island", Text = "Rhode Island" });
            states.Add(new SelectListItem() { Value = "South Carolina", Text = "South Carolina" });
            states.Add(new SelectListItem() { Value = "South Dakota", Text = "South Dakota" });
            states.Add(new SelectListItem() { Value = "Tennessee", Text = "Tennessee" });
            states.Add(new SelectListItem() { Value = "Texas", Text = "Texas" });
            states.Add(new SelectListItem() { Value = "Utah", Text = "Utah" });
            states.Add(new SelectListItem() { Value = "Vermont", Text = "Vermont" });
            states.Add(new SelectListItem() { Value = "Virginia", Text = "Virginia" });
            states.Add(new SelectListItem() { Value = "Washington", Text = "Washington" });
            states.Add(new SelectListItem() { Value = "West Virginia", Text = "West Virginia" });
            states.Add(new SelectListItem() { Value = "Wisconsin", Text = "Wisconsin" });
            states.Add(new SelectListItem() { Value = "Wyoming", Text = "Wyoming" });
            return states;
        }
        public List<SelectListItem> GetPermissionList()
        {
            List<SelectListItem> names = new List<SelectListItem>();
            names.Add(new SelectListItem() { Value = "0", Text = "None" });
            names.Add(new SelectListItem() { Value = "1", Text = "Drawer" });
            names.Add(new SelectListItem() { Value = "2", Text = "Field Personnel" });
            names.Add(new SelectListItem() { Value = "3", Text = "Admin" });
            return names;
        }

        public List<SelectListItem> GetResolutions()
        {
            List<SelectListItem> names = new List<SelectListItem>();
            names.Add(new SelectListItem() { Value = "1", Text = "Normal" });
            names.Add(new SelectListItem() { Value = "2", Text = "HD" });
            return names;
        }


        
        public string GetPermissionName(int id)
        {
            string name = "None";
            switch (id)
            {
                case 1:
                    name = "Drawer";
                    break;
                case 2:
                    name = "Field Personnel";
                    break;
                case 3:
                    name = "Admin";
                    break;
                default:
                    break;
            }
            return name;
        }
        public List<SelectListItem> GetStatusList()
        {
            List<SelectListItem> names = new List<SelectListItem>();
            names.Add(new SelectListItem() { Value = "0", Text = "Created" });
            names.Add(new SelectListItem() { Value = "1", Text = "Measured" });
            names.Add(new SelectListItem() { Value = "2", Text = "NeedInfo" });
            names.Add(new SelectListItem() { Value = "3", Text = "Drawn" });
            names.Add(new SelectListItem() { Value = "4", Text = "Approved" });
            names.Add(new SelectListItem() { Value = "5", Text = "Completed" });

            return names;
        }
        public string GetStatusName(int id)
        {
            string name = "Created";
            switch (id)
            {
                case 1:
                    name = "Measured";
                    break;
                case 2:
                    name = "NeedInfo";
                    break;
                case 3:
                    name = "Drawn";
                    break;
                case 4:
                    name = "Approved";
                    break;
                case 5:
                    name = "Completed";
                    break;
                default:
                    break;
            }
            return name;
        }

        [HttpPost]
        public JsonResult DBSync()
        {
            DB db = new DB();
            db.message = "Good";
            db.Id = -1;
            db.name = "";
            db.pass = "";
            db.projects = new List<Project1>();
            db.levels = new List<Level1>();
            db.rooms = new List<Room1>();
            string goodFileName = "";

            try
            {
                using (Stream readstream = Request.InputStream)
                {
                    using (StreamReader reader = new StreamReader(readstream, Encoding.UTF8))
                    {
                        goodFileName = reader.ReadToEnd();
                    }
                }
                if (goodFileName.Trim().Length == 0)
                {
                    db.message = "File Name Not Found";
                    return Json(new JavaScriptSerializer().Serialize(db), JsonRequestBehavior.AllowGet);
                }
                string sourceFile = System.IO.Path.Combine(pathFTP, goodFileName);
                if (System.IO.File.Exists(sourceFile) == false)
                {
                    db.message = "File Name Not Found";
                    return Json(new JavaScriptSerializer().Serialize(db), JsonRequestBehavior.AllowGet);
                }
                string folderDate = "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string folderpath = System.IO.Path.Combine(pathZipfilesContainer, folderDate);
                Directory.CreateDirectory(folderpath);
                string destFile = System.IO.Path.Combine(folderpath, goodFileName);
                System.IO.File.Copy(sourceFile, destFile, true);
                if (System.IO.File.Exists(sourceFile))
                {
                    System.IO.File.Delete(sourceFile);
                }

                string unZipFolderpath = System.IO.Path.Combine(pathUnZipfilesContainer, folderDate);
                Directory.CreateDirectory(unZipFolderpath);
                using (System.IO.Compression.ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(destFile))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        entry.ExtractToFile(System.IO.Path.Combine(unZipFolderpath, entry.Name));
                    }
                }
                Directory.Delete(System.IO.Path.Combine(pathZipfilesContainer, folderDate), true);

                var filePath = System.IO.Path.Combine(unZipFolderpath, "db.txt");
                string value = System.IO.File.ReadAllText(filePath);
                DirectoryInfo d = new DirectoryInfo(unZipFolderpath);
                FileInfo[] Files = d.GetFiles();
                List<FileInfo> images = new List<FileInfo>();
                foreach (FileInfo item in Files)
                {
                    if (item.Name.Trim().ToLower() == "db.txt")
                    {
                    }
                    else if (item.Name.Trim().ToLower().Contains(".jpg") || item.Name.Trim().ToLower().Contains(".jpeg"))
                    {
                        images.Add(item);
                    }
                }
                getObjects(value, images, folderDate, db);
                //db.message = "All Good";
                Directory.Delete(System.IO.Path.Combine(pathUnZipfilesContainer, folderDate), true);
                return Json(new JavaScriptSerializer().Serialize(db), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                db.message = e.Message;
                return Json(new JavaScriptSerializer().Serialize(db), JsonRequestBehavior.AllowGet);
            }
        }
        private void getObjects(string value, List<FileInfo> images, string folderDate, DB db)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<DB>(value);
                string username = data.name;
                string pass = data.pass;
                db.Id = 1;
                db.name = username;
                db.pass = pass;
                db.projects = new List<Project1>();
                db.levels = new List<Level1>();
                db.rooms = new List<Room1>();
                var _signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
                var result = _signInManager.PasswordSignInAsync(username, pass, false, shouldLockout: false).Result;
                if (result == SignInStatus.Success)
                {
                    db.message = "Good";
                }
                else
                {
                    db.message = "404";
                    return;
                }

                foreach (var p in data.projects)
                {
                    Project project = DataService.FindProject(p.ProjectId);
                    project.ProjectStatus = 1;
                    project.Notes = p.Notes;

                    try
                    {
                        List<string> gt = JsonConvert.DeserializeObject<List<string>>(project.OutsidePictures);


                        gt.AddRange(JsonConvert.DeserializeObject<List<string>>(getPictures(p.OutsidePictures, folderDate, images)));

                        project.OutsidePictures = new JavaScriptSerializer().Serialize(gt);



                    }
                    catch (Exception j)
                    {
                        project.OutsidePictures = getPictures(p.OutsidePictures, folderDate, images);
                    }


                    try
                    {
                        List<string> gt = JsonConvert.DeserializeObject<List<string>>(project.Outside3DPictures);
                        gt.AddRange(JsonConvert.DeserializeObject<List<string>>(getPictures(p.Outside3DPictures, folderDate, images)));
                        project.Outside3DPictures = new JavaScriptSerializer().Serialize(gt);



                    }
                    catch (Exception j)
                    {
                        project.Outside3DPictures = getPictures(p.Outside3DPictures, folderDate, images);
                    }

                    DataService.UpdateProject(project);
                }
                foreach (var l in data.levels)
                {
                    Level levelInfo = new Level();
                    levelInfo.Name = l.Name;
                    levelInfo.ProjectID = l.ProjectId;
                    levelInfo.Status = 1;
                    levelInfo.PicName = l.PicName;
                    getPictures2(l.PicName, folderDate, images);
                    Plan plan = new Models.Plan();
                    plan.PlanAdded = false;
                    levelInfo.PlanId = new JavaScriptSerializer().Serialize(plan);

                    Level exited = null;
                    try
                    {
                        exited = DataService.FindLevel(levelInfo);
                        exited.PicName = exited.PicName.Trim() + ";" + l.PicName;
                        DataService.UpdateLevel(exited);
                    }
                    catch (Exception ex3)
                    {
                    }
                    if (exited == null)
                    {
                        int k = DataService.CreateLevel(levelInfo);
                    }
                    Level a = new Level();
                    a.Name = l.Name;
                    a.ProjectID = l.ProjectId;
                    Level b = DataService.FindLevel(a);

                    foreach (var r in data.rooms)
                    {
                        if (r.LevelId == l.LevelId && r.ProjectId == l.ProjectId)
                        {
                            r.LevelId = b.Id;
                        }
                    }
                }



                foreach (var r in data.rooms)
                {
                    Room roomInfo = new Room();
                    roomInfo.Name = r.Name;
                    roomInfo.ProjectID = r.ProjectId;
                    roomInfo.LevelID = r.LevelId;
                    roomInfo.Status = 1;
                    roomInfo.RoomLength = r.RoomLength;
                    roomInfo.RoomWidth = r.RoomWidth;
                    Plan plan = new Plan();
                    plan.ProjectID = roomInfo.ProjectID;
                    plan.LevelID = roomInfo.LevelID;
                    plan.RoomID = -1;
                    plan.Id = "";
                    plan.Name = "";
                    plan.URLToDelete = "";
                    plan.URLToViewer = "";
                    plan.URLToDownload = "";
                    roomInfo.PlanId = new JavaScriptSerializer().Serialize(plan);


                    List<Picture> pictures = new List<Picture>();
                    roomInfo.PictureList = new JavaScriptSerializer().Serialize(pictures);
                    roomInfo.ConnectorList = getConnectors("");
                    roomInfo.ConnectorList = getConnectors(r.Connectors);
                    roomInfo.Connectors = new JavaScriptSerializer().Serialize(roomInfo.ConnectorList);

                    roomInfo.CenterX = r.CenterX;
                    roomInfo.CenterY = r.CenterY;
                    roomInfo.ScaleX = r.ScaleX;
                    roomInfo.ScaleY = r.ScaleY;
                    roomInfo.Rotation = r.Rotation;
                    roomInfo.Shape = r.Shape;
                    roomInfo.Fliped = r.Fliped;


                    List<Room> allRoom = DataService.ReadAllRooms(r.ProjectId, "");
                    Room exited = null;
                    foreach (Room j in allRoom)
                    {
                        if (j.ProjectID == r.ProjectId && j.LevelID == r.LevelId && j.Name.Trim().ToUpper().Equals(r.Name.Trim().ToUpper()))
                        {
                            exited = j;
                            break;
                        }
                    }

                    if (exited == null)
                    {
                        int k = DataService.CreateRoom(roomInfo);
                    }
                    else
                    {
                        DeleteRoomProperties(exited);
                        DataService.DeleteRoom(exited.Id);
                        int k = DataService.CreateRoom(roomInfo);
                    }
                    Room m = new Room();
                    m.Name = r.Name;
                    m.LevelID = r.LevelId;
                    m.ProjectID = r.ProjectId;
                    Room m2 = DataService.FindRoom(m);
                    string[] nameList = r.PictureName.Split(',');
                    List<string> imageNameList = new List<string>();

                    foreach (string n in nameList)
                    {
                        imageNameList.Add(getImageName(n));
                    }

                    //string fileName = getImageName(r.PictureName);
                    m2.Pictures = getPictures(imageNameList, images, r, folderDate, m2);


                    m2.PictureList = new JavaScriptSerializer().Serialize(m2.Pictures);
                    DataService.UpdateRoom(m2);
                }


                List<Project> plist = DataService.ReadAllProjectForUserFielder(username.Trim().ToUpper()).FindAll(x => x.ProjectStatus == 0 || x.ProjectStatus == 2).ToList<Project>();
                db.projects = getDBProject(plist);
                //foreach(Project p in plist)
                //{
                //    List<Level> levels = DataService.ReadAllLevels(p.Id, "").FindAll(x => x.ProjectID == p.Id).ToList<Level>();
                //    db.levels.AddRange(getDBLevels(levels));

                //    List<Room> rooms = DataService.ReadAllRooms(p.Id, "").FindAll(x => x.ProjectID == p.Id).ToList<Room>();
                //    db.rooms.AddRange(getDBRooms(rooms, p));
                //}  
                db.message = "Good";
            }
            catch (Exception ex)
            {
                //db.message = ex.Message;
            }
        }
        private List<Room1> getDBRooms(List<Room> rooms, Project project)
        {
            List<Room1> list = new List<Room1>();
            foreach (Room p in rooms)
            {
                p.ConnectorList = JsonConvert.DeserializeObject<List<string>>(p.Connectors);
                Room1 k = new Room1();
                k.Id = -20;
                k.RoomId = p.Id;
                k.LevelId = p.LevelID;
                k.ProjectId = p.ProjectID;
                k.Name = p.Name;
                k.PictureName = "";
                k.RoomLength = p.RoomLength;
                k.RoomWidth = p.RoomWidth;
                k.Connectors = string.Join("_", p.ConnectorList.ToArray());
                k.Address = project.Address;
                k.State = project.State;
                k.City = project.City;
                k.ZIP = project.Zip;
                Level level = DataService.FindLevel(p.LevelID, project.Id);
                k.LevelName = level.Name;
                list.Add(k);
            }
            return list;
        }
        private List<Level1> getDBLevels(List<Level> levels)
        {
            List<Level1> list = new List<Level1>();
            foreach (Level p in levels)
            {
                Level1 k = new Level1();
                k.Id = -20;
                k.LevelId = p.Id;
                k.ProjectId = p.Id;
                k.Name = p.Name;
                k.Status = p.Status;
                k.Status2 = "Created";
                list.Add(k);
            }
            return list;
        }
        private List<Project1> getDBProject(List<Project> plist)
        {
            List<Project1> list = new List<Project1>();
            foreach (Project p in plist)
            {
                Project1 k = new Project1();
                k.Id = -20;
                k.ProjectId = p.Id;
                k.Address = p.Address;
                k.City = p.City;
                k.ZIPCode = p.Zip;
                k.State = p.State;
                k.Status = p.ProjectStatus;
                k.Status2 = GetStatusName(p.ProjectStatus);
                k.Notes = p.Notes;
                k.Completed = "No";
                k.OutsidePictures = "";
                k.Resolution = p.Resolution;
                k.Outside3DPictures = "";
                list.Add(k);
            }
            return list;
        }
        private string getImageName(string name)
        {
            int index = (name.LastIndexOf('/')) + 1;
            return name.Substring(index);
        }
        private List<Picture> getPictures(List<string> imageNameList, List<FileInfo> images, Room1 r, string folderDate, Room m2)
        {
            for (var j = 0; j < imageNameList.Count; j++)
            {
                imageNameList[j].Trim();
            }
            List<Picture> list = new List<Picture>();
            int count = 1;

            foreach (var name in imageNameList)
            {

                foreach (FileInfo item in images)
                {
                    if (name.Trim() == item.Name.Trim())
                    {
                        string picId = "";
                        Project p5 = new Project();
                        p5.Id = -9999;
                        foreach (Project j in OldDB)
                        {
                            if (j.Id == r.ProjectId)
                            {
                                p5 = j;
                            }
                        }
                        if (p5.Id != -9999)
                        {
                            if (p5.ProjectStatus == 0)
                            {
                                picId = "Project_" + r.ProjectId + "_Level_" + r.LevelId + "_Room_" + m2.Id + "_Picture_1" + count + ".jpg";

                            }
                            else
                            {
                                picId = "Project_" + r.ProjectId + "_Level_" + r.LevelId + "_Room_" + m2.Id + "_Picture_2" + count + ".jpg";
                            }
                        }
                        else
                        {
                            picId = "Project_" + r.ProjectId + "_Level_" + r.LevelId + "_Room_" + m2.Id + "_Picture_1" + count + ".jpg";

                        }
                        string p = System.IO.Path.Combine(pathUnZipfilesContainer, folderDate);
                        System.IO.File.Copy(System.IO.Path.Combine(p, item.Name), Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), picId));
                        Picture pic = new Picture();
                        pic.ProjectID = r.ProjectId;
                        pic.LevelID = r.LevelId;
                        pic.RoomID = m2.Id;
                        pic.Id = picId;
                        pic.Name = "Picture " + count;
                        pic.URLToDelete = m2.ProjectID + "_" + m2.LevelID + "_" + m2.Id + "_" + 1;
                        pic.URLToViewer = "https://www.yahoo.com/" + pic.Id;
                        list.Add(pic);
                        count = count + 1;
                    }

                }
            }
            return list;
        }
        private string getPictures(string fileNames, string folderDate, List<FileInfo> images)
        {
            List<string> found = new List<string>();
            try
            {
                string[] names = fileNames.Split(',');
                List<string> picNames = new List<string>();
                //List<string> picNames = new List<string>();
                foreach (string name in names)
                {
                    picNames.Add(name.Trim());
                }

                string p = System.IO.Path.Combine(pathUnZipfilesContainer, folderDate);

                foreach (FileInfo f in images)
                {
                    if (picNames.Contains(f.Name.Trim()))
                    {
                        try
                        {
                            DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), f.Name));
                        }
                        catch (Exception u)
                        {
                        }
                        System.IO.File.Copy(System.IO.Path.Combine(p, f.Name), Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), f.Name));
                        found.Add(f.Name);
                    }
                }
            }
            catch (Exception e)
            {
                int k = 0;
            }

            return (new JavaScriptSerializer().Serialize(found));

        }
        private List<string> getConnectors(string connectors)
        {
            if (connectors.Trim().Length == 0)
            {
                return new List<string>();
            }

            string[] list = connectors.Split(',');
            return list.ToList<string>();
        }
        private bool checkLogin(string email, string pass)
        {
            AppUser user = DataService.GetUser(email.Trim().ToUpper());
            if (user == null || user.Permission < 1)
            {
                return false;
            }
            return true;
        }
        private void getPictures2(string fileName, string folderDate, List<FileInfo> images)
        {
            string p = System.IO.Path.Combine(pathUnZipfilesContainer, folderDate);
            foreach (FileInfo f in images)
            {
                if (fileName.Contains(f.Name.Trim()))
                {
                    try
                    {
                        DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), f.Name));
                    }
                    catch (Exception u)
                    {

                    }
                    System.IO.File.Copy(System.IO.Path.Combine(p, f.Name), Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), f.Name));
                }
            }
        }
        [HttpPost]
        public JsonResult LoadPictures(string file, int start)
        {
            if (start == 1)
            {
                string folderDate = file + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string unZipFolderpath = System.IO.Path.Combine(pathUnZipfilesContainer, folderDate);
                Directory.CreateDirectory(unZipFolderpath);
                HttpPostedFileBase attachedFile = Request.Files[0];
                string attachedFileFileName = attachedFile.FileName;
                attachedFile.SaveAs(System.IO.Path.Combine(unZipFolderpath, attachedFileFileName));
            }
            else if (start == 2)
            {
                string[] subdirectoryEntries = Directory.GetDirectories(pathUnZipfilesContainer);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    if (subdirectory.Contains(file))
                    {
                        FileInfo f = new FileInfo(subdirectory);
                        string path = System.IO.Path.Combine(pathUnZipfilesContainer, f.Name);
                        HttpPostedFileBase attachedFile = Request.Files[0];
                        string attachedFileFileName = attachedFile.FileName;
                        attachedFile.SaveAs(System.IO.Path.Combine(path, attachedFileFileName));
                        break;
                    }
                }
            }

            string url = Request.Url.ToString();
            return Json(new JavaScriptSerializer().Serialize(url), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LoadDBIOS(string file)
        {
            var bodyStream = new StreamReader(Request.InputStream);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var value = bodyStream.ReadToEnd();
            var db = DBIOS(value, file);
            //string url = Request.Url.ToString();
            //return Json(new JavaScriptSerializer().Serialize(value), JsonRequestBehavior.AllowGet);
            //Response.ContentType = "application/json; charset=utf-8";

            //string json = JavaScriptSerializer.Serialize(new { results = resultRows });
            // return Json(new JavaScriptSerializer().Serialize(db), JsonRequestBehavior.AllowGet);
            return Json(db, JsonRequestBehavior.AllowGet);
        }

        public DB DBIOS(string value, string file)
        {
            OldDB.Clear();
            OldDB = DataService.ReadAllProject();
            DB db = new DB();
            db.message = "Good";
            db.Id = -1;
            db.name = "";
            db.pass = "";
            db.projects = new List<Project1>();
            db.levels = new List<Level1>();
            db.rooms = new List<Room1>();
            try
            {
                bool isFound = false;
                string[] subdirectoryEntries = Directory.GetDirectories(pathUnZipfilesContainer);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    if (subdirectory.Contains(file))
                    {
                        isFound = true;
                        FileInfo f = new FileInfo(subdirectory);
                        string unZipFolderpath = System.IO.Path.Combine(pathUnZipfilesContainer, f.Name);

                        DirectoryInfo d = new DirectoryInfo(unZipFolderpath);
                        FileInfo[] Files = d.GetFiles();
                        List<FileInfo> images = new List<FileInfo>();
                        foreach (FileInfo item in Files)
                        {
                            if (item.Name.Trim().ToLower().Contains(".jpg") || item.Name.Trim().ToLower().Contains(".jpeg"))
                            {
                                images.Add(item);
                            }
                        }
                        getObjects(value, images, f.Name, db);
                        Directory.Delete(System.IO.Path.Combine(pathUnZipfilesContainer, f.Name), true);
                        break;
                    }
                }
                if (isFound == false)
                {
                    List<FileInfo> images = new List<FileInfo>();
                    getObjects(value, images, "", db);
                }
            }
            catch (Exception ex)
            {
                db.message = ex.Message;
            }
            return db;
        }

        [HttpPost]
        public JsonResult AddTour(int projectId, string src)
        {
            var iframe = DataService.FindIFrame(projectId);
            if (iframe == null)
            {
                iframe = new IFrame();
                iframe.ProjectId = projectId;
                iframe.Url = src;
                iframe.Id = -1;
                DataService.CreateAddIFrame(iframe);
            }
            else
            {
                iframe.Url = src;
                DataService.CreateAddIFrame(iframe);
            }
            return Json("Good", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddRoomTour(int pId, int lId, int rId, string src)
        {
            Room r = DataService.FindRoom(rId, pId);
            var iframe = new IRoomFrame();
            iframe.RoomId = rId;
            iframe.LevelId = lId;
            iframe.ProjectId = pId;
            iframe.Url = src;
            iframe.Id = -1;
            iframe.Name = r.LevelName + ": " +r.Name;

            var iframe2 = DataService.FindRoomIFrame(iframe);
            if (iframe2 == null)
            {           
                DataService.CreateAddRoomIFrame(iframe);
            }
            else
            {
                iframe2.Url = src;
                DataService.CreateAddRoomIFrame(iframe2);
            }
            return Json("Good", JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetIFrame(int projectId)
        {
            var iframe = DataService.FindIFrame(projectId);
            if (iframe == null)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(iframe.Url, JsonRequestBehavior.AllowGet);
            }           
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult CleanServer()
        {
            List<int> list = new List<int>();
            List<Project> projects = DataService.ReadAllProject();
            foreach(var p in projects)
            {
                list.Add(p.Id);
            }

            DirectoryInfo d = new DirectoryInfo(HostingEnvironment.MapPath("~/UploadedPictureFiles"));
            FileInfo[] Files = d.GetFiles();
            foreach (FileInfo item in Files)
            {
                var name = item.Name.Trim().ToLower();
                if (name.Contains("project")){
                    var parts = item.Name.Split('_');
                    int result = Int32.Parse(parts[1]);
                    if (list.Contains(result))
                    {
                    }
                    else
                    {
                        DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), item.Name));
                    }
                }
                else if (name.Contains("_out_"))
                {
                    var parts = item.Name.Split('_');
                    int result = Int32.Parse(parts[1]);
                    if (list.Contains(result))
                    {
                    }
                    else
                    {
                        DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), item.Name));
                    }
                }

                else if (name.Contains("_pro_"))
                {
                    var parts = item.Name.Split('_');
                    int result = Int32.Parse(parts[2]);
                    if (list.Contains(result))
                    {
                    }
                    else
                    {
                        DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), item.Name));
                    }
                }

                else if (name.Contains("pro_"))
                {
                    var parts = item.Name.Split('_');
                    int result = Int32.Parse(parts[1]);
                    if (list.Contains(result))
                    {
                    }
                    else
                    {
                        DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), item.Name));
                    }
                }
            }
            return Json("Folders Cleaned", JsonRequestBehavior.AllowGet);           
        }

        public ActionResult GetPhotos(int projectId)
        {
            var bundle = "bundle" + projectId + ".zip";
            if (System.IO.File.Exists(HostingEnvironment.MapPath("~/UploadedPictureFiles/" + bundle)))
            {
                System.IO.File.Delete(HostingEnvironment.MapPath("~/UploadedPictureFiles/" + bundle));
            }
            ZipArchive zip = ZipFile.Open(HostingEnvironment.MapPath("~/UploadedPictureFiles/" + bundle), ZipArchiveMode.Create);
            DirectoryInfo d = new DirectoryInfo(HostingEnvironment.MapPath("~/UploadedPictureFiles"));
            FileInfo[] Files = d.GetFiles();
            foreach (FileInfo item in Files)
            {
                var name = item.Name;
                name = name.Trim().ToLower();
                if (name.Contains(".jpg") || name.ToLower().Contains(".jpeg"))
                {
                    if (name.Contains("project_") || item.Name.StartsWith("_pro_"))
                    {
                        var parts = item.Name.Split('_');
                        int proId = Int32.Parse(parts[1]);
                        if (proId == projectId)
                        {
                            zip.CreateEntryFromFile(HostingEnvironment.MapPath("~/UploadedPictureFiles/" + item.Name), item.Name);
                        }
                    }   
                }
            }
            zip.Dispose();
            var file = File(HostingEnvironment.MapPath("~/UploadedPictureFiles/" + bundle), "application/zip", bundle);
            return file;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetPhotosDeleteFile(int projectId)
        {
            var bundle = "bundle" + projectId + ".zip";
            if (System.IO.File.Exists(HostingEnvironment.MapPath("~/UploadedPictureFiles/" + bundle)))
            {
                System.IO.File.Delete(HostingEnvironment.MapPath("~/UploadedPictureFiles/" + bundle));
            }
            return Json("Good", JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult ReadAllIRoomFrames(int projectId)
        {
            List<IRoomFrame> list = DataService.ReadAllIRoomFrame(projectId);
            if (list == null)
            {
                list = new List<IRoomFrame>();
            }        
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult ExpiredProjectDelete()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json("Invalid User", JsonRequestBehavior.AllowGet);
            }
            AppUser user = DataService.GetUser(User.Identity.Name.Trim().ToUpper());
            if (user == null || user.Permission != 3)
            {
                return Json("Invalid User", JsonRequestBehavior.AllowGet);
            }
            List<Project> projects = DataService.ReadAllProject();
            foreach(var p in projects)
            {
                if(p.ProjectStatus == 5)
                {
                    var frame = DataService.FindIFrame(p.Id);
                    DateTime start = frame.DateInserted;
                    DateTime end = DateTime.Today;
                    TimeSpan difference = end - start;
                    if (difference.Days> 90)
                    {
                        DataService.DeleteProject(p.Id);
                    }    
                }
            }
            return Json("Database Cleaned", JsonRequestBehavior.AllowGet);
        }

        public void CleanServer2(int id)
        {
            DirectoryInfo d = new DirectoryInfo(HostingEnvironment.MapPath("~/UploadedPictureFiles"));
            FileInfo[] Files = d.GetFiles();
            foreach (FileInfo item in Files)
            {
                string name = item.Name.Trim().ToLower();
                string a = "_pro_"+ id + "_3d";
                string b = "pro_" + id + "_";
                string c = "project_" + id + "_level";
                string f = "proj_" + id + "_out_";
                if (name.Contains(a) || name.Contains(b) || name.Contains(c) || name.Contains(f)) { 
                   DeleteFiles(Path.Combine(HostingEnvironment.MapPath("~/UploadedPictureFiles"), item.Name));   
                }                
            }
        }

        public void SendAsync(string email)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return;
            }

            var loginInfo = DataService.ReadAllEmailNotification();
            var notifierEmail = loginInfo[0].Email.Trim();
            string fromPassword = Decrypt(loginInfo[0].Login.Trim());
            var fromAddress = new MailAddress(notifierEmail, "NVMS");
            var toAddress = new MailAddress(email, email);
            string subject = "Approval Notice";
            string body = "Hi,\nYou have been approved by NVMS to use our AllView360 App.\nThanks";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587
            };
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromAddress.Address, fromPassword);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;
            smtp.Timeout = 20000;
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        public string Crypt(string text)
        {
            byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);   
            byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Convert.ToBase64String(outputBuffer);
           
        }
        public  string Decrypt(string text)
        {
            try
            {
                byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
                byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
                SymmetricAlgorithm algorithm = DES.Create();
                ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
                byte[] inputbuffer = Convert.FromBase64String(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Encoding.Unicode.GetString(outputBuffer);
            }
            catch(Exception ex)
            {
                return "UnableToDecrypt";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult UpdateEmailNotifier(string email, string password)
        {
            string pass = Crypt(password);
            var loginInfo = DataService.ReadAllEmailNotification();
            var notifier = new EmailNotification();
            if (loginInfo.Count == 0)
            {                
                notifier.Id = -1;
                notifier.Email = email;
                notifier.Login = pass;
            }
            else
            {
                notifier = loginInfo[0];
                notifier.Email = email;
                notifier.Login = pass;
            }
            try
            {
                DataService.AddEmailNotification(notifier);
            }
            catch(Exception tx)
            {
                return Json("Bad", JsonRequestBehavior.AllowGet);
            }           
            return Json("Good", JsonRequestBehavior.AllowGet);
        }
    }
}