using System.Web;
using System.Web.Mvc;

namespace Ja.Mvc.SignalR.Phidget
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
