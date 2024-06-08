using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Graph
{
    public class Vertex : MonoBehaviour
    {
        public int id;
        public Vector3 graphPos;
        public List<Edge> inEdges = new();
        public List<Edge> outEdges = new();

        public void Init(int id, Vector3 graphPos, float scale)
        {
            this.id = id;
            this.graphPos = graphPos;

            transform.position = graphPos;
            transform.position.Scale(new Vector3(scale, scale, scale));
        }
    }
}