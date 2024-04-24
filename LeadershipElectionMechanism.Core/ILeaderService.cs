
namespace LeadershipElectionMechanism.Core
{
    internal interface ILeaderService
    {
        Task TryToBecomeLeader();

        Task GiveUpLeaderShip();
    }
}
