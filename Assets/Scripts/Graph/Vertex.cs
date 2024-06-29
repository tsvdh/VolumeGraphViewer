using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Graph
{
    public class Vertex : MonoBehaviour, IScalable
    {
        public int id;
        public Vector3 graphPos;
        public List<Edge> inEdges = new();
        public List<Edge> outEdges = new();

        private Renderer _renderer;

        public void Init(int id, Vector3 graphPos)
        {
            this.id = id;
            this.graphPos = graphPos;
            
            _renderer = GetComponentInChildren<Renderer>();

            transform.position = graphPos;
        }

        public void SetMaterial(Material material)
        {
            _renderer.material = material;
        }
        
        public void SetScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public void ScaleChild(float scale)
        {
            transform.GetChild(0).localScale *= scale;
        }
    }
}