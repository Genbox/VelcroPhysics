using System.IO;
using VelcroPhysics.Dynamics;

namespace VelcroPhysics.Tools.Serialization.XML
{
    /// <summary>
    /// Serialize the world into an XML file
    /// </summary>
    public static class WorldSerializer
    {
        /// <summary>
        /// Serialize the world to an XML file
        /// </summary>
        /// <param name="world"></param>
        /// <param name="filename"></param>
        public static void Serialize(World world, string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                WorldXmlSerializer.Serialize(world, fs);
            }
        }

        /// <summary>
        /// Deserialize the world from an XML file
        /// </summary>
        /// <param name="filename"></param>
        public static World Deserialize(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                return WorldXmlDeserializer.Deserialize(fs);
            }
        }
    }

    #region XMLFragment

    #endregion
}