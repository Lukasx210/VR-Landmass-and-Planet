﻿
using UnityEngine;

[CreateAssetMenu()]
public class ColourSettings : ScriptableObject
{

    public Gradient oceanColour;

    public Material planetMaterial;

    public BiomeColourSettings biomeColourSettings;

    [System.Serializable]
    public class BiomeColourSettings
    {
        public Biome[] biome;
        public NoiseSettings noise;
        public float noiseOffset;
        public float noiseStrength;
        [Range(0,1)]
        public float blendAmount;

        [System.Serializable]
        public class Biome
        {
            public Gradient gradient;
            public Color tint;
            [Range(0, 1)]
            public float startHeight;
            [Range(0, 1)]
            public float tintPercent;
        }
    }

}