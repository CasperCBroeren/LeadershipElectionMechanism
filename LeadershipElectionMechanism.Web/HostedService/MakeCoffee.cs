using LeadershipElectionMechanism.Core;

namespace LeadershipElectionMechanism.Web.HostedService
{
    public class MakeCoffee : ILeaderWork
    {
        public async Task DoLeaderWork(CancellationToken stoppingToken)
        {
            Console.WriteLine("Making cofee");
            await Task.Delay(500);
        }
    }
}
