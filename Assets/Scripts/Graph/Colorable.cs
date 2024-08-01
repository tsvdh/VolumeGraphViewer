using System;
using UnityEngine;

namespace Graph
{
    public enum GraphColor
    {
        BlueTrans,
        RedTrans,
        Yellow
    }

    public class Colorable : MonoBehaviour
    {
        private static readonly Material BlueTrans = Resources.Load<Material>("BlueTransparent");
        private static readonly Material RedTrans = Resources.Load<Material>("RedTransparent");
        private static readonly Material Yellow = Resources.Load<Material>("Yellow");

        private Renderer _renderer;
        private GraphColor _color;
        private EdgeData _data;

        public void InitColor(EdgeData data)
        {
            _renderer = GetComponentInChildren<Renderer>();
            _data = data;
        }

        public void SetMaterial(GraphColor color)
        {
            _color = color;
            _renderer.material = color switch
            {
                GraphColor.BlueTrans => BlueTrans,
                GraphColor.RedTrans => RedTrans,
                GraphColor.Yellow => Yellow,
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
            newColor.b = 255 * weight;
            _renderer.material.color = newColor;
        }
    }
}