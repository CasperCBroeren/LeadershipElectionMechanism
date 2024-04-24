namespace LeadershipElectionMechanism.Core.Locks
{
    public interface IDistributedLock
    {
        Task AddToLockQueue(string processname, string id, CancellationToken cancelToken);
        Task ConfirmLeadership(string processName, string workerId, CancellationToken cancellationToken, int retry = 0);
        Task<bool> IsLeader(string processName, string id, CancellationToken cancellationToken);
        Task<bool> StartConsumeLeaderAsync(string processName, string workerId, CancellationToken cancellationToken);
    }
}
