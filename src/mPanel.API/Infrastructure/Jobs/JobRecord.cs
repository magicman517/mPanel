using FastEndpoints;

namespace mPanel.API.Infrastructure.Jobs;

public sealed class JobRecord : IJobStorageRecord
{
    public string QueueID { get; set; } = null!;
    public Guid TrackingID { get; set; }

    public object Command { get; set; } = null!;

    public bool IsComplete { get; set; }
    public bool IsCancelled { get; set; }

    public DateTime ExecuteAfter { get; set; }
    public DateTime ExpireOn { get; set; }
    public DateTime DequeueAfter { get; set; }
}