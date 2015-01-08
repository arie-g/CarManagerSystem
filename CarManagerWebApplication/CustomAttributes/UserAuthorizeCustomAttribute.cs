using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http.Filters;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WebMatrix.WebData;

namespace CarManagerWebApplication.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UserAuthorizeCustomAttribute : AuthorizeAttribute 
    {
        private readonly string[] validRoles;
        public UserAuthorizeCustomAttribute(params string[] roles)
        {
            this.validRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //Getting the current request here, which will have the requested URL.
            var request = HttpContext.Current.Request;
            //From request object, get the original path the user entered in the url, before routing is applied, to get the company name.
            string originalUrlPath = request.RawUrl;
            //Initialize Authorization to true
            bool isAuthorized = false;


            if (httpContext.User.Identity.IsAuthenticated)
            {
                //Check Whether the user is logged in first.

                if (originalUrlPath.Trim() == string.Empty || originalUrlPath.Trim().ToLower() == "home/index")
                {
                    //What need to do If the user is logged in and directly access home/index or home url???? Do that logic here. 
                    //For now I return true for such cases.
                    isAuthorized = true;

                }
                else
                {
                    //Get the company name from first position
                    string companyName = originalUrlPath.Split('/')[1]; //URL Form is : /CompanyName/Home/Index
                    // Using above companyName get the companyID as you mentioned.

                    Guid? companyId = RoleService.GetCompanyId(companyName);
                    if (companyId.HasValue)
                    {
                        //Define Your Entity
                        //var carManagerEntity = new CarManagerEntity();
                        foreach (string role in validRoles)
                        {
                            int UserId = WebSecurity.CurrentUserId; 

                            if (UserId != default(int))
                            {
                                var roleID = RoleService.GetRoleID(role.ToLower());
                                if (roleID.HasValue)
                                {
                                    isAuthorized = UsersManager.IsUserAuthorized(UserId, companyId.Value, roleID.Value);
                                }
                                else
                                {
                                    isAuthorized = false;
                                }
                            }
                            else
                            {
                                isAuthorized = false;
                            }
                        }
                    }
                }
            }
            return isAuthorized;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }  
    }
}