using LeadershipElectionMechanism.Core;

namespace LeadershipElectionMechanism.Web.HostedService
{
    public class DoSomeHttpRequest : ILeaderWork
    {
        public async Task DoLeaderWork(CancellationToken stoppingToken)
        {
            Console.WriteLine("Doing fake http call");
            await Task.Delay(10000);
        }
    }
}
