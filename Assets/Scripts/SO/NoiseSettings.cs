using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum FilterType
    {
        Simple,
        Ridgid
    };
    public FilterType filterType;

    public SimpleNoiseSettings simpleNoiseSettings;
    public RidgidNoiseSettings ridgidNoiseSettings;



    [System.Serializable]
    public class SimpleNoiseSettings
    {
        public float strengh = 1;
        [Range(1, 8)]
        public int numLayers = 1;
        public float baseRoughness = 1;
        public float roughness = 2;
        public float persistence = 0.5f;
        public Vector3 center;

        public float minValue;
    }

    [System.Serializable]
    public class RidgidNoiseSettings : SimpleNoiseSettings
    {

        public float weightMult = .8f;
    }


}
