using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Persistence.Interfaces;

public interface IRepositoryAsync<T> where T : class
{
    /// <summary>
    ///     Iqueryable entity of Entity Framework. Use this to execute query in database level.\n
    /// </summary>
    IQueryable<T> Entity { get; }

    Task<IReadOnlyList<T>> GetAllAsync();

    Task<T> AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);
}
