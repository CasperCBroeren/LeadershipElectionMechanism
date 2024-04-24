using LeadershipElectionMechanism.Core.Locks;

namespace LeadershipElectionMechanism.Core
{
    public class LeadershipOptions
    {
        public IDistributedLock DistributedLock { get; set; }
        public int LockWait { get; internal set; } = 1000;
    }
}
