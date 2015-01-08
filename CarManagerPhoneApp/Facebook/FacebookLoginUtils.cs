using System.Threading.Tasks;
using Facebook.Client;
using Microsoft.Phone.Controls;

namespace CarManagerPhoneApp.Facebook
{
    public class FacebookLoginUtils
    {
        private FacebookSession session;
        public FacebookSessionClient SessionClient;
        public string AccessToken {
            get { return session.AccessToken; } }
        public string FacebookId {
            get { return session.FacebookId; }  }
        public const string FacebookAppId = "1458839147697033";
        public FacebookLoginUtils()
        {
               SessionClient = new FacebookSessionClient(FacebookAppId);
        }
        public async void ResetFacebookUser()
        {
            SessionClient.Logout();
            await new WebBrowser().ClearCookiesAsync();
        }


        public async Task Login()
        {
            session = await SessionClient.LoginAsync("user_about_me");
        }



    }
}
