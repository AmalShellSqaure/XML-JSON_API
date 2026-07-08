using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using XML_JSON_API.Repositories;

namespace XML_JSON_API.Services
{
    public class FileProcessingService
    {
        private readonly MemoryRepository _repository;

        public FileProcessingService(MemoryRepository repository)
        {
            _repository = repository;
        }
        public async Task ProcessXmlAsync(Guid fileId, byte[] xmlBytes)
        {
            using var stream = new MemoryStream(xmlBytes);
            try
            {
                var xml = XDocument.Load(stream);

                string json = JsonConvert.SerializeXNode(xml, Formatting.Indented);

                JToken token = JToken.Parse(json);
                JToken renamedToken = RenameProperties(token);
                json = renamedToken.ToString(Formatting.Indented);

                if (_repository.Files.TryGetValue(fileId, out var status))
                {
                    status.JsonData = json;
                    status.IsProcessed = true;
                    status.Error = null;
                }
            }
            catch (Exception ex)
            {
                if (_repository.Files.TryGetValue(fileId, out var status))
                {
                    status.IsProcessed = false;
                    status.Error = ex.Message;
                }
            }
            await Task.CompletedTask;
        }
        private static JToken RenameProperties(JToken token)
        {
            if (token is JObject obj)
            {
                var newObj = new JObject();

                foreach (var property in obj.Properties())
                {
                    string newName = property.Name;

                    // Rename XML attributes
                    if (newName.StartsWith("@"))
                    {
                        newName = "_" + newName.Substring(1);
                    }
                    // Rename XML text node
                    else if (newName == "#text")
                    {
                        newName = "__text";
                    }

                    newObj.Add(newName, RenameProperties(property.Value));
                }

                return newObj;
            }

            if (token is JArray array)
            {
                var newArray = new JArray();

                foreach (var item in array)
                {
                    newArray.Add(RenameProperties(item));
                }

                return newArray;
            }

            // Primitive value (string, number, bool, null)
            return token.DeepClone();
        }
    }
}