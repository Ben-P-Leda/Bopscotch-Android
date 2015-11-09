using System;
using System.Collections.Generic;

namespace Leda.Core.Gamestate_Management
{
    public sealed class SceneParameters
    {
        private static SceneParameters _instance = null;
        public static SceneParameters Instance { get { if (_instance == null) { _instance = new SceneParameters(); } return _instance; } }


        private Dictionary<string, object> _parameters = new Dictionary<string, object>();

        private SceneParameters()
        {
            _parameters = new Dictionary<string, object>();
        }

        public void Set(string name, object value)
        {
            if (!_parameters.ContainsKey(name)) { _parameters.Add(name, null); }
            _parameters[name] = value;
        }

        public T Get<T>(string name)
        {
            T target = default(T);

            if (_parameters.ContainsKey(name))
            {
                try
                {
                    target = (T)Convert.ChangeType(_parameters[name], typeof(T), System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                }
            }

            return target;
        }

        public void Clear()
        {
            _parameters.Clear();
        }
    }
}
