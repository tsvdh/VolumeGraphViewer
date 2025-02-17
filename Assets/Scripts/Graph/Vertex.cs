using System.Collections.Generic;
using UnityEngine;

namespace Graph
{
    public enum RayVertexType
    {
        Absorp, // black
        Scatter, // white
        Null, // white
        Entry, // green
        ScatterFinal // red
    } 
    
    public class Vertex : Colorable, IScalable
    {

        public int id;
        public Vector3 graphPos;
        public RayVertexType? Type;
        public List<Edge> inEdges = new();
        public List<Edge> outEdges = new();
        public float Lighting;

        public void Init(int id, Vector3 graphPos, int type, float? lighting)
        {
            base.Init();
            
            this.id = id;
            this.graphPos = graphPos;
            if (type != -1)
                Type = (RayVertexType)type;
            if (lighting.HasValue)
                Lighting = lighting.Value;

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