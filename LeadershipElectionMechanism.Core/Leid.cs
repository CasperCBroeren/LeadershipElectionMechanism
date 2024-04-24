using System.Text;

namespace LeadershipElectionMechanism.Core
{
    public static class Leid
    {
        public static string GenerateUnique()
        {
            string processorId = Environment.ProcessorCount.ToString();
            string machineName = Environment.MachineName;
            string machineId = $"{processorId}_{machineName}";

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string uniqueId = $"{machineId}_{timestamp}";
            var bytes = Encoding.UTF8.GetBytes(uniqueId);
            return Convert.ToBase64String(bytes);

        }
    }
}
