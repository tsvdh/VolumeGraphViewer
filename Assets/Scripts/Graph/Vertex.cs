using System.Collections.Generic;
using UnityEngine;

namespace Graph
{
    public class Vertex : Colorable, IScalable
    {

        public int id;
        public Vector3 graphPos;
        public List<Edge> inEdges = new();
        public List<Edge> outEdges = new();

        public void Init(int id, Vector3 graphPos)
        {
            base.Init();
            
            this.id = id;
            this.graphPos = graphPos;

            transform.position = graphPos;
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