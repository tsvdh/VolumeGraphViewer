using System;
using UnityEngine;

namespace Graph
{
    public class Edge : MonoBehaviour, IScalable
    {
        public int id;
        public Vertex from;
        public Vertex to;
        
        private Renderer _renderer;

        public void Init(int id, Vertex from, Vertex to)
        {
            this.id = id;
            this.from = from;
            this.to = to;
            
            _renderer = GetComponentInChildren<Renderer>();

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
        
        public void SetMaterial(Material material)
        {
            _renderer.material = material;
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
}