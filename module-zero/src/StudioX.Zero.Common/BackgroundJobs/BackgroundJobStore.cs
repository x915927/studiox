﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudioX.Dependency;
using StudioX.Domain.Repositories;
using StudioX.Domain.Uow;
using StudioX.Timing;

namespace StudioX.BackgroundJobs
{
    /// <summary>
    /// Implements <see cref="IBackgroundJobStore"/> using repositories.
    /// </summary>
    public class BackgroundJobStore : IBackgroundJobStore, ITransientDependency
    {
        private readonly IRepository<BackgroundJobInfo, long> backgroundJobRepository;

        public BackgroundJobStore(IRepository<BackgroundJobInfo, long> backgroundJobRepository)
        {
            this.backgroundJobRepository = backgroundJobRepository;
        }

        public Task InsertAsync(BackgroundJobInfo jobInfo)
        {
            return backgroundJobRepository.InsertAsync(jobInfo);
        }

        [UnitOfWork]
        public virtual Task<List<BackgroundJobInfo>> GetWaitingJobsAsync(int maxResultCount)
        {
            var waitingJobs = backgroundJobRepository.GetAll()
                .Where(t => !t.IsAbandoned && t.NextTryTime <= Clock.Now)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.TryCount)
                .ThenBy(t => t.NextTryTime)
                .Take(maxResultCount)
                .ToList();

            return Task.FromResult(waitingJobs);
        }

        public Task DeleteAsync(BackgroundJobInfo jobInfo)
        {
            return backgroundJobRepository.DeleteAsync(jobInfo);
        }

        public Task UpdateAsync(BackgroundJobInfo jobInfo)
        {
            return backgroundJobRepository.UpdateAsync(jobInfo);
        }
    }
}
