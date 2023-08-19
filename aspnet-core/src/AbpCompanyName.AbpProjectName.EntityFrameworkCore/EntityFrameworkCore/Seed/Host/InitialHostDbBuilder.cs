﻿namespace AbpCompanyName.AbpProjectName.EntityFrameworkCore.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly AbpProjectNameDbContext _context;

        public InitialHostDbBuilder(AbpProjectNameDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultEditionCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create();

            _ = _context.SaveChanges();
        }
    }
}
