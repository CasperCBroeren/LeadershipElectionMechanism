using Microsoft.Extensions.Hosting;

namespace LeadershipElectionMechanism.Core
{
    internal class HostedServiceLeaderWorker<T> : IHostedService
    {
        private ILeaderWork work;
        private readonly string processName;
        private readonly LeadershipOptions options;
        private string workerId;
        private CancellationTokenSource tokenSource = new CancellationTokenSource(); 

        public HostedServiceLeaderWorker(ILeaderWork work, Action<LeadershipOptions> configureOptions, string processName)
        {
            this.work = work;
            this.processName = processName;
            this.options = new LeadershipOptions();
            configureOptions(options); 
            this.workerId = Leid.GenerateUnique(); 
        }      

        protected async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await this.options.DistributedLock.AddToLockQueue(this.processName, workerId, cancellationToken);
            await WaitToBecomeLeader(cancellationToken);
            StartKeepAlive(cancellationToken);
            await ExecuteWork(cancellationToken).ConfigureAwait(false);
        }

        private async Task ExecuteWork(CancellationToken cancellationToken)
        {
            while (await this.options.DistributedLock.IsLeader(this.processName, workerId, cancellationToken)
                             && !cancellationToken.IsCancellationRequested)
            {
                await this.work.DoLeaderWork(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task WaitToBecomeLeader(CancellationToken cancellationToken)
        {            
            while (!await this.options.DistributedLock.IsLeader(this.processName, workerId, cancellationToken)
                            && !await this.options.DistributedLock.StartConsumeLeaderAsync(this.processName, workerId, cancellationToken)
                            && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(options.LockWait);
            }
        }

        private void StartKeepAlive(CancellationToken cancellationToken)
        {
           Task.Run(async () => 
            {
                while (await this.options.DistributedLock.IsLeader(this.processName, workerId, cancellationToken)
                  && !cancellationToken.IsCancellationRequested)
                {
                    await options.DistributedLock.ConfirmLeadership(this.processName, workerId, cancellationToken);
                    await Task.Delay(options.LockWait / 2, cancellationToken);
                }
            }, cancellationToken);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task.Run(async () =>
            {
                await this.options.DistributedLock.AddToLockQueue(this.processName, workerId, cancellationToken);
                await WaitToBecomeLeader(cancellationToken);
                StartKeepAlive(cancellationToken);
                await ExecuteWork(cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.tokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
