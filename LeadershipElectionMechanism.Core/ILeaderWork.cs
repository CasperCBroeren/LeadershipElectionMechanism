namespace LeadershipElectionMechanism.Core
{
    public interface ILeaderWork
    {
        Task DoLeaderWork(CancellationToken stoppingToken);
    }
}