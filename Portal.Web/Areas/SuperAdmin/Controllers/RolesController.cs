using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Portal.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Portal.Web.Controllers;
using System.Data.Entity.Validation;
using System.Threading.Tasks;

namespace Portal.Web.Areas.SuperAdmin.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [RouteArea("SuperAdmin")]
    public class RolesController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();

        #region ActionResult Index()
        public ActionResult Index()
        {
            var roles = context.Roles.ToList();
            return View(roles);
        }
        #endregion
        
        #region ActionResult Create()
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region Task<ActionResult> Create(FormCollection collection)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FormCollection collection)
        {
            try
            {
                if (string.IsNullOrEmpty(collection["RoleName"].Trim()))
                {
                    throw new InvalidOperationException("Role Name can not be empty");
                }
                context.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
                {
                    Name = collection["RoleName"].Trim()
                });
                await context.SaveChangesAsync();
                ViewBag.ResultMessage = "Role created successfully !";
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        sb.AppendFormat("{0}", validationError.ErrorMessage);
                    }
                }
                ViewBag.ResultMessage = $"Unable to create the Role. Reason is { sb.ToString() }";
            }
            catch (Exception ex)
            {
                ViewBag.ResultMessage = $"Unable to create the Role. Reason is { ex.GetBaseException().Message }";
            }
            return View();
        } 
        #endregion
        
        #region ActionResult Edit(string id)
        public ActionResult Edit(string id)
        {
            var thisRole = context.Roles.Where(r => r.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            return View(thisRole);
        }
        #endregion
        
        #region Task<ActionResult> Edit(Microsoft.AspNet.Identity.EntityFramework.IdentityRole role)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Microsoft.AspNet.Identity.EntityFramework.IdentityRole role)
        {
            try
            {
                if (string.IsNullOrEmpty(role.Name.Trim()))
                {
                    throw new InvalidOperationException("Role Name can not be empty");
                }
                role.Name = role.Name.Trim();
                context.Entry(role).State = System.Data.Entity.EntityState.Modified;
                await context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        sb.AppendFormat("{0}", validationError.ErrorMessage);
                    }
                }
                ViewBag.ResultMessage = $"Unable to update the Role. Reason is { sb.ToString() }";
            }
            catch (Exception ex)
            {
                ViewBag.ResultMessage = $"Unable to update the Role. Reason is { ex.GetBaseException().Message }";
            }
            return View();
        }
        #endregion

        #region Task<ActionResult> Delete(string id)
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var thisRole = context.Roles.Where(r => r.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                //First check whether there are any users associated with this role
                if (thisRole.Users.Any())
                {
                    throw new InvalidOperationException("Users are associated with this Role.");
                }
                context.Roles.Remove(thisRole);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"Unable to delete the Role. Reason is : { ex.GetBaseException().Message }";
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region ActionResult DisplayRoleUsers(string id)
        public ActionResult DisplayRoleUsers(string id)
        {
            var users = context.Users.OrderBy(u => u.Email).ToList();
            ViewBag.Role = context.Roles.Where(r => r.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            return View(users);
        }
        #endregion

        #region async Task<ActionResult> DisplayRoleUsers(string roleId, string operation, string userEmail)
        [Route("Roles/{roleId}/{operation}/{userId}")]
        public async Task<ActionResult> UserRoleUpdate(string roleId, string operation, string userId)
        {
            //First Get the user record
            ApplicationUser user = context.Users.Where(u => u.Id.Equals(userId, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            //Make sure that the user record is found
            if (user != null)
            {
                //Get the role object 
                var thisRole = context.Roles.Where(r => r.Id.Equals(roleId, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                //Make sure that role exists
                if (thisRole != null)
                {
                    //Note: We need to call the overload constructor here becaause the HttpContext only available in a request.
                    var account = new AccountController(HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(), HttpContext.GetOwinContext().Get<ApplicationSignInManager>());
                    if (operation.Equals("add", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await account.UserManager.AddToRoleAsync(user.Id, thisRole.Name);
                    }
                    else if (operation.Equals("remove", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await account.UserManager.RemoveFromRoleAsync(user.Id, thisRole.Name);
                    }
                }
            }
            if(Request.QueryString["from"] == "userupdate")
            {
                return RedirectToAction("Update", new { id = userId});
            }
            return RedirectToAction("DisplayRoleUsers", new { id = roleId });
        }
        #endregion

        #region ActionResult Users()
        public ActionResult Users()
        {
            var users = context.Users.ToList();
            ViewBag.Roles = context.Roles.ToList();
            return View(users);
        }
        #endregion

        #region ActionResult Update(string id)
        /// <summary>
        /// display the view for updating the user roles
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns></returns>
        public ActionResult Update(string id)
        {
            ApplicationUser user = context.Users.Where(u => u.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if(user != null)
            {                
                ViewBag.Roles = context.Roles.ToList();
                return View(user);
            }
            return RedirectToAction("Users");
        } 
        #endregion
        public ActionResult ManageUserRoles()
        {
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleAddToUser(string UserEmail, string RoleName)
        {
            ApplicationUser user = context.Users.Where(u => u.Email.Equals(UserEmail, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            //Note: We need to call the overload constructor here becaause the HttpContext only available in a request.
            var account = new AccountController(HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(), HttpContext.GetOwinContext().Get<ApplicationSignInManager>());
            account.UserManager.AddToRole(user.Id, RoleName);

            ViewBag.ResultMessage = "Role created successfully !";

            // prepopulate the roles for the view dropdown
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;

            return View("ManageUserRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetRoles(string UserEmail)
        {
            if (!string.IsNullOrWhiteSpace(UserEmail))
            {
                ApplicationUser user = context.Users.Where(u => u.Email.Equals(UserEmail, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                //Make sure that the user is found
                if (user != null)
                {
                    //Note: We need to call the overload constructor here becaause the HttpContext only available in a request.
                    var account = new AccountController(HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(), HttpContext.GetOwinContext().Get<ApplicationSignInManager>());

                    ViewBag.RolesForThisUser = account.UserManager.GetRoles(user.Id);
                }                
            }
            // prepopulat roles for the view dropdown
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;

            return View("ManageUserRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleForUser(string UserEmail, string RoleName)
        {
            ApplicationUser user = context.Users.Where(u => u.Email.Equals(UserEmail, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            //Make sure that the user is found
            if (user != null)
            {
                //Note: We need to call the overload constructor here becaause the HttpContext only available in a request.
                var account = new AccountController(HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(), HttpContext.GetOwinContext().Get<ApplicationSignInManager>());

                if (account.UserManager.IsInRole(user.Id, RoleName))
                {
                    account.UserManager.RemoveFromRole(user.Id, RoleName);
                    ViewBag.ResultMessage = "Role removed from this user successfully !";
                }
                else
                {
                    ViewBag.ResultMessage = "This user doesn't belong to selected role.";
                }
            }

            // prepopulat roles for the view dropdown
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;

            return View("ManageUserRoles");
        }
    }
}