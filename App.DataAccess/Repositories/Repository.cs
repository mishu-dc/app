using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Z.EntityFramework.Plus;

namespace App.DataAccess.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        internal readonly DbContext _context;
        internal readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }
        public void Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IList<TEntity> entities)
        {
            _context.Set<TEntity>().AddRange(entities);
        }

        public void Remove(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(List<TEntity> entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<TEntity?> GetByIdAsync(object id, string includeCollections = "", string includeReferences = "")
        {
            var model = await _dbSet.FindAsync(id);

            if (model != null)
            {
                foreach (var include in includeCollections.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    _context.Entry(model).Collection(include).Load();
                }

                foreach (var include in includeReferences.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    _context.Entry(model).Reference(include).Load();
                }
            }

            return model;
        }

        public async Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = BuildQuery(new List<Expression<Func<TEntity, bool>>>() { filter }, orderBy, includeProperties);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            string includeProperties = "",
            int? pageOffset = null,
            int? pageSize = null)
        {
            return await GetAsync(new List<Expression<Func<TEntity, bool>>> { filter }, orderBy, includeProperties, pageOffset, pageSize);
        }

        public async Task<List<TEntity>> GetAsync(IList<Expression<Func<TEntity, bool>>> filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            int? pageOffset = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = BuildQuery(filter, orderBy, includeProperties);

            if (pageOffset.HasValue && pageSize.HasValue)
            {
                query = query.Skip(pageOffset.Value).Take(pageSize.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> filter,
           string? queryHint = null,
           bool useFuture = false)
        {
            IQueryable<TEntity> query = BuildQuery(new List<Expression<Func<TEntity, bool>>> { filter }, queryHint: queryHint);
            return await FindCountAsync(query, useFuture);
        }

        private async Task<int> FindCountAsync(IQueryable<TEntity> query, bool useFuture = false)
        {
            if (useFuture)
            {
                return query.DeferredCount().FutureValue();
            }
            return await query.CountAsync();
        }

        public Task<int> ExecuteSP(string spName, string[] parameters, int timeoutSeconds = 90)
        {
            string paramVars = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                paramVars += $", @p{i}";
            }
            if (paramVars != "")
            {
                paramVars = paramVars.Substring(2);
            }
            _context.Database.SetCommandTimeout(timeoutSeconds);
            return _context.Database.ExecuteSqlRawAsync($"{spName} {paramVars}", parameters);
        }

        protected IQueryable<TEntity> BuildQuery(
            IList<Expression<Func<TEntity, bool>>>? filterList = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            string? queryHint = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filterList != null)
            {
                foreach (var lamdaFilter in filterList)
                {
                    query = query.Where(lamdaFilter);
                }
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (queryHint != null)
            {
                query = query.TagWith(queryHint);
            }

            return query;
        }
    }
}
