using System.Diagnostics;

namespace NetworkMetrics
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FileManager.ReadParametersFromConfigFile();         // Read parameters from Config.json file

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Network.Create();                             // Create the network based on the selected model

            Network.DegreeDistribution();           // Calculate degree distribution and
                                                    // write it in a file named "DegreeDistribution.csv"

            Network.CountNumberOfLinks();           // Count number of links in the network
            Network.CalculateAverageDegree();        // Calculate the average degree

            Network.CalculateClusteringCoefficient();                // Calculate clustering coefficient

            Network.CalculateLargestComponentAveragePathDistance();  // Calculate average path distance in
                                                                     // the largest component by breadth-first search
                                                                     // described in part 10.3 of the book
                                                                     // Network, an Introduction by Newman

            WriteParameters();                      // Output the network charactristics in
                                                    // a file named "NetworkMetrics.csv"

            stopWatch.Stop();
            var timePast = stopWatch.Elapsed;

            // Print Elapsed time run of the application
            Console.WriteLine("\n\n\nruntime of application: " + timePast.ToString("hh\\:mm\\:ss\\.ff"));

            FileManager.CloseFiles();                // Close all opened files

            // Run the python script for plotting data
            RunPythonScriptForPlottingDegreeDistribution();

            Console.ReadLine();
        }

        static void WriteParameters()
        {
            Console.Clear();

            var metrics = new List<(string Title, object Value)>
            {
                ("Number of nodes", Network.NodesCount)
            };

            if (Network.NetworkType == NetworkTypes.ErdosRenyi)
            {
                metrics.Add(("Probality of connection between each pair", Network.ErdosRenyiProbability));
                metrics.Add(("Number of nodes in the giant component", Network.LargestComponentNodeCount));
            }

            metrics.Add(("Number of links", Network.LinksCount));
            metrics.Add(("Average degree", Network.AverageDegree));
            metrics.Add(("Clustering coefficient", Network.ClusteringCoefficient));

            if (Network.NetworkType == NetworkTypes.ErdosRenyi)
                metrics.Add(("Average path distance in the giant component", Network.LargestComponentAveragePathDistance));
            else
                metrics.Add(("Average path distance", Network.LargestComponentAveragePathDistance));

            var sw = FileManager.ResultFile;
            FileManager.WriteMetrics(sw, metrics);
        }

        static void RunPythonScriptForPlottingDegreeDistribution()
        {
            // Python script file path
            string pythonFilePath = "script.py";

            // Start a new process to run the Python script
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python"; // or "python3" depending on your installation
            start.Arguments = pythonFilePath;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                using (System.IO.StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }
    }
}