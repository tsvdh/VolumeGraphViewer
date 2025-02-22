﻿using System;
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
            Debug.Log($"Reading {id} {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms");

            start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string description = lines.First.Value;
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
            bool useThroughput = bool.Parse(flags[1]);
            bool useRayVertexType = bool.Parse(flags[2]);
            bool useLighting = bool.Parse(flags[3]);
            lines.RemoveFirst();

            string[] baseMeta = lines.First.Value.Split(' ');
            int curVertexId = int.Parse(baseMeta[0]);
            int curEdgeId = int.Parse(baseMeta[1]);
            int curPathId = int.Parse(baseMeta[2]);
            int numVertices = int.Parse(baseMeta[3]);
            int numEdges = int.Parse(baseMeta[4]);
            int numPaths = int.Parse(baseMeta[5]);
            lines.RemoveFirst();

            var vertexWorldPositions = new List<Vector3>();

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

                int vertexType = useRayVertexType ? int.Parse(vertexData[4]) : -1;
                int lightingIndex = useRayVertexType ? 5 : 4;
                float? lighting = useLighting ? float.Parse(vertexData[lightingIndex]) : null;
                
                v.Init(int.Parse(vertexData[0]), graphPos, vertexType, lighting);
                vertexWorldPositions.Add(vertexObj.transform.position);
                
                lines.RemoveFirst();
            }
            
            Vector3 graphMin = Vector3.positiveInfinity;
            Vector3 graphMax = Vector3.negativeInfinity;
            foreach (Vector3 pos in vertexWorldPositions)
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

                EdgeData parsedData = null;
                if (useThroughput)
                    (_, parsedData) = ReadEdgeData(3, edgeData);

                e.Init(int.Parse(edgeData[0]), fromVertex, toVertex, parsedData);
                
                lines.RemoveFirst();
            }

            for (var i = 0; i < numPaths; i++)
            {
                string[] pathData = lines.First.Value.Split(' ');
                var path = new Path { ID = int.Parse(pathData[0]) };
                _paths.Add(path);

                var index = 2;
                int edgesInPath = int.Parse(pathData[1]);
                // int numsToParse = edgesInPath * (useThroughput ? 6 : 1);
                while (index <  2 + edgesInPath)
                {
                    int pathEdgeId = int.Parse(pathData[index++]);
                    foreach (Edge edge in _edges)
                    {
                        if (edge.id == pathEdgeId)
                        {
                            path.Edges.Add(edge);

                            // if (useThroughput)
                            // {
                            //     (int newIndex, EdgeData edgeData) = ReadEdgeData(index, pathData);
                            //     index = newIndex;
                            //     edge.SetColorData(edgeData);
                            // }
                            break;
                        }
                    }
                }
                lines.RemoveFirst();
            }

            if (description.Contains("paths"))
            {
                foreach (Path path in _paths)
                {
                    if (path.Edges.Count > 0)
                    {
                        path.Edges[0].SetMaterial(GraphColor.YellowTransparent);
                        path.Edges[0].from.SetMaterial(GraphColor.YellowTransparent);
                        path.Edges[0].ScaleChild(0.2f);
                        path.Edges[0].from.ScaleChild(0.2f);
                    }

                    if (useThroughput)
                    {
                        for (var i = 1; i < path.Edges.Count; i++)
                        {
                            path.Edges[i].SetMaterial(GraphColor.Yellow);
                            path.Edges[i].ShowThroughput();
                        }
                    }

                    if (useRayVertexType)
                    {
                        for (var i = 0; i < path.Edges.Count; i++)
                        {
                            Vertex curVertex = path.Edges[i].to;
                            curVertex.SetMaterial(curVertex.Type.Value switch
                            {
                                RayVertexType.Absorp => GraphColor.Black,
                                RayVertexType.Scatter => GraphColor.White,
                                RayVertexType.Null => GraphColor.White,
                                RayVertexType.Entry => GraphColor.Green,
                                RayVertexType.ScatterFinal => GraphColor.Red,
                                _ => throw new SystemException()
                            });
                        }
                    }
                }
            }

            if (description.Contains("search_queue"))
            {
                foreach (Vertex vertex in _vertices)
                {
                    vertex.SetMaterial(GraphColor.YellowTransparent);
                    vertex.ScaleChild(1.2f);
                }
            }
            
            if (description.Contains("search_surface"))
            {
                foreach (Vertex vertex in _vertices)
                {
                    vertex.SetMaterial(GraphColor.BlueTransparent);
                    vertex.ScaleChild(1.2f);
                }
            }

            if (description.Contains("surface"))
            {
                foreach (Vertex vertex in _vertices)
                {
                    vertex.SetMaterial(GraphColor.BlueTransparent);
                    vertex.ScaleChild(0.6f);
                }
            }
            
            if (description.Contains("grid"))
            {
                foreach (Vertex vertex in _vertices)
                {
                    vertex.SetMaterial(GraphColor.BlueTransparent);
                    vertex.ScaleChild(0.6f);
                }
            }

            if (description.Contains("transmittance"))
            {
                foreach (Edge edge in _edges)
                {
                    edge.SetMaterial(GraphColor.Yellow);
                    edge.ShowThroughput();
                }
            }

            if (description.Contains("outline"))
            {
                foreach (Vertex vertex in _vertices)
                {
                    vertex.SetMaterial(GraphColor.BlueTransparent);
                }

                foreach (Edge edge in _edges)
                {
                    edge.SetMaterial(GraphColor.BlueTransparent);
                }
            }

            Debug.Log($"Parsing {id} {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms");
        }

        private static Tuple<int, EdgeData> ReadEdgeData(int start, string[] input)
        {
            var data = new EdgeData {
                Throughput = new List<float>(4),
                WeightedThroughput = float.Parse(input[start + 4]),
                NumSamples = int.Parse(input[start + 5])
            };
            for (var j = 0; j < 4; j++)
                data.Throughput.Add(float.Parse(input[start + j]));

            return new Tuple<int, EdgeData>(start + 6, data);
        }
    }
}