using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Graph
{
    public class Graph : MonoBehaviour
    {
        public string basePath;
        public string fileName;
        public bool useSimpleMeshes;

        private GameObject _vertexPrefab; 
        private GameObject _edgePrefab;
        private Material _blueTrans;
        
        private readonly List<Vertex> _vertices = new();
        private readonly List<Edge> _edges = new();
        private readonly List<Path> _paths = new();

        private float _curScale = 1;

        private void Start()
        {
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
            _blueTrans = Resources.Load<Material>("BlueTransparent");
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
            if (newScale < 0.1 || newScale > 10)
                return;

            var newVertexScale = new Vector3(newScale, newScale, newScale);
            foreach (Vertex vertex in _vertices)
                vertex.transform.localScale = newVertexScale;

            var newEdgeScale = new Vector3(newScale, newScale, 0);
            foreach (Edge edge in _edges)
                edge.transform.localScale = newEdgeScale;
            
            _curScale = newScale;
        }

        private void ReadFromFile()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            
            var lines = new LinkedList<string>(File.ReadAllLines($"{basePath}/{fileName}.txt"));

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
                
                GameObject vertexObj = Instantiate(_vertexPrefab, transform);
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
                graphMin.x = Math.Min(graphMin.x, pos.x);
                graphMax.x = Math.Max(graphMax.x, pos.x);
                graphMin.y = Math.Min(graphMin.y, pos.y);
                graphMax.y = Math.Max(graphMax.y, pos.y);
                graphMin.z = Math.Min(graphMin.z, pos.z);
                graphMax.z = Math.Max(graphMax.z, pos.z);
            }
            Vector3 graphCenter = graphMin + (graphMax - graphMin) / 2;

            transform.position = -graphCenter;

            GameObject.Find("Camera").GetComponent<CameraControls>()
                      .Init(graphCenter, graphMax);

            for (var i = 0; i < numEdges; i++)
            {
                string[] edgeData = lines.First.Value.Split(' ');
                
                GameObject edgeObj = Instantiate(_edgePrefab, transform);
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
                
                e.Init(int.Parse(edgeData[0]), fromVertex, toVertex);
                
                lines.RemoveFirst();
            }

            for (var i = 0; i < numPaths; i++)
            {
                string[] pathData = lines.First.Value.Split(' ');
                var path = new Path { ID = int.Parse(pathData[0]) };
                _paths.Add(path);

                for (var pathEdge = 2; pathEdge < int.Parse(pathData[1]) + 2; pathEdge++)
                {
                    int pathEdgeId = int.Parse(pathData[pathEdge]);
                    foreach (Edge edge in _edges)
                    {
                        if (edge.id == pathEdgeId)
                            path.Edges.Add(edge);
                    }
                }
                
                lines.RemoveFirst();
            }

            if (description.Contains("full_paths"))
            {
                foreach (Path path in _paths)
                {
                    path.Edges[0].SetMaterial(_blueTrans);
                    path.Edges[0].from.SetMaterial(_blueTrans);
                }
            }
        }
    }
}