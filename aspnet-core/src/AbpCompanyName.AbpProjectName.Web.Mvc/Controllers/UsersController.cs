﻿using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using AbpCompanyName.AbpProjectName.Authorization;
using AbpCompanyName.AbpProjectName.Controllers;
using AbpCompanyName.AbpProjectName.Users;
using AbpCompanyName.AbpProjectName.Web.Models.Users;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AbpCompanyName.AbpProjectName.Web.Controllers
{
    [AbpMvcAuthorize(PermissionNames.PagesUsers)]
    public class UsersController : AbpProjectNameControllerBase
    {
        private readonly IUserAppService _userAppService;

        public UsersController(IUserAppService userAppService)
        {
            _userAppService = userAppService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var roles = (await _userAppService.GetRoles().ConfigureAwait(false)).Items;
            var model = new UserListViewModel
            {
                Roles = roles
            };
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> EditModal(long userId)
        {
            var user = await _userAppService.GetAsync(new EntityDto<long>(userId)).ConfigureAwait(false);
            var roles = (await _userAppService.GetRoles().ConfigureAwait(false)).Items;
            var model = new EditUserModalViewModel
            {
                User = user,
                Roles = roles
            };
            return PartialView("_EditModal", model);
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }
    }
}
