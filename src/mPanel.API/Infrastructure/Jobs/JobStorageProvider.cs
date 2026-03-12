using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Jobs;

[UsedImplicitly]
internal sealed class JobStorageProvider(
    ILogger<JobStorageProvider> logger,
    IDbContextFactory<PanelDbContext> dbContextFactory)
    : IJobStorageProvider<JobRecord>
{
    public bool DistributedJobProcessingEnabled => true;

    public async Task StoreJobAsync(JobRecord r, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        await dbContext.JobRecords.AddAsync(r, ct);
        await dbContext.SaveChangesAsync(ct);

        logger.LogDebug("Stored job {TrackingId}", r.TrackingID);
    }

    public async Task<ICollection<JobRecord>> GetNextBatchAsync(PendingJobSearchParams<JobRecord> parameters)
    {
        var ct = parameters.CancellationToken;
        var now = DateTime.UtcNow;
        var leaseTime = parameters.ExecutionTimeLimit == Timeout.InfiniteTimeSpan
            ? TimeSpan.FromMinutes(5)
            : parameters.ExecutionTimeLimit + TimeSpan.FromMinutes(1);

        logger.LogDebug(
            "Fetching next batch of jobs for queue {QueueId} with limit {Limit}. LeaseTime: {LeaseTime}",
            parameters.QueueID,
            parameters.Limit,
            leaseTime);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var jobs = await dbContext
            .JobRecords
            .FromSqlInterpolated(
                $"""
                UPDATE "JobRecords"
                SET "DequeueAfter" = {now + leaseTime}
                WHERE "TrackingID" IN (
                    SELECT "TrackingID" FROM "JobRecords"
                    WHERE "QueueID" = {parameters.QueueID}
                        AND "IsComplete" = FALSE
                        AND "IsCancelled" = FALSE
                        AND "ExecuteAfter" <= {now}
                        AND "ExpireOn" > {now}
                        AND "DequeueAfter" <= {now}
                    ORDER BY "ExecuteAfter" ASC
                    FOR UPDATE SKIP LOCKED
                    LIMIT {parameters.Limit}
                )
                RETURNING *
                """
            )
            .AsNoTracking()
            .ToListAsync(ct);

        logger.LogDebug(
            "Fetched {Count} job(s) for queue {QueueId}",
            jobs.Count,
            parameters.QueueID);

        return jobs;
    }

    public async Task MarkJobAsCompleteAsync(JobRecord r, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var updated = await dbContext
            .JobRecords
            .Where(x => x.TrackingID == r.TrackingID)
            .ExecuteUpdateAsync(s => s.SetProperty(j => j.IsComplete, true), cancellationToken: ct);

        logger.LogDebug(
            "Marked job {TrackingId} as complete. Updated rows: {UpdatedRows}",
            r.TrackingID,
            updated);
    }

    public async Task CancelJobAsync(Guid trackingId, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var updated = await dbContext
            .JobRecords
            .Where(x => x.TrackingID == trackingId)
            .ExecuteUpdateAsync(s => s.SetProperty(j => j.IsCancelled, true), cancellationToken: ct);

        logger.LogDebug(
            "Cancelled job {TrackingId}. Updated rows: {UpdatedRows}",
            trackingId,
            updated);
    }

    public async Task OnHandlerExecutionFailureAsync(JobRecord r, Exception exception, CancellationToken ct)
    {
        var retryAt = DateTime.UtcNow.AddMinutes(1);

        logger.LogWarning(
            exception,
            "Job handler failed for job {TrackingId}. Scheduling retry at {RetryAt}",
            r.TrackingID,
            retryAt);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var updated = await dbContext
            .JobRecords
            .Where(x => x.TrackingID == r.TrackingID)
            .ExecuteUpdateAsync(s => s
                .SetProperty(j => j.ExecuteAfter, retryAt)
                .SetProperty(j => j.DequeueAfter, retryAt), ct);

        logger.LogDebug(
            "Rescheduled failed job {TrackingId}. Updated rows: {UpdatedRows}",
            r.TrackingID,
            updated);
    }

    public async Task PurgeStaleJobsAsync(StaleJobSearchParams<JobRecord> parameters)
    {
        var ct = parameters.CancellationToken;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var deleted = await dbContext
            .JobRecords
            .Where(parameters.Match)
            .ExecuteDeleteAsync(ct);

        logger.LogInformation("Purged {DeletedRows} stale job(s)", deleted);
    }
}