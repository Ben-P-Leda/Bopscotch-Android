using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Serialization
{
    public class Serializer
    {
        public delegate object CustomDeserializer(string source);

        private XElement _serializedData = null;
        private Dictionary<Type, CustomDeserializer> _customDeserializers;
        private List<ISerializable> _knownSerializedObjects;

        public XElement SerializedData { get { return _serializedData; } }

        public List<ISerializable> KnownSerializedObjects 
        { 
            set 
            { 
                _knownSerializedObjects = value; 
            }
            get
            {
                if (_knownSerializedObjects == null) { _knownSerializedObjects = new List<ISerializable>(); }
                return _knownSerializedObjects;
            }
        }

        public Serializer(ISerializable target)
        {
            _serializedData = new XElement("object");

            _serializedData.Add(new XAttribute("id", target.ID));
            _serializedData.Add(new XAttribute("type", target.GetType()));
        }

        public Serializer(XElement serializedData)
        {
            _serializedData = serializedData;

            _knownSerializedObjects = null;

            _customDeserializers = new Dictionary<Type, CustomDeserializer>();
            _customDeserializers.Add(typeof(Vector2), DeserializeVector2);
            _customDeserializers.Add(typeof(Range), DeserializeRange);
            _customDeserializers.Add(typeof(Point), DeserializePoint);
            _customDeserializers.Add(typeof(Color), DeserializeColor);
            _customDeserializers.Add(typeof(Rectangle), DeserializeRectangle);
        }

        public void AddDataItem(string name, object value)
        {
            if (value != null)
            {
                var item = new XElement("dataitem");

                item.Add(new XAttribute("name", name));

                if (value is ISerializable) { item.Add(((ISerializable)value).Serialize()); }
                else { item.SetValue(value); }

                _serializedData.Add(item);
            }
        }

        public void AddDataForStaticListOfSerializables(string listName, List<ISerializable> listToAdd)
        {
            if ((listToAdd != null) && (listToAdd.Count > 0))
            {
                XElement item = new XElement("dataitem");

                item.Add(new XAttribute("name", listName));
                for (int i = 0; i < listToAdd.Count; i++) { item.Add(listToAdd[i].Serialize()); }

                if (item.Elements("object").Count() > 0) { _serializedData.Add(item); }
            }
        }

        public void AddDataElement(XElement nodeToAdd)
        {
            _serializedData.Add(nodeToAdd);
        }

        public T GetDataItem<T>(string name)
        {
            return GetDataItem<T>(name, _serializedData);
        }

        public T GetDataItem<T>(string name, XElement source)
        {
            T target = default(T);
            List<XElement> dataItems = (from el
                                        in source.Elements("dataitem")
                                        where (string)el.Attribute("name") == name
                                        select el).ToList();

            if (dataItems.Count > 0)
            {
                if (typeof(ISerializable).IsAssignableFrom(typeof(T)))
                {
                    bool alreadyCreated = false;
                    if ((_knownSerializedObjects != null) && (_knownSerializedObjects.Count > 0))
                    {
                        for (int i = 0; i < _knownSerializedObjects.Count; i++)
                        {
                            if ((_knownSerializedObjects[i].ID == dataItems[0].Element("object").Attribute("id").Value) &&
                                (_knownSerializedObjects[i].GetType().ToString() == dataItems[0].Element("object").Attribute("type").Value))
                            {
                                target = (T)_knownSerializedObjects[i];
                                alreadyCreated = true;
                            }
                        }
                    }

                    if (!alreadyCreated)
                    {
                        target = Activator.CreateInstance<T>();
                        ((ISerializable)target).ID = dataItems[0].Element("object").Attribute("id").Value;
                    }

                    ((ISerializable)target).Deserialize(dataItems[0].Element("object"));
                }
                else if (typeof(T).IsEnum)
                {
                    foreach (T item in Enum.GetValues(typeof(T)))
                    {
                        if (item.ToString().ToLower().Equals(dataItems[0].Value.Trim().ToLower())) { target = item; break; }
                    }
                }
                else
                {
                    bool deserialized = false;
                    bool noError = true;

                    foreach (KeyValuePair<Type, CustomDeserializer> kvp in _customDeserializers)
                    {
                        if (typeof(T) == kvp.Key)
                        {
                            try
                            {
                                target = (T)Convert.ChangeType(kvp.Value(dataItems[0].Value), typeof(T), System.Globalization.CultureInfo.InvariantCulture);
                                deserialized = true;
                                break;
                            }
                            catch
                            {
                                noError = false;
                                break;
                            }
                        }
                    }

                    if ((!deserialized) && (noError))
                    {
                        try
                        {
                            target = (T)Convert.ChangeType(dataItems[0].Value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return target;
        }

        public void GetDataForStaticListOfSerializables(string listName, List<ISerializable> targetList)
        {
            List<XElement> dataItems = (from el
                                        in _serializedData.Elements("dataitem")
                                        where (string)el.Attribute("name") == listName
                                        select el).ToList();

            if ((dataItems.Count > 0) && (dataItems[0].Elements("object").Count() > 0))
            {
                for (int i = 0; i < targetList.Count; i++)
                {
                    List<XElement> objectData = (from el
                        in dataItems[0].Elements("object")
                        where ( (string)el.Attribute("id") == targetList[i].ID && (string)el.Attribute("type") == targetList[i].GetType().ToString())
                        select el).ToList();

                    if (objectData.Count > 0) { targetList[i].Deserialize(objectData[0]); }
                }
            }
        }

        public XElement GetDataElement(string elementName)
        {
            return _serializedData.Element(elementName);
        }

        private object DeserializeVector2(string source)
        {
            try
            {
                string[] components = source.Split(' ');
                for (int i = 0; i < components.Length; i++)
                {
                    components[i] = components[i].Trim();
                    components[i] = components[i].Substring(components[i].IndexOf(":") + 1);
                }
                return new Vector2(
                    (float)Convert.ToDouble(components[0]), 
                    (float)Convert.ToDouble(components[1].Trim('}')));
            }
            catch
            {
                return null;
            }
        }

        private object DeserializeRange(string source)
        {
            try
            {
                string[] components = source.Split(' ');
                for (int i = 0; i < components.Length; i++)
                {
                    components[i] = components[i].Trim();
                    components[i] = components[i].Substring(components[i].IndexOf(":") + 1);
                }
                return new Range(
                    (float)Convert.ToDouble(components[0]),
                    (float)Convert.ToDouble(components[1].Trim('}')));
            }
            catch
            {
                return null;
            }
        }

        private object DeserializeColor(string source)
        {
            try
            {
                string[] components = source.Split(ColorElementSeparator);
                for (int i = 0; i < components.Length; i++)
                {
                    components[i] = components[i].Trim();
                    components[i] = components[i].Substring(components[i].IndexOf(ColorValueSeparator) + 1);
                }
                return new Color(
                    Convert.ToInt16(components[0]),
                    Convert.ToInt16(components[1]),
                    Convert.ToInt16(components[2]),
					Convert.ToInt16(components[3].Trim('}')));
            }
            catch
            {
                return null;
            }
        }

		private char ColorElementSeparator
		{
			get
			{
#if ANDROID
				return ' ';
#else
				return ',';
#endif
			}
		}

		private string ColorValueSeparator
		{
			get
			{
#if ANDROID
				return ":";
#else
				return "=";
#endif
			}
		}

        private object DeserializePoint(string source)
        {
            try
            {
                string[] components = source.Split(' ');
                for (int i = 0; i < components.Length; i++)
                {
                    components[i] = components[i].Trim();
                    components[i] = components[i].Substring(components[i].IndexOf(":") + 1);
                }
                return new Point(
                    Convert.ToInt32(components[0]),
                    Convert.ToInt32(components[1].Trim('}')));
            }
            catch
            {
                return null;
            }
        }

        private object DeserializeRectangle(string source)
        {
            try
            {
                string[] components = source.Split(' ');
                for (int i = 0; i < components.Length; i++)
                {
                    components[i] = components[i].Trim();
                    components[i] = components[i].Substring(components[i].IndexOf(":") + 1);
                }
                return new Rectangle(
                    Convert.ToInt32(components[0]),
                    Convert.ToInt32(components[1]),
                    Convert.ToInt32(components[2]),
                    Convert.ToInt32(components[3].Trim('}')));
            }
            catch
            {
                return null;
            }
        }
    }
}
