using System.Threading.Tasks;
using StudioX.Dependency;
using StudioX.Domain.Entities;
using StudioX.Domain.Repositories;
using StudioX.EntityFramework.GraphDiff.Mapping;
using StudioX.EntityFramework.Repositories;
using RefactorThis.GraphDiff;

namespace StudioX.EntityFramework.GraphDiff.Extensions
{
    /// <summary>
    /// This class is an extension for GraphDiff library which provides a possibility to attach a detached graphs (i.e. entities) to a context.
    /// Attaching a whole graph using this methods updates all entity's navigation properties on entity creation or modification.
    /// </summary>
    public static class GraphDiffExtensions
    {
        /// <summary>
        /// Attaches an <paramref name="entity"/> (as a detached graph) to a context.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
        /// <param name="repository"></param>
        /// <param name="entity"></param>
        /// <returns>Attached entity</returns>
        public static TEntity AttachGraph<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository, TEntity entity)
            where TEntity : class, IEntity<TPrimaryKey>, new()
        {
            using (var mappingManager = repository.GetIocResolver().ResolveAsDisposable<IEntityMappingManager>())
            {
                var mapping = mappingManager.Object.GetEntityMappingOrNull<TEntity>();
                return repository
                    .GetDbContext()
                    .UpdateGraph(entity, mapping);
            }
        }

        /// <summary>
        /// Attaches an <paramref name="entity"/> (as a detached graph) to a context.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
        /// <param name="repository"></param>
        /// <param name="entity"></param>
        /// <returns>Attached entity</returns>
        public static Task<TEntity> AttachGraphAsync<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository, TEntity entity)
            where TEntity : class, IEntity<TPrimaryKey>, new()
        {
            return Task.FromResult(AttachGraph(repository, entity));
        }

        /// <summary>
        /// Attaches an <paramref name="entity"/> (as a detached graph) to a context and gets it's Id.
        /// It may require to save current unit of work
        /// to be able to retrieve id.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
        /// <param name="repository"></param>
        /// <param name="entity"></param>
        /// <returns>Id of the entity</returns>
        public static TPrimaryKey AttachGraphAndGetId<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository, TEntity entity)
            where TEntity : class, IEntity<TPrimaryKey>, new()
        {
            return AttachGraph(repository, entity).Id;
        }

        /// <summary>
        /// Attaches an <paramref name="entity"/> (as a detached graph) to a context and gets it's Id.
        /// It may require to save current unit of work
        /// to be able to retrieve id.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
        /// <param name="repository"></param>
        /// <param name="entity"></param>
        /// <returns>Id of the entity</returns>
        public static Task<TPrimaryKey> AttachGraphAndGetIdAsync<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository, TEntity entity)
            where TEntity : class, IEntity<TPrimaryKey>, new()
        {
            return Task.FromResult(AttachGraphAndGetId(repository, entity));
        }
    }
}