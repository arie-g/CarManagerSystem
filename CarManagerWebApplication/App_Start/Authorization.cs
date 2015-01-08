using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace CarManagerWebApplication.App_Start
{
    public class Authorization : AuthorizeAttribute
    {
        public string  CompanyName { get; set; }
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var isAuthorised = base.IsAuthorized(actionContext);


            if (isAuthorised)
            {
                var cookie = HttpContext.Current.Request.Cookies
                [FormsAuthentication.FormsCookieName];
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                var identity = new GenericIdentity(ticket.Name);
                string userData = ticket.UserData;
                if (userData.Contains("_"))
                {
                   
                }
            }
        }
    }
}