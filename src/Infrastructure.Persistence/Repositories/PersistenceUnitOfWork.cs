using System;
using System.Threading.Tasks;
using Core.Domain.Persistence.Entities;
using Core.Domain.Persistence.Interfaces;
using Infrastructure.Persistence.Context;
namespace Infrastructure.Persistence.Repositories
{
    public class PersistenceUnitOfWork : IPersistenceUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private bool _disposed;
        public IPostRepositoryAsync Post { get; }
        public ICategoryRepositoryAsync Category { get; }
        public ITagRepositoryAsync Tag { get; }
        public IRepositoryAsync<Comment> Comment { get; }
        public PersistenceUnitOfWork(AppDbContext appDbContext,
                                        IPostRepositoryAsync post, 
                                        IRepositoryAsync<Comment> comment,
                                        ICategoryRepositoryAsync category,
                                        ITagRepositoryAsync tag)
        {
            _dbContext = appDbContext;
            Post = post;
            Comment = comment;
            Category = category;
            Tag = tag;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<int> CommitAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing) _dbContext.Dispose();
            _disposed = true;
        }
    }
}
