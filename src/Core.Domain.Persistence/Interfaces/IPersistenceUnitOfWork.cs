using System;
using System.Threading.Tasks;
using Core.Domain.Persistence.Entities;

namespace Core.Domain.Persistence.Interfaces
{
    public interface IPersistenceUnitOfWork : IDisposable
    {

        public IPostRepositoryAsync Post { get; }
        public ICategoryRepositoryAsync Category { get; }
        public ITagRepositoryAsync Tag { get; }
        public IRepositoryAsync<Comment> Comment { get; }
        Task<int> CommitAsync();
    }
}