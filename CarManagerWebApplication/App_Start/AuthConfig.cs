using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;

namespace CarManagerWebApplication
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

             WebSecurity.InitializeDatabaseConnection("DefaultConnection", "User", "Id", "UserName", autoCreateTables: true);
            
            OAuthWebSecurity.RegisterFacebookClient(
                appId: "1458839147697033",
                appSecret: "5e6a15d035214cfa0351be19a4bc5249");

           //OAuthWebSecurity.RegisterGoogleClient();


        }
    }
}
