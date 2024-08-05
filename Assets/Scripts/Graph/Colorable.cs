using System;
using UnityEngine;

namespace Graph
{
    public enum GraphColor
    {
        YellowTrans,
        RedTrans,
        Yellow
    }

    public class Colorable : MonoBehaviour
    {
        private static Material _yellowTrans;
        private static Material _redTrans;
        private static Material _yellow;

        private Renderer _renderer;
        private GraphColor _color;
        private EdgeData _data;

        protected void Init()
        {
            _yellowTrans = Resources.Load<Material>("YellowTransparent");
            _redTrans = Resources.Load<Material>("RedTransparent");
            _yellow = Resources.Load<Material>("Yellow");
            
            _renderer = GetComponentInChildren<Renderer>();
        }

        public void SetColorData(EdgeData data)
        {
            _data = data;
        }

        public void SetMaterial(GraphColor color)
        {
            _color = color;
            _renderer.material = color switch
            {
                GraphColor.YellowTrans => _yellowTrans,
                GraphColor.RedTrans => _redTrans,
                GraphColor.Yellow => _yellow,
                _ => throw new SystemException()
            };
        }

        public void ShowThroughput()
        {
            SetYellowGradient(_data.Throughput[0]);
        }

        public void ShowWeightedThroughput()
        {
            SetYellowGradient(_data.WeightedThroughput);
        }

        private void SetYellowGradient(float weight)
        {
            if (_color != GraphColor.Yellow)
                throw new SystemException();

            Color newColor = _renderer.material.color;
            newColor.b = 1 - weight;
            _renderer.material.color = newColor;
        }
    }
}