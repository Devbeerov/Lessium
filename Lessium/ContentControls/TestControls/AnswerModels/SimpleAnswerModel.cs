using Lessium.Classes.IO;
using Lessium.Interfaces;
using Lessium.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.ContentControls.TestControls.AnswerModels
{
    [Serializable]
    public class SimpleAnswerModel : IAnswerModel
    {
        private IDispatcher dispatcher;

        public string Text { get; set; } = string.Copy(Properties.Resources.DefaultAnswerHeader);

        #region Constructors

        public SimpleAnswerModel(IDispatcher dispatcher = null)
        {
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;
        }

        public SimpleAnswerModel(string text, IDispatcher dispatcher = null)
        {
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;

            Text = text;
        }

        // For serialization
        protected SimpleAnswerModel(SerializationInfo info, StreamingContext context)
        {
            this.dispatcher = DispatcherUtility.Dispatcher;

            Text = info.GetString(nameof(Text));
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            return obj is SimpleAnswerModel model &&
                   Text == model.Text &&
                   Guid.Equals(model.Guid);
        }

        public override int GetHashCode()
        {
            int hashCode = -146193094;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        #endregion

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dispatcher.Invoke(() =>
            {
                info.AddValue(nameof(Text), Text);
            });
        }

        #endregion

        #region ILsnSerializable

        public async Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token)
        {
            #region Answer

            await writer.WriteStartElementAsync("Answer");

            await dispatcher.InvokeAsync(async () =>
            {
                await writer.WriteStringAsync(Text);
            });

            await writer.WriteEndElementAsync();

            #endregion
        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            await dispatcher.InvokeAsync(async () =>
            {
                Text = await reader.ReadElementContentAsStringAsync();
            });
        }

        #endregion

        #region IAnswerModel

        public Guid Guid { get; set; } = Guid.NewGuid();

        #endregion
    }
}
