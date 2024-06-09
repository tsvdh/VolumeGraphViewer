﻿using System;
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

        public void Init(int id, Vector3 graphPos)
        {
            this.id = id;
            this.graphPos = graphPos;

            transform.position = graphPos;
        }

        public void ScaleSize(float extraScale)
        {
            Vector3 vertexScale = transform.localScale;
            vertexScale += new Vector3(extraScale, extraScale, extraScale);
            transform.localScale = vertexScale;
        }
    }
}