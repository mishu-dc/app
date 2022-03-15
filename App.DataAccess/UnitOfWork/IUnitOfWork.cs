namespace App.DataAccess.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        void SetUserName(string name);
        Task<int> SaveAsync();
    }
}
