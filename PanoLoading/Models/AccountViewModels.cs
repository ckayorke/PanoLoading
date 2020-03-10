using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PanoLoading.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }


    public class CreateProjectViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "ZIPCode")]
        public string ZIPCode { get; set; }

        [Required]
        [Display(Name = "State")]
        public string ProjectState { get; set; }

        [Required]
        [Display(Name = "Fielder")]
        public string Fielder { get; set; }

        [Required]
        [Display(Name = "Drawer")]
        public string Drawer { get; set; }

       
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        public List<SelectListItem> ProjectStates { get; set; }
        public List<SelectListItem> Fielders { get; set; }
        public int Valid { get; set; }

        
        [Required]
        [Display(Name = "Resolution")]
        public string Resolution { get; set; }
        public List<SelectListItem> Resolutions { get; set; }

    }


    public class ModifyProjectViewModel
    {
        [Required]
        [Display(Name = "State")]
        public string ProjectState { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string ProjectAddress { get; set; }

        [Required]
        [Display(Name = "City")]
        public string ProjectCity { get; set; }

        [Required]
        [Display(Name = "ZIPCode")]
        public string ProjectZIPCode { get; set; }

        [Required]
        [Display(Name = "Fielder")]
        public string Fielder { get; set; }
        public List<SelectListItem> Fielders { get; set; }

        public List<SelectListItem> ProjectStates { get; set; }
        public List<SelectListItem> ProjectCities { get; set; }
        public List<SelectListItem> ProjectZIPS { get; set; }
        public List<SelectListItem> ProjectAddresses { get; set; }
        public Project project { get; set; }

        public AppUser AppUser { get; set; }
        public bool HasUser { get; set; }
    }


    public class LevelViewModel
    {   public Project _Project { get; set; }
        public List<Level> Levels { get; set; }
        public List<SelectListItem> LevelNames { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        public AppUser AppUser { get; set; }
        public bool HasUser { get; set; }
        public string AllImages { get; set; }
        public string CustomerViewerUrl { get; set; }
        public List<string> EntranceUrls { get; set; }

    }

    public class LevelDetailsViewModel
    {
        public List<Level> Levels { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
    } 


    public class RoomViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        public Level Level { get; set; }
        public AppUser AppUser { get; set; }
        public bool HasUser { get; set; }
        public List<Room> Rooms { get; set; }
        public List<SelectListItem> RoomNames { get; set; }
    }

    public class RoomDetailsViewModel
    {
        [Required]
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        public Room Room { get; set; }

        public AppUser AppUser { get; set; }
        public bool HasUser { get; set; }

        [Display(Name = "Neigbor")]
        public string Neigbor { get; set; }
        public List<SelectListItem> RoomNamesList { get; set; }
    }

    public class PlanViewModel
    {       
        [Required]
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Room")]
        public Room Room { get; set; }
    }


    public class DecisionViewModel
    {       
        public AppUser AppUser { get; set; }
        public bool HasUser { get; set; }
    }

    public class ProjectListViewModel
    {
        public AppUser AppUser { get; set; }
        public bool HasUser { get; set; }
        public List<Project> Projects { get; set; }        
    }

    public class ViewerViewModel
    {
  
        [Display(Name = "URL")]
        public string URL { get; set; }

        [Display(Name = "3DRoom")]
        public string ha3D { get; set; }

        [Display(Name = "Info")]
        public string Info { get; set; }

        [Display(Name = "FloorURL")]
        public string FloorURL { get; set; }
    }

    public class CustomerViewerViewModel
    {

        [Display(Name = "URL")]
        public string URL { get; set; }
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "FloorURL")]
        public string FloorURL { get; set; }
        public List<IRoomFrame2> RoomURLs { get; set; }
    }

    public class EntranceViewerViewModel
    {
        [Display(Name = "URL")]
        public string URL { get; set; }
    }
}
