using Lessium.Interfaces;
using System.Windows;

namespace Lessium.Utility
{
    public static class ILsnSerializableExtensions
    {
        /// <summary>
        /// Using Serialization of ILsnSerializable for cloning.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializable"></param>
        /// <returns></returns>
        public static T CloneSerializable<T>(this T serializable) where T: ILsnSerializable
        {
            var serializableObject = new DataObject(DataFormats.Serializable, serializable);
            var newSerializable = (T) serializableObject.GetData(DataFormats.Serializable);

            return newSerializable;
        }
    }
}
