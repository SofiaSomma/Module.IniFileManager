using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IniFileManager.Core
{
    public sealed class Writer
    {
        private Dictionary<string, object> pairs = new Dictionary<string, object>();

        public void SetFile<T>(string fileName, T model)
        {
            SetDictionary(model);

            if (pairs.Count == 0) return;

            if (!File.Exists(fileName))
            {
                var file = File.AppendText(fileName);
                file.Close();
            }

            WriteFile(fileName);
        }

        private void WriteFile(string fileName)
        {
            var section = pairs.FirstOrDefault().Key.Split('_')[0];

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine($"[{section.ToUpper()}]");
                foreach (var pair in pairs)
                {
                    if (pair.Key.Split('_')[0] != section)
                    {
                        section = pair.Key.Split('_')[0];
                        sw.WriteLine($"[{section.ToUpper()}]");
                    }

                    var prop = pair.Key.Split('_')[1];
                    sw.WriteLine($"{prop}={pair.Value}".ToUpper());
                }
            }
        }

        private void SetDictionary<T>(T model)
        {
            var nestedObjNames = model.GetType().GetProperties().Select(x => x.Name).ToList();

            foreach (var nestedObjectName in nestedObjNames)
            {
                var nestedObj = model.GetType().GetProperties().Where(x => x.Name == nestedObjectName).FirstOrDefault();
                var propertiesName = ((TypeInfo)(nestedObj).PropertyType).DeclaredProperties.Select(x => x.Name).ToList();

                foreach (var propertyName in propertiesName)
                {
                    AddValueToTheDictionary(nestedObj, propertyName, nestedObjectName, model);
                }
            }
        }

        private void AddValueToTheDictionary<T>(PropertyInfo nestedObj, string propertyName, string nestedObjName, T model)
        {
            var val = nestedObj.GetValue(model).GetType().GetProperties().Where(x => x.Name == propertyName).FirstOrDefault().GetValue(nestedObj.GetValue(model));
            pairs.Add($"{nestedObjName}_{propertyName}", val);
        }
    }
}
