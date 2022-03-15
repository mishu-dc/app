using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace App.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private bool _disposed = false;
        private string? _userName;

        public UnitOfWork(DbContext dbContext)
        {
            _context = dbContext;
            AuditManager.DefaultConfiguration.AutoSavePreAction = (context, audit) =>
            {
            };
            AuditManager.DefaultConfiguration.ExcludeDataAnnotation();
            AuditManager.DefaultConfiguration.DataAnnotationDisplayName();
        }

        public void SetUserName(string name)
        {
            _userName = name;
        }

        public async Task<int> SaveAsync()
        {
            var audit = new Audit();
            if (!string.IsNullOrEmpty(_userName))
            {
                audit.CreatedBy = _userName;
            }
            var result = await _context.SaveChangesAsync(audit);
            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
