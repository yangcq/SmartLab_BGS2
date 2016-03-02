using System.Collections.Generic;
using System.Text;

namespace SmartLab.BGS2.Type
{
    public class HttpHeaders
    {
        private Dictionary<string, string> values = new Dictionary<string, string>();

        public void Add(string key, string value)
        {
            if (key == null)
                return;

            lock (values)
            {
                values.Add(key, value);
            }
        }

        public void Remove(string key)
        {
            if (key == null)
                return;

            lock (values)
            {
                values.Remove(key);
            }
        }

        public string Read(string key)
        {
            if (key == null)
                return null;

            lock (values)
            {
                return values[key];
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            lock (values)
            {
                foreach (KeyValuePair<string, string> entry in values)
                {
                    if (sb.Length != 0)
                        sb.Append("\\0d\\0a");
                    sb.Append(entry.Key);
                    sb.Append(":\\20");
                    sb.Append(entry.Value);
                }
            }
            return sb.ToString();
        }
    }
}