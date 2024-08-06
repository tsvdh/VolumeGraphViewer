using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graph
{
    public enum GraphColor
    {
        YellowTransparent,
        BlueTransparent,
        Yellow,
        Red,
        Green,
        Black,
        White
    }

    public class Colorable : MonoBehaviour
    {
        private static List<Material> _materials;

        private Renderer _renderer;
        private GraphColor _color;
        private EdgeData _data;

        protected void Init()
        {
            if (ReferenceEquals(_materials, null))
            {
                _materials = new List<Material>(Enum.GetValues(typeof(GraphColor)).Length);
                foreach (GraphColor color in Enum.GetValues(typeof(GraphColor)))
                {
                    string colorName = Enum.GetName(typeof(GraphColor), color);
                    _materials.Add(Resources.Load<Material>($"Materials/{colorName}"));
                }
            }

            _renderer = GetComponentInChildren<Renderer>();
        }

        public void SetColorData(EdgeData data)
        {
            _data = data;
        }

        public void SetMaterial(GraphColor color)
        {
            _color = color;
            _renderer.material = _materials[(int)color];
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