namespace LeaderEngine
{
    public struct MaterialProperty
    {
        public string Name;
        public MaterialPropertyType PropertyType;

        public MaterialProperty(string name, MaterialPropertyType propertyType)
        {
            Name = name;
            PropertyType = propertyType;
        }
    }
}
