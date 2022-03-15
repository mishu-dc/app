using System.Linq.Expressions;

namespace App.DataAccess.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);
        void AddRange(IList<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(List<TEntity> entities);
        Task<TEntity?> GetByIdAsync(object id, string includeCollections = "", string includeReferences = "");
        Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, 
            IOrderedQueryable<TEntity>> orderBy, 
            string includeProperties = "");
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            string includeProperties = "",
            int? pageOffset = null,
            int? pageSize = null);
        Task<List<TEntity>> GetAsync(IList<Expression<Func<TEntity, bool>>> filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            int? pageOffset = null,
            int? pageSize = null);
        Task<int> GetCountAsync(Expression<Func<TEntity, bool>> filter,
           string? queryHint = null,
           bool useFuture = false);
        Task<int> ExecuteSP(string spName, string[] parameters, int timeoutSeconds = 90);
    }
}
