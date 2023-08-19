using Abp.AspNetCore.TestBase;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.Json;
using Abp.MultiTenancy;
using Abp.Web.Models;
using AbpCompanyName.AbpProjectName.EntityFrameworkCore;
using AbpCompanyName.AbpProjectName.Models.TokenAuth;
using AbpCompanyName.AbpProjectName.Web.Startup;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUglify.Helpers;
using Shouldly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AbpCompanyName.AbpProjectName.Web.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "By design")]
    public abstract class AbpProjectNameWebTestBase : AbpAspNetCoreIntegratedTestBase<Startup>
    {
        protected static readonly Lazy<string> ContentRootFolder;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "By design")]
        static AbpProjectNameWebTestBase() => ContentRootFolder = new Lazy<string>(WebContentDirectoryFinder.CalculateContentRootFolder, true);

        protected static IHtmlDocument ParseHtml(string htmlString)
        {
            return new HtmlParser().ParseDocument(htmlString);
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return base
                .CreateWebHostBuilder()
                .UseContentRoot(ContentRootFolder.Value)
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(AbpProjectNameWebMvcModule).Assembly.FullName);
        }

        protected async Task<T> GetResponseAsObjectAsync<T>(
            string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var strResponse = await GetResponseAsStringAsync(url, expectedStatusCode).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(strResponse, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        protected async Task<string> GetResponseAsStringAsync(
            string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var response = await GetResponseAsync(url, expectedStatusCode).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        protected async Task<HttpResponseMessage> GetResponseAsync(
            string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var response = await Client.GetAsync(url).ConfigureAwait(false);
            response.StatusCode.ShouldBe(expectedStatusCode);
            return response;
        }

        /// <summary>
        /// /api/TokenAuth/Authenticate
        /// TokenAuthController.
        /// </summary>
        /// <param name="tenancyName">tenancyName.</param>
        /// <param name="input">input.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task AuthenticateAsync(string tenancyName, AuthenticateModel input)
        {
            if (string.IsNullOrWhiteSpace(tenancyName))
            {
                var tenant = UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
                if (tenant != null)
                {
                    AbpSession.TenantId = tenant.Id;
                    Client.DefaultRequestHeaders.Add("Abp.TenantId", tenant.Id.ToStringInvariant());  // Set TenantId
                }
            }

            using var content = new StringContent(input.ToJsonString(), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(
                "/api/TokenAuth/Authenticate",
                content).ConfigureAwait(false);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result =
                JsonConvert.DeserializeObject<AjaxResponse<AuthenticateResultModel>>(
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Result.AccessToken);

            AbpSession.UserId = result.Result.UserId;
        }

        protected void LoginAsHostAdmin()
        {
            LoginAsHost(AbpUserBase.AdminUserName);
        }

        protected void LoginAsDefaultTenantAdmin()
        {
            LoginAsTenant(AbpTenantBase.DefaultTenantName, AbpUserBase.AdminUserName);
        }

        protected void LoginAsHost(string userName)
        {
            AbpSession.TenantId = null;

            var user =
                UsingDbContext(
                    context =>
                        context.Users.FirstOrDefault(u => u.TenantId == AbpSession.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new Exception("There is no user: " + userName + " for host.");
            }

            AbpSession.UserId = user.Id;
        }

        protected void LoginAsTenant(string tenancyName, string userName)
        {
            var tenant = UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
            if (tenant == null)
            {
                throw new Exception("There is no tenant: " + tenancyName);
            }

            AbpSession.TenantId = tenant.Id;

            var user =
                UsingDbContext(
                    context =>
                        context.Users.FirstOrDefault(u => u.TenantId == AbpSession.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new Exception("There is no user: " + userName + " for tenant: " + tenancyName);
            }

            AbpSession.UserId = user.Id;
        }

        protected void UsingDbContext(Action<AbpProjectNameDbContext> action)
        {
            using var context = IocManager.Resolve<AbpProjectNameDbContext>();
            action(context);
            _ = context.SaveChanges();
        }

        protected T UsingDbContext<T>(Func<AbpProjectNameDbContext, T> func)
        {
            T result;

            using (var context = IocManager.Resolve<AbpProjectNameDbContext>())
            {
                result = func(context);
                _ = context.SaveChanges();
            }

            return result;
        }

        protected async Task UsingDbContextAsync(Func<AbpProjectNameDbContext, Task> action)
        {
            using var context = IocManager.Resolve<AbpProjectNameDbContext>();
            await action(context).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(true).ConfigureAwait(false);
        }

        protected async Task<T> UsingDbContextAsync<T>(Func<AbpProjectNameDbContext, Task<T>> func)
        {
            T result;

            using (var context = IocManager.Resolve<AbpProjectNameDbContext>())
            {
                result = await func(context).ConfigureAwait(false);
                _ = await context.SaveChangesAsync(true).ConfigureAwait(false);
            }

            return result;
        }

        protected Task<T> GetResponseAsObjectAsync<T>(Uri url, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            throw new NotImplementedException();
        }

        protected Task<string> GetResponseAsStringAsync(Uri url, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            throw new NotImplementedException();
        }

        protected Task<HttpResponseMessage> GetResponseAsync(Uri url, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            throw new NotImplementedException();
        }
    }
}