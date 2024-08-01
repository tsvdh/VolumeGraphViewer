using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Graph
{
    public class Graph : MonoBehaviour
    {
        public bool useSimpleMeshes;
        public string id;
        public string fileName;
        public bool primaryGraph;

        private GraphHelper _helper;

        private GameObject _vertexPrefab; 
        private GameObject _edgePrefab;
        private Material _blueTrans;
        private Material _redTrans;
        
        private readonly List<Vertex> _vertices = new();
        private readonly List<Edge> _edges = new();
        private readonly List<Path> _paths = new();

        private float _curScale = 1;

        private void Start()
        {
            _helper = GetComponent<GraphHelper>();
            
            string vertexPrefabPath;
            string edgePrefabPath;
            
            if (useSimpleMeshes)
            {
                vertexPrefabPath = "Prefabs/Simple/VertexSimple";
                edgePrefabPath = "Prefabs/Simple/EdgeSimple";
            }
            else
            {
                vertexPrefabPath = "Prefabs/Detailed/Vertex";
                edgePrefabPath = "Prefabs/Detailed/Edge";
            }
            
            _vertexPrefab = Resources.Load<GameObject>(vertexPrefabPath);
            _edgePrefab = Resources.Load<GameObject>(edgePrefabPath);

            ReadFromFile();
        }

        private void Update()
        {
            // double scale in .2 seconds
            const float scaleSpeed = 0.2f;
            if (Input.GetKey(KeyCode.LeftBracket))
                ScaleElementSizes(1 - Time.deltaTime / scaleSpeed);
            if (Input.GetKey(KeyCode.RightBracket))
                ScaleElementSizes(1 + Time.deltaTime / scaleSpeed);
        }

        private void ScaleElementSizes(float scale)
        {
            float newScale = _curScale * scale;
            if (newScale < 0.01 || newScale > 100)
                return;
            
            foreach (Vertex vertex in _vertices)
                vertex.SetScale(newScale);
            
            foreach (Edge edge in _edges)
                edge.SetScale(newScale);
            
            _curScale = newScale;
        }

        private void ReadFromFile()
        {
            GameObject example = GameObject.Find("Example");
            if (primaryGraph && example)
                Destroy(example);

            var container = new GameObject(id);
            container.transform.parent = transform;

            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var lines = new LinkedList<string>(File.ReadAllLines($"{_helper.basePath}/{fileName}.txt"));
            Debug.Log($"{id} {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}");

            start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var description = new List<string>(lines.First.Value.Split(' '));
            lines.RemoveFirst();
            
            string[] extraMeta = lines.First.Value.Split(' ');
            string graphName = extraMeta[0];
            float? spacing = null;
            switch (graphName)
            {
                case "uniform":
                    spacing = float.Parse(extraMeta[1]);
                    break;
            }
            lines.RemoveFirst();

            string[] flags = lines.First.Value.Split(' ');
            bool useCoors = bool.Parse(flags[0]);
            lines.RemoveFirst();

            string[] baseMeta = lines.First.Value.Split(' ');
            int curId = int.Parse(baseMeta[0]);
            int numVertices = int.Parse(baseMeta[1]);
            int numEdges = int.Parse(baseMeta[2]);
            int numPaths = int.Parse(baseMeta[3]);
            lines.RemoveFirst();

            var worldPositions = new List<Vector3>();

            for (var i = 0; i < numVertices; i++)
            {
                string[] vertexData = lines.First.Value.Split(' ');
                var graphPos = new Vector3
                {
                    x = float.Parse(vertexData[1]),
                    y = float.Parse(vertexData[2]),
                    z = float.Parse(vertexData[3])
                };
                
                GameObject vertexObj = Instantiate(_vertexPrefab, container.transform);
                var v = vertexObj.GetComponent<Vertex>();
                _vertices.Add(v);
                
                v.Init(int.Parse(vertexData[0]), graphPos);
                worldPositions.Add(vertexObj.transform.position);
                
                lines.RemoveFirst();
            }
            
            Vector3 graphMin = Vector3.positiveInfinity;
            Vector3 graphMax = Vector3.negativeInfinity;
            foreach (Vector3 pos in worldPositions)
            {
                for (var i = 0; i < 3; i++)
                {
                    graphMin[i] = Math.Min(graphMin[i], pos[i]);
                    graphMax[i] = Math.Max(graphMax[i], pos[i]);
                }
            }
            Vector3 graphCenter = graphMin + (graphMax - graphMin) / 2;

            if (primaryGraph)
                GameObject.Find("Camera").GetComponent<CameraControls>()
                          .Init(graphCenter, graphMax);

            for (var i = 0; i < numEdges; i++)
            {
                string[] edgeData = lines.First.Value.Split(' ');
                
                GameObject edgeObj = Instantiate(_edgePrefab, container.transform);
                var e = edgeObj.GetComponent<Edge>();
                _edges.Add(e);
                
                int fromId = int.Parse(edgeData[1]);
                int toId = int.Parse(edgeData[2]);
                Vertex fromVertex = null;
                Vertex toVertex = null;
                
                foreach (Vertex v in _vertices)
                {
                    if (v.id == fromId)
                    {
                        fromVertex = v;
                        v.outEdges.Add(e);
                    }
                    if (v.id == toId)
                    {
                        toVertex = v;
                        v.inEdges.Add(e);
                    }
                }
                Assert.IsTrue(fromVertex && toVertex);

                (_, EdgeData parsedData) = ReadEdgeData(3, edgeData);

                e.Init(int.Parse(edgeData[0]), fromVertex, toVertex, parsedData);
                
                lines.RemoveFirst();
            }

            for (var i = 0; i < numPaths; i++)
            {
                string[] pathData = lines.First.Value.Split(' ');
                var path = new Path { ID = int.Parse(pathData[0]) };
                _paths.Add(path);

                var index = 2;
                while (index < int.Parse(pathData[1]))
                {
                    int pathEdgeId = int.Parse(pathData[index++]);
                    foreach (Edge edge in _edges)
                    {
                        if (edge.id == pathEdgeId)
                        {
                            path.Edges.Add(edge);
                            (int newIndex, EdgeData edgeData) = ReadEdgeData(index, pathData);
                            index = newIndex;
                            edge.InitColor(edgeData);

                            break;
                        }
                    }
                }
                
                lines.RemoveFirst();
            }

            if (description.Contains("full_paths"))
            {
                foreach (Path path in _paths)
                {
                    if (path.Edges.Count > 0)
                    {
                        path.Edges[0].SetMaterial(GraphColor.BlueTrans);
                        path.Edges[0].from.SetMaterial(GraphColor.BlueTrans);
                        path.Edges[0].ScaleChild(0.2f);
                        path.Edges[0].from.ScaleChild(0.2f);
                    }

                    for (var i = 1; i < path.Edges.Count; i++)
                    {
                        path.Edges[i].ShowThroughput();
                    }
                }
            }

            if (description.Contains("grid_queue"))
            {
                foreach (Vertex vertex in _vertices)
                {
                    vertex.SetMaterial(GraphColor.BlueTrans);
                    vertex.ScaleChild(1.2f);
                }
            }
            
            if (description.Contains("grid_surface"))
            {
                foreach (Vertex vertex in _vertices)
                {
                    vertex.SetMaterial(GraphColor.RedTrans);
                    vertex.ScaleChild(1.2f);
                }
            }

            if (description.Contains("surface"))
            {
                foreach (Vertex vertex in _vertices)
                {
                    vertex.SetMaterial(GraphColor.RedTrans);
                    vertex.ScaleChild(0.6f);
                }
            }

            Debug.Log($"{id} {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}");
        }

        private Tuple<int, EdgeData> ReadEdgeData(int start, string[] input)
        {
            var data = new EdgeData {Throughput = new List<float>(4),
                WeightedThroughput = float.Parse(input[start + 4]) };
            for (var j = 0; j < 4; j++)
                data.Throughput[j] = float.Parse(input[j]);

            return new Tuple<int, EdgeData>(start + 5, data);
        }
    }
}