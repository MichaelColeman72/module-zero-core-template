using Abp.AspNetCore.Mvc.Authorization;
using AbpCompanyName.AbpProjectName.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AbpCompanyName.AbpProjectName.Web.Controllers
{
    [AbpMvcAuthorize]
    public class AboutController : AbpProjectNameControllerBase
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}
