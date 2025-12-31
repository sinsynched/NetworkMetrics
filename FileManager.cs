using System.Text.Json;

namespace NetworkMetrics
{
    internal static class FileManager
    {
        public static StreamWriter ResultFile = new("NetworkMetrics.csv");

        public static StreamWriter DegreeDistributionFile = new("DegreeDistribution.csv");

        public static StreamWriter NetworkFile;

        public static void ReadParametersFromConfigFile()
        {
            var jsonFilePath = "Config.json";
            var jsonContent = File.ReadAllText(jsonFilePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;

            var networkParameters = root.GetProperty(nameof(Network));
            Network.Initialize(
                networkParameters.GetProperty(nameof(Network.LoadNetwork)).GetBoolean(),
                (NetworkTypes)networkParameters.GetProperty(nameof(Network.NetworkType)).GetInt32(),
                networkParameters.GetProperty(nameof(Network.ErdosRenyiProbability)).GetDouble(),
                networkParameters.GetProperty(nameof(Network.NodesCount)).GetInt32(),
                networkParameters.GetProperty(nameof(Network.RingGraphDegree)).GetInt32(),
                networkParameters.GetProperty(nameof(Network.RewiringProbability)).GetDouble()
                );
        }

        public static void WriteMetrics(StreamWriter sw, List<(string title, object value)> metrics)
        {
            const int titleWidth = 45;
            const int valueWidth = 30;

            foreach (var (title, value) in metrics)
            {
                sw.WriteLine(title + "," + value);
                Console.WriteLine($"{title,-titleWidth}{value,valueWidth}");
                //Console.WriteLine(title + "\t" + value);
            }
        }

        public static void CloseFiles()
        {
            ResultFile.Close();
            DegreeDistributionFile.Close();
            NetworkFile?.Close();
        }
    }
}
