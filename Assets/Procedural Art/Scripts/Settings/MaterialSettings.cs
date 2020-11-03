using UnityEngine;

    [CreateAssetMenu(fileName = "Material Settings", menuName = "Material Settings", order = 0)]
    public class MaterialSettings : ScriptableObject {
        public Material WallMaterial;
        public Material RoofMaterial;
        public Material WindowMaterial;
        public Material FeatureMaterial1;
        public Material FeatureMaterial2;
        public Material FeatureMaterial3;
    }