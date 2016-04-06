using System.Web;
using System.Web.Mvc;

namespace Modulo9_ProvidedHostedWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
