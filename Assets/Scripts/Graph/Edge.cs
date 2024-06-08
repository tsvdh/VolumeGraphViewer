using System;
using UnityEngine;

namespace Graph
{
    public class Edge : MonoBehaviour
    {
        public int id;
        public Vertex from;
        public Vertex to;

        public void Init(int id, Vertex from, Vertex to)
        {
            this.id = id;
            this.from = from;
            this.to = to;

            Vector3 start = from.transform.position;
            Vector3 end = to.transform.position;

            Vector3 middle = (end + start) / 2;
            float lengthScale = (end - start).magnitude;

            transform.position = middle;
            Vector3 localScale = transform.localScale;
            localScale.z = lengthScale;
            transform.localScale = localScale;
            transform.LookAt(end);
        }
    }
}