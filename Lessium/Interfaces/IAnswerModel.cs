using System;

namespace Lessium.Interfaces
{
    /// <summary>
    /// Each AnswerModel should be unique, so it will be handled properly by ItemControls.
    /// </summary>
    public interface IAnswerModel : ILsnSerializable
    {
        /// <summary>
        /// Ensures that each IAnswerModel is unique. 
        /// Should be also used in Equals and GetHashCode.
        /// </summary>
        Guid Guid { get; set; }
    }
}
