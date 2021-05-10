using Lessium.Interfaces;
using System.Windows;
using System.Runtime.Serialization;

namespace Lessium.Services
{
    public static class ClipboardService
    {
        public static void CopySerializable(ILsnSerializable serializable)
        {
            if (serializable == null) return;

            var dataObject = new DataObject(DataFormats.Serializable, serializable);

            Clipboard.Clear();
            Clipboard.SetDataObject(dataObject);
        }

        public static ILsnSerializable GetStoredSerializable()
        {
            var dataObject = Clipboard.GetDataObject();
            if (!dataObject.GetDataPresent(DataFormats.Serializable)) return null;

            var serializable = dataObject.GetData(DataFormats.Serializable) as ILsnSerializable;
            if (serializable == null) throw new SerializationException("Stored object is not ILsnSerializable.");

            return serializable;
        }
    }
}
