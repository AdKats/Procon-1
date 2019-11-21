namespace PRoCon.Core.Settings
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// this class serializes the general settings of a bfbc2 game server for import and export
    /// from / into procon
    /// 
    /// @copyright by macx
    /// </summary>
    public class ServerSettingsSerializer
    {
        private XmlSerializer serializer = new XmlSerializer(typeof(ServerSettings));

        public byte[] Serialize(ServerSettings settings)
        {
            var memoryStream = new MemoryStream();
            serializer.Serialize(memoryStream, settings);
            return memoryStream.GetBuffer();
        }

        public ServerSettings Deserialize(byte[] content)
        {
            var memoryStream = new MemoryStream(content);
            return serializer.Deserialize(memoryStream) as ServerSettings;
        }
    }
}
