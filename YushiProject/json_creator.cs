using System.Text.Json;
using System.IO;

namespace SerializeToFile
{
    public static class jsonWriter
    {
        public static void write_json(string data)
        {
            string fileName = "flags.json";
            string json_string = JsonSerializer.Serialize(data);
            File.WriteAllText(fileName, json_string);
        }
    }
}