using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayAtTable.TestPos
{
    public class JsonWriter 
    {
        string GetPath(string filename)
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Linkly\\";
            Directory.CreateDirectory(path);

            var fullPath = $"{path}PAT-{filename}";

            if (!File.Exists(fullPath))
                File.Copy($"PAT-{filename}", fullPath);
           
            return fullPath;
        }

        public bool Load<T>(string filename, out T items)
        {
            try
            {
                string contents = File.ReadAllText(GetPath(filename));
                items = (T)JsonConvert.DeserializeObject(contents, typeof(T));

                return (items != null);
            }
            catch
            {
                items = default(T);
            }

            return false;
        }

        public bool Save<T>(T items, string filename)
        {
            try
            {
                string json = JsonConvert.SerializeObject(items, Formatting.Indented);
                File.WriteAllText(GetPath(filename), json);

                return true;
            }
            catch
            {
            }

            return false;
        }
    }
}
