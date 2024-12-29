using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // _context.Entity.Include("blabla").ToList();
        // _context.Entity.Where(x=>x.Id == Id).ToList();
        IEnumerable<T> GetAll(Expression<Func<T,bool>>? predicate = null, string? IncludeWord = null);

        // _context.Entity.Include("blabla").FirstOrDefault(x=>x.id == id);
        // _context.Entity.Where(x=>x.Id == Id).FirstOrDefault(x=>x.Id == Id);
        T GetFirstOrDefault(Expression<Func<T, bool>>? predicate = null, string? IncludeWord = null);

        // _context.Entity.Add(item);
        void Add(T entity);

        // _context.Entity.Remove(item);
        void Remove(T entity);

        // _context.Entity.RemoveRange(Range);
        void RemoveRange(IEnumerable<T> entities);
    }
}
