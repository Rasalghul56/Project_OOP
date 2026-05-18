using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Confectionery.Data;

namespace Confectionery.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext Context;
        protected readonly DbSet<T> DbSet;

        public Repository(AppDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll()
            => DbSet.ToList();

        public virtual T GetById(int id)
            => DbSet.Find(id);

        public virtual void Add(T entity)
            => DbSet.Add(entity);

        public virtual void Update(T entity)
            => Context.Entry(entity).State = EntityState.Modified;

        public virtual void Delete(int id)
        {
            var entity = DbSet.Find(id);
            if (entity != null)
                DbSet.Remove(entity);
        }
    }
}
