using System.Web.Security;
using MusicStore.WebUI.Infrastructure.Abstract;
namespace MusicStore.WebUI.Infrastructure.Concrete
{
    public class FormsAuthProvider : IAuthProvider
    {
        public bool Authenticate(string username, string password)
        {
            bool result = FormsAuthentication.Authenticate(username, password);
            if (result)
            {
                FormsAuthentication.SetAuthCookie(username, false);
            }
            return result;
        }

        public void Logout()
        {
            FormsAuthentication.SignOut();
        }
    }
}