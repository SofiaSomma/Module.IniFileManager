using System.Reflection;

namespace IniFileManager.Core
{
    public sealed class Reader
    {
        private readonly string _delimiter = "\n";

        public T GetFile<T>(string fileName)
        {
            T model = (T)Activator.CreateInstance(typeof(T));

            if (!File.Exists(fileName)) throw new FileNotFoundException();
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                string sectionName = string.Empty;
                object section = null;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0) continue;  // empty
                    if (!String.IsNullOrEmpty(_delimiter) && line.StartsWith(_delimiter))
                        continue;  // comment
                    if (line.StartsWith("[") && line.Contains("]"))
                        section = GetSection(out sectionName, line, model);
                    if (line.Contains("=")) SetProperty(section, line);
                    if (section != null)
                        ((TypeInfo)model.GetType()).DeclaredProperties.Where(x => x.Name.ToLower() == sectionName.ToLower()).FirstOrDefault().SetValue(model, section, null);

                }
            }
            return model;
        }

        private void SetProperty(object section, string line)
        {
            if (section == null) return;
            int index = line.IndexOf('=');
            string key = line.Substring(0, index).Trim();
            string val = line.Substring(index + 1).Trim();
            var prop = section.GetType().GetProperties().Where(x => x.Name.ToLower() == key.ToLower()).SingleOrDefault();
            if (prop == null) return;
            prop?.SetValue(section, Convert.ChangeType(val, prop.PropertyType));
        }

        private object GetSection<T>(out string rootPropertyName, string line, T model)
        {
            int index = line.IndexOf(']');
            var sectionName = rootPropertyName = line.Substring(1, index - 1).Trim();
            var section = ((TypeInfo)model.GetType()).DeclaredProperties
                            .Where(x => x.Name.ToLower() == sectionName.ToLower()).FirstOrDefault();

            if (section == null) return null;
            var type = Type.GetType(section.PropertyType.AssemblyQualifiedName);
            var obj = Activator.CreateInstance(type);
            return obj;

        }
    }
}