namespace NetworkMetrics
{
    enum NetworkTypes
    {
        SimpleSquareLattice,
        ErdosRenyi,
        RegularRingGraph,
        SmallWorld,
        BarabasiAlbert,
        TriangularLattice,
        Honeycomb
    }

    static class Network
    {
        public static bool LoadNetwork { get; set; }
        public static int RingGraphDegree { get; set; }
        public static NetworkTypes NetworkType { get; set; }
        public static double RewiringProbability { get; set; }
        public static double ErdosRenyiProbability { get; set; }
        public static int NodesCount { get; set; }
        public static int LatticeLength { get; set; }
        public static int LinksCount { get; set; }
        public static double AverageDegree { get; set; }
        public static double ClusteringCoefficient { get; set; }


        private static int[][] LinksOfNodes { get; set; }
        private static int[] DegreeOfNodes { get; set; }
        private static int[] ComponentOfEachNode { get; set; }
        private static int[] ShortestPaths { get; set; }

        private static int CurrentComponentIndex { get; set; }
        private static int NextComponentIndex { get; set; }
        private static Dictionary<int, int> ComponentsSize = [];

        private static int LargestComponentIndex = 0;

        public static int LargestComponentNodeCount = 0;

        public static double LargestComponentAveragePathDistance;


        private static readonly Random random = new();

        public static void Initialize(bool loadNetwork,
                                      NetworkTypes networkToCreate,
                                      double erdosRenyiProbability,
                                      int nodesCount,
                                      int ringGraphDegree,
                                      double rewiringProbability)
        {
            LoadNetwork = loadNetwork;
            NetworkType = networkToCreate;
            ErdosRenyiProbability = erdosRenyiProbability;
            NodesCount = nodesCount;
            RingGraphDegree = ringGraphDegree;
            RewiringProbability = rewiringProbability;
        }

        public static void Create()
        {
            if (LoadNetwork)
                LoadNetworkProcess();

            if (NetworkType == NetworkTypes.SimpleSquareLattice ||
                NetworkType == NetworkTypes.TriangularLattice ||
                NetworkType == NetworkTypes.Honeycomb)
            {
                LatticeLength = (int)Math.Sqrt(NodesCount);
                NodesCount = LatticeLength * LatticeLength;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nNumber of nodes adjusted to {NodesCount} " +
                                  $"to form a perfect lattice.");
                Console.ResetColor();
            }

            DegreeOfNodes = new int[NodesCount];

            if (!LoadNetwork)
            {
                LinksOfNodes = new int[NodesCount][];

                switch (NetworkType)
                {
                    case NetworkTypes.SimpleSquareLattice:
                        SimpleSquareLattice();
                        break;
                    case NetworkTypes.ErdosRenyi:
                        ErdosRenyiNetwork(ErdosRenyiProbability);
                        break;
                    case NetworkTypes.RegularRingGraph:
                        RegularRingGraph(RingGraphDegree);
                        break;
                    case NetworkTypes.SmallWorld:
                        RegularRingGraph(RingGraphDegree, RewiringProbability);
                        break;
                    case NetworkTypes.BarabasiAlbert:
                        BarabasiAlbertNetwork();
                        break;
                    case NetworkTypes.TriangularLattice:
                        TriangularLattice();
                        break;
                    case NetworkTypes.Honeycomb:
                        HoneycombNetwork();
                        break;
                }

                SaveNetwork();
            }
        }

        private static void AssignLinksToNodes(List<int>[] tempLinks)
        {
            for (int node = 0; node < NodesCount; node++)
            {
                LinksOfNodes[node] = tempLinks[node].ToArray();
            }
        }

        private static void SimpleSquareLattice()
        {
            var tempLinks = new List<int>[NodesCount];

            for (int node = 0; node < NodesCount; node++)
            {
                var linksOfEachNode = new List<int>();

                //left link
                if (node % LatticeLength == 0)
                    linksOfEachNode.Add(node + LatticeLength - 1);
                else
                    linksOfEachNode.Add(node - 1);

                //top link
                if (node - LatticeLength < 0)
                    linksOfEachNode.Add(node + LatticeLength * (LatticeLength - 1));
                else
                    linksOfEachNode.Add(node - LatticeLength);

                //right link
                if ((node + 1) % LatticeLength == 0)
                    linksOfEachNode.Add(node - LatticeLength + 1);
                else
                    linksOfEachNode.Add(node + 1);

                //bottom link
                if (node + LatticeLength >= LatticeLength * LatticeLength)
                    linksOfEachNode.Add(node + LatticeLength * (1 - LatticeLength));
                else
                    linksOfEachNode.Add(node + LatticeLength);

                tempLinks[node] = linksOfEachNode;
            }

            AssignLinksToNodes(tempLinks);
        }

        private static void ErdosRenyiNetwork(double p)
        {
            var tempLinks = new List<int>[NodesCount];
            for (int i = 0; i < NodesCount; i++)
            {
                tempLinks[i] = new List<int>();
            }

            float rndNum;
            for (int i = 0; i < NodesCount; i++)
            {
                for (int j = i + 1; j < NodesCount; j++)
                {
                    rndNum = random.NextSingle();
                    if (rndNum < p)
                    {
                        tempLinks[i].Add(j);
                        tempLinks[j].Add(i);
                    }
                }
            }

            AssignLinksToNodes(tempLinks);
        }

        static void RegularRingGraph(int degree, double? rewiringProbability = null)
        {
            var tempLinks = new List<int>[NodesCount];

            for (int node = 0; node < NodesCount; node++)
            {
                var linksOfEachNode = new List<int>();

                var nodeIncreament = 1;
                while (nodeIncreament <= degree / 2)
                {

                    linksOfEachNode.Add(PeriodicCondition(node + nodeIncreament));
                    linksOfEachNode.Add(PeriodicCondition(node - nodeIncreament));

                    nodeIncreament++;
                }

                tempLinks[node] = linksOfEachNode;
            }

            if (rewiringProbability != null)
            {
                var LinksOfNodesBeforeRewiring = new List<int>[NodesCount];
                for (int node = 0; node < NodesCount; node++)
                {
                    LinksOfNodesBeforeRewiring[node] = tempLinks[node];
                }
                RingGraphRewiring(rewiringProbability!.Value, LinksOfNodesBeforeRewiring);
            }
            else
            {
                AssignLinksToNodes(tempLinks);
            }

            int PeriodicCondition(int value)
            {
                if (value < 0)
                    return NodesCount + value;
                else if (value >= NodesCount)
                    return value - NodesCount;
                else
                    return value;
            }
        }

        private static void TriangularLattice()
        {
            var tempLinks = new List<int>[NodesCount];

            var condition = 1;
            for (int j = 0; j < LatticeLength; j++)
            {
                for (int i = 0; i < LatticeLength; i++)
                {
                    var linksOfEachNode = new List<int>();
                    int nodeToLink;

                    //left link
                    nodeToLink = FindTheTargetNode(PeriodicCondition(i - 1), j);
                    linksOfEachNode.Add(nodeToLink);

                    //right link
                    nodeToLink = FindTheTargetNode(PeriodicCondition(i + 1), j);
                    linksOfEachNode.Add(nodeToLink);

                    //top link
                    nodeToLink = FindTheTargetNode(i, PeriodicCondition(j - 1));
                    linksOfEachNode.Add(nodeToLink);

                    //bottom link
                    nodeToLink = FindTheTargetNode(i, PeriodicCondition(j + 1));
                    linksOfEachNode.Add(nodeToLink);

                    nodeToLink = FindTheTargetNode
                        (PeriodicCondition(i - condition), PeriodicCondition(j - 1));
                    linksOfEachNode.Add(nodeToLink);

                    nodeToLink = FindTheTargetNode
                        (PeriodicCondition(i - condition), PeriodicCondition(j + 1));
                    linksOfEachNode.Add(nodeToLink);

                    tempLinks[FindTheTargetNode(i, j)] =
                        linksOfEachNode;
                }
                condition = -condition;
            }

            AssignLinksToNodes(tempLinks);

            int FindTheTargetNode(int i, int j)
            {
                var targetNode = j * LatticeLength + i;
                return targetNode;
            }

            int PeriodicCondition(int value)
            {
                if (value < 0)
                    return LatticeLength + value;
                else if (value >= LatticeLength)
                    return value - LatticeLength;
                else
                    return value;
            }
        }

        private static void HoneycombNetwork()
        {
            var tempLinks = new List<int>[NodesCount];

            var bottomLink = true;
            for (int j = 0; j < LatticeLength; j++)
            {
                for (int i = 0; i < LatticeLength; i++)
                {
                    var linksOfEachNode = new List<int>();
                    int nodeToLink;

                    //left link
                    nodeToLink = FindTheTargetNode(PeriodicCondition(i - 1), j);
                    linksOfEachNode.Add(nodeToLink);

                    //right link
                    nodeToLink = FindTheTargetNode(PeriodicCondition(i + 1), j);
                    linksOfEachNode.Add(nodeToLink);

                    if (!bottomLink)
                    {
                        //top link
                        nodeToLink = FindTheTargetNode(i, PeriodicCondition(j - 1));
                        linksOfEachNode.Add(nodeToLink);
                    }
                    else
                    {
                        //bottom link
                        nodeToLink = FindTheTargetNode(i, PeriodicCondition(j + 1));
                        linksOfEachNode.Add(nodeToLink);
                    }

                    bottomLink = !bottomLink;

                    tempLinks[FindTheTargetNode(i, j)] = linksOfEachNode;
                }

                bottomLink = !bottomLink;
            }

            AssignLinksToNodes(tempLinks);

            int FindTheTargetNode(int i, int j)
            {
                var targetNode = j * LatticeLength + i;
                return targetNode;
            }

            int PeriodicCondition(int value)
            {
                if (value < 0)
                    return LatticeLength + value;
                else if (value >= LatticeLength)
                    return value - LatticeLength;
                else
                    return value;
            }
        }

        private static void BarabasiAlbertNetwork()
        {
            var tmpLinks = new List<int>[NodesCount];

            tmpLinks[0] = new List<int> { 1 };
            tmpLinks[1] = new List<int> { 0 };

            var tickets = 2;

            for (int node = 2; node < NodesCount; node++)
            {
                var rndNumber = random.Next(tickets);
                int winnerNode = 0;

                for (int i = 0; i < tmpLinks.Length; i++)
                {
                    rndNumber -= tmpLinks[i].Count;
                    if (rndNumber <= 0)
                    {
                        winnerNode = i;
                        tickets += 2;
                        break;
                    }
                }

                tmpLinks[node] = new List<int> { winnerNode };
                tmpLinks[winnerNode].Add(node);
            }

            AssignLinksToNodes(tmpLinks);
        }

        private static void RingGraphRewiring(double rewiringProbability, List<int>[] LinksOfNodesBeforeRewiring)
        {
            for (int node = 0; node < NodesCount; node++)
            {
                var nthRightNeighbor = 1;
                while (nthRightNeighbor <= RingGraphDegree / 2)
                {
                    var linkedNodeToRewire = PeriodicCondition(node + nthRightNeighbor);

                    var modify = random.NextSingle();
                    if (modify < rewiringProbability)
                    {
                        while (true)
                        {
                            var rndNode = random.Next(NodesCount);
                            if (rndNode == node)
                                continue;

                            var duplicateLink = false;
                            foreach (var n in LinksOfNodesBeforeRewiring[node])
                            {
                                if (n == linkedNodeToRewire)
                                    continue;

                                if (n == rndNode)
                                {
                                    duplicateLink = true;
                                    break;
                                }
                            }

                            if (!duplicateLink)
                            {
                                LinksOfNodesBeforeRewiring[node].Remove(linkedNodeToRewire);
                                LinksOfNodesBeforeRewiring[linkedNodeToRewire].Remove(node);

                                LinksOfNodesBeforeRewiring[node].Add(rndNode);
                                LinksOfNodesBeforeRewiring[rndNode].Add(node);

                                break;
                            }
                        }
                    }

                    nthRightNeighbor++;
                }
            }

            AssignLinksToNodes(LinksOfNodesBeforeRewiring);

            int PeriodicCondition(int value)
            {
                if (value < 0)
                    return NodesCount + value;
                else if (value >= NodesCount)
                    return value - NodesCount;
                else
                    return value;
            }
        }

        public static void CountNumberOfLinks()
        {
            var numberOfLinks = 0;

            for (int node = 0; node < NodesCount; node++)
            {
                numberOfLinks += DegreeOfNodes[node];
            }
            numberOfLinks /= 2;

            LinksCount = numberOfLinks;
        }

        public static void DegreeDistribution()
        {
            var degreeFrequency = new Dictionary<int, int>();

            for (int node = 0; node < NodesCount; node++)
            {
                var nodeDegree = LinksOfNodes[node].Length;

                DegreeOfNodes[node] = nodeDegree;

                if (!degreeFrequency.TryAdd(nodeDegree, 1))
                    degreeFrequency[nodeDegree] += 1;
            }

            var orderedDegreeFrequency = degreeFrequency.OrderBy(x => x.Key);

            var sw = FileManager.DegreeDistributionFile;
            foreach (var node in orderedDegreeFrequency)
            {
                sw.WriteLine(node.Key + "," +
                             node.Value * 1.0 / NodesCount);
            }

            sw.Flush();
        }

        public static void CalculateAverageDegree()
        {
            // <K> = 2m/n, where m is number of the links and n is number of nodes
            AverageDegree = LinksCount * 2.0 / NodesCount;


            /* 
               This is the alternative way:
               Calculate average degree by summing over all node degrees
               and dividing by the number of nodes
            */

            //var sumOfNodesDegree = 0;
            //for (int node = 0; node < NodesCount; node++)
            //{
            //    sumOfNodesDegree += LinksOfNodes[node].Length;
            //}
            //averageDegree = sumOfNodesDegree * 1.0 / NodesCount;
        }

        public static void CalculateClusteringCoefficient()
        {
            /* Calculatle clustering coefficient by counting connected neighbors and 
             * dividing by the number of possible triangles
             */
            var triangleCount = 0;
            for (int node = 0; node < NodesCount; node++)
            {
                var neighbors = LinksOfNodes[node];

                for (int i = 0; i < neighbors.Length; i++)
                {
                    var neighborsOfNeighbor = LinksOfNodes[neighbors[i]];
                    for (int j = i + 1; j < neighbors.Length; j++)
                    {
                        if (neighborsOfNeighbor.Contains(neighbors[j]))
                            triangleCount++;
                    }
                }
            }

            var connectedTriplesCount = 0;
            for (int node = 0; node < NodesCount; node++)
            {
                var nodeDegree = LinksOfNodes[node].Length;
                connectedTriplesCount += nodeDegree * (nodeDegree - 1) / 2;
            }

            ClusteringCoefficient = triangleCount * 1.0 / connectedTriplesCount;
        }

        public static void CalculateLargestComponentAveragePathDistance()
        {
            var averagePathDistanceOfNodes = AveragePathDistanceOfAllNodes();

            FindTheLargestComponent();

            var sumOfPathDistances = .0;
            for (int node = 0; node < NodesCount; node++)
            {
                if (ComponentOfEachNode[node] == LargestComponentIndex)
                {
                    sumOfPathDistances += averagePathDistanceOfNodes[node];
                }
            }

            LargestComponentAveragePathDistance = sumOfPathDistances * 1.0 / LargestComponentNodeCount;

        }

        static List<double> AveragePathDistanceOfAllNodes()
        {
            var averagePathDistanceOfNodes = new List<double>();

            ComponentOfEachNode = new int[NodesCount];

            for (int node = 0; node < NodesCount; node++)
            {
                ComponentOfEachNode[node] = -1;
            }


            for (int node = 0; node < NodesCount; node++)
            {
                FindShortestPaths(node);

                ComponentAssigningProcess();

                var averagePathDistance = AveragePathDistanceForOneNode();

                averagePathDistanceOfNodes.Add(averagePathDistance);
            }

            return averagePathDistanceOfNodes;
        }

        static void FindTheLargestComponent()
        {
            var largestComponent = ComponentsSize.MaxBy(x => x.Value);
            LargestComponentIndex = largestComponent.Key;
            LargestComponentNodeCount = largestComponent.Value;
        }

        static void FindShortestPaths(int sourceNode)
        {
            ShortestPaths = new int[NodesCount];

            for (int node = 0; node < NodesCount; node++)
            {
                if (node == sourceNode)
                    ShortestPaths[node] = 0;
                else
                    ShortestPaths[node] = -1;
            }

            var queue = new Queue<int>();
            queue.Enqueue(sourceNode);

            while (true)
            {
                if (queue.Count == 0)
                    break;

                var nextNode = queue.Dequeue();
                var neighbors = LinksOfNodes[nextNode];
                var distance = ShortestPaths[nextNode];
                foreach (var neighbor in neighbors)
                {
                    if (ShortestPaths[neighbor] == -1)
                    {
                        ShortestPaths[neighbor] = distance + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        static double AveragePathDistanceForOneNode()
        {
            var sumOfPathDistances = 0;
            foreach (var pathDistance in ShortestPaths)
            {
                if (pathDistance != -1)
                {
                    sumOfPathDistances += pathDistance;
                }
            }

            // Todo: Check why -1 is used here
            //var pathCountForTheNode = componentsSize.Where(x => x.Key == CurrentComponentIndex).Single().Value - 1;
            var pathCountForTheNode = ComponentsSize.Where(x => x.Key == CurrentComponentIndex).Single().Value;
            var averagePathDistance = sumOfPathDistances * 1.0 / pathCountForTheNode;
            return averagePathDistance;
        }

        static void ComponentAssigningProcess()
        {
            var memberCount = 0;
            var knownComponent = true;
            for (int node = 0; node < NodesCount; node++)
            {
                if (ShortestPaths[node] != -1)
                {
                    if (knownComponent)
                    {
                        if (ComponentOfEachNode[node] != -1)
                        {
                            CurrentComponentIndex = ComponentOfEachNode[node];
                            break;
                        }
                        else
                            knownComponent = false;

                    }
                    ComponentOfEachNode[node] = NextComponentIndex;
                    memberCount++;
                }
            }
            if (!knownComponent)
            {
                ComponentsSize.Add(NextComponentIndex, memberCount);
                NextComponentIndex++;
            }
        }

        private static void SaveNetwork()
        {
            var sw = FileManager.NetworkFile;
            sw = new StreamWriter("Network.csv");
            sw.WriteLine("#Load Network Info" + "\n");

            var metrics = new List<(string Title, object Value)>
            {
                ("NetworkType", NetworkType),
                ("NodesCount" , NodesCount)
            };

            switch (NetworkType)
            {
                case NetworkTypes.ErdosRenyi:
                    metrics.Add(("ErdosRenyiProbability", ErdosRenyiProbability));
                    break;
                case NetworkTypes.RegularRingGraph:
                    metrics.Add(("RingGraphDegree", RingGraphDegree));
                    break;
                case NetworkTypes.SmallWorld:
                    metrics.Add(("RewiringProbability", RewiringProbability));
                    break;
            }

            FileManager.WriteMetrics(sw, metrics);

            sw.WriteLine("\n#Links of Each Node");

            for (int i = 0; i < NodesCount; i++)
            {
                if (LinksOfNodes[i].Length == 0)
                    sw.Write("-1"); // to indicate that the node has no links
                else
                {
                    foreach (var item in LinksOfNodes[i])
                    {
                        if (item != LinksOfNodes[i].Last())
                            sw.Write(item + ",");
                        else
                            sw.Write(item);
                    }
                }

                if (i != NodesCount - 1)
                    sw.WriteLine();
            }
            sw.Flush();
            sw.Close();
        }

        private static void LoadNetworkProcess()
        {
            var loadedLinksOfNodes = new List<int[]>();

            var readingNetworkParameters = false;
            var skipLines = true;
            foreach (var line in File.ReadLines("Network.csv"))
            {
                if (line.StartsWith("#Load Network Info"))
                {
                    skipLines = false;
                    continue;
                }
                else if (line.StartsWith("#Links of Each Node"))
                {
                    readingNetworkParameters = true;
                    continue;
                }
                else if (skipLines)
                    continue;

                var parts = line.Split(",");

                if (readingNetworkParameters)
                {
                    var linksOfEachnode = Array.ConvertAll(parts, int.Parse);

                    if (linksOfEachnode[0] == -1)
                        loadedLinksOfNodes.Add(Array.Empty<int>());
                    else
                        loadedLinksOfNodes.Add(linksOfEachnode);
                }
                else
                {
                    if (parts.Length == 2 && parts[0].StartsWith("NodesCount"))
                        NodesCount = int.Parse(parts[1]);
                    else if (parts.Length == 2 && parts[0].StartsWith("NetworkType"))
                        NetworkType = (NetworkTypes)Enum.Parse(typeof(NetworkTypes), parts[1]);
                    else if (parts.Length == 2 && parts[0].StartsWith("RingGraphDegree"))
                        RingGraphDegree = int.Parse(parts[1]);
                    else if (parts.Length == 2 && parts[0].StartsWith("RewiringProbability"))
                        RewiringProbability = double.Parse(parts[1]);
                }
            }

            LinksOfNodes = new int[NodesCount][];
            for (int n = 0; n < NodesCount; n++)
            {
                LinksOfNodes[n] = loadedLinksOfNodes[n];
            }

        }
    }
}
