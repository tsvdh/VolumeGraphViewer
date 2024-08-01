using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph
{
    public class Edge : Colorable, IScalable
    {
        public int id;
        public Vertex from;
        public Vertex to;
        public EdgeData data;

        public void Init(int id, Vertex from, Vertex to, EdgeData data)
        {
            this.id = id;
            this.from = from;
            this.to = to;
            this.data = data;

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

        public void SetScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
        }

        public void ScaleChild(float scale)
        {
            Transform child = transform.GetChild(0);
            Vector3 newScale = child.localScale;
            newScale.x *= scale;
            newScale.z *= scale;
            child.localScale = newScale;
        }
    }

    public class EdgeData
    {
        public List<float> Throughput;
        public float WeightedThroughput;
    }
}