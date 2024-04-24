namespace LeadershipElectionMechanism.Core.Locks
{
    public class FileLock : IDistributedLock
    {
        private const string extention = ".lck";
        private string lockFolder;
        private readonly int decayMiliseconds;

        public FileLock(string lockFolder, int decayMiliseconds = 4000)
        {
            this.lockFolder = lockFolder;
            this.decayMiliseconds = decayMiliseconds;
        }

        public async Task AddToLockQueue(string processName, string id, CancellationToken cancellationToken)
        {
            try
            {
                var path = GetLockFilePath(processName);
                await File.AppendAllLinesAsync(path, new List<string>() { Register(id) }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                await AddToLockQueue(processName, id, cancellationToken);
            }
        }


        public async Task<bool> IsLeader(string processName, string workerId, CancellationToken cancellationToken)
        {
            try
            {
                var path = GetLockFilePath(processName);
                var content = await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
                return content[0].Contains(workerId);
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<bool> StartConsumeLeaderAsync(string processName, string workerId, CancellationToken cancellationToken)
        {
            try
            {
                var path = GetLockFilePath(processName);
                var content = await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
                return await ConsumeLeaderAsync(0, workerId, content, path, cancellationToken);
            }             
            catch(Exception)
            {
                return false;
            }
        }

        public async Task ConfirmLeadership(string processName, string workerId, CancellationToken cancellationToken, int retry=0)
        {
            try
            {
                var path = GetLockFilePath(processName);
                var content = await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
                var list = new List<string>() { Register(workerId) };
                list.AddRange(content[1..]); // just consume first and replace with first
                await File.WriteAllLinesAsync(path, list, cancellationToken).ConfigureAwait(false);
            }
            catch(Exception)
            {
                if (retry < 3)
                {
                    await ConfirmLeadership(processName, workerId, cancellationToken, retry + 1);
                }
            }
        }

        private async Task<bool> ConsumeLeaderAsync(int position, string workerId, string[] content, string path, CancellationToken cancellationToken)
        {
            var next = position + 1;
            if (next >= content.Length)
                return false;
            // Does the other worker still has a shot?
            var updated = new DateTime(long.Parse(content[position].Split('-')[1]));
            if (updated >= DateTime.UtcNow.AddMilliseconds(-decayMiliseconds))
            {
                return false;
            }

            if (content[next].Contains(workerId))
            {
                if (updated < DateTime.UtcNow.AddMilliseconds(-decayMiliseconds))
                {
                    // Kick leader                                      
                    var list = new List<string>() { Register(workerId) };
                    list.AddRange(content.Skip(position + 2)); // add the tail
                    await File.WriteAllLinesAsync(path, list, cancellationToken).ConfigureAwait(false);
                    return true;
                }
            }
            return await ConsumeLeaderAsync(next, workerId, content, path, cancellationToken);
        }



        private static string Register(string id)
        {
            return $"{id}-{DateTime.UtcNow.Ticks}";
        }

        private string GetLockFilePath(string processName)
        {
            return Path.Join(this.lockFolder, string.Concat(processName, extention));
        }

    }
}
