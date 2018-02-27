using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KoikatsuUnlimited
{
    abstract public class Extension
    {
        /// <summary>
        /// The unique identifier of the plugin. Should not change between plugin versions.
        /// </summary>
        public abstract string ID { get; }

        /// <summary>
        /// The user friendly name of the plugin. Is able to be changed between versions.
        /// </summary>
        public abstract string Name { get; }

        /*public object ExtensionData
        {
            get
            {
                object data = null;
                KoikatsuUnlimited.ExtensionData.TryGetValue(ID, out data);
                return data;
            }
            set { KoikatsuUnlimited.ExtensionData[ID] = value; }
        }*/

        public object GetCharacterExtensionData(ChaFile character)
        {
            return KoikatsuUnlimited.GetExtensionData(character, ID);
        }

        public void SetCharacterExtensionData(ChaFile character, object data)
        {
            KoikatsuUnlimited.SetExtensionData(character, ID, data);
        }

        public abstract void Start();

        public abstract void WindowFunction(int windowID);

    }
}
