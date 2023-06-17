using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    /// <summary>
    /// interface implemented by all classes stored in <see cref="RememberedList{T, Storage}"/>.
    /// </summary>
    public interface Serializable
    {
        /// <summary>
        /// Event is invoked when the data entry is updated.
        /// </summary>
        event EventHandler<int> IGotUpdated;
        /// <summary>
        /// Invokes <see cref="IGotUpdated"/>.
        /// </summary>
        /// <param name="index">Index of updated attribute.</param>
        void InvokeIGotUpdated(int index);

        /// <summary>
        /// Sets the attribute to a new value.
        /// </summary>
        /// <param name="index">See <see cref="getAttributes"/> and <see cref="getAttributeNames"/>.</param>
        /// <param name="value">New value of the property.</param>
        void setAttribute(int index, string value);

        /// <summary>
        /// Gets list of attribute names.
        /// </summary>
        /// <returns></returns>
        string[] getAttributeNames();

        /// <summary>
        /// Gets list of attribute values of serialized item.
        /// </summary>
        /// <returns></returns>
        string[] getAttributes();

        /// <summary>
        /// Returns count of attributes.
        /// </summary>
        /// <returns></returns>
        int numOfAttributes();

        /// <summary>
        /// Unique ID of serialized item.
        /// </summary>
        /// <returns></returns>
        long getID();

        /// <summary>
        /// Basically deserialization.
        /// </summary>
        /// <param name="input">List of attributes.</param>
        void buildFromStrings(string[] input);

        /// <summary>
        /// Gets attribute at specified index. See <see cref="getAttributeNames"/> and <see cref="getAttributes"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Serialized attribute.</returns>
        string getAttribute(int index);

        /// <summary>
        /// ID of a type. Used to identify, what type is this interface.
        /// </summary>
        /// <returns></returns>
        int getTypeID();

        /// <summary>
        /// Called when the object is added into a <see cref="RememberedList{T, Storage}".
        /// </summary>
        /// <param name="list"></param>
        void addedInto(Object list);

        /// <summary>
        /// Called when the object is removed from a <see cref="RememberedList{T, Storage}".
        /// </summary>
        /// <param name="list"></param>
        void removed();
        
    }

    public interface SerializableDictionaryItem
    {
        void setAttribute(int index, string value);
        string[] getAttributeNames();
        string[] getAttributes();
        int numOfAttributes();
        string getKey();
        void buildFromStrings(string[] input);
        string getAttribute(int index);
    }
}
