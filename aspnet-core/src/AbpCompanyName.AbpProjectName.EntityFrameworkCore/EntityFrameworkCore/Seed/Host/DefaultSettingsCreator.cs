using Abp.Configuration;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Net.Mail;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AbpCompanyName.AbpProjectName.EntityFrameworkCore.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly AbpProjectNameDbContext _context;

        public DefaultSettingsCreator(AbpProjectNameDbContext context)
        {
            _context = context;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "By design.")]
        public void Create()
        {
            int? tenantId = AbpProjectNameConsts.MultiTenancyEnabled ? null : MultiTenancyConsts.DefaultTenantId;

            // Emailing
            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "admin@mydomain.com", tenantId);
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "mydomain.com mailer", tenantId);

            // Languages
            AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "en", tenantId);
        }

        private void AddSettingIfNotExists(string name, string value, int? tenantId = null)
        {
            if (_context.Settings.IgnoreQueryFilters().Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == null))
            {
                return;
            }

            _ = _context.Settings.Add(new Setting(tenantId, null, name, value));
            _ = _context.SaveChanges();
        }
    }
}
