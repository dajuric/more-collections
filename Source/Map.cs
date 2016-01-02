#region Licence and Terms
// MoreCollections
// https://github.com/more-dotnet/more-collections
//
// Copyright © Darko Jurić, 2015 
// darko.juric2@gmail.com
//
//The MIT License (MIT)
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoreCollections
{
    //Taken from: http://stackoverflow.com/questions/10966331/two-way-bidirectional-dictionary-in-c" and modified.
    /// <summary>
    /// Two-way dictionary (keyA - keyB). The between key association is 1-1.
    /// </summary>
    /// <typeparam name="T1">First key type.</typeparam>
    /// <typeparam name="T2">Second key type.</typeparam>
    public class Map<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        /// <summary>
        /// Represents all associations associated with a key.
        /// </summary>
        /// <typeparam name="T3">First key type.</typeparam>
        /// <typeparam name="T4">Second key type.</typeparam>
        public class Indexer<T3, T4> : IEnumerable<T3>
        {
            private Dictionary<T3, T4> dictionary;
            /// <summary>
            /// Creates new instance from a dictionary.
            /// </summary>
            /// <param name="dictionary">Association dictionary.</param>
            public Indexer(Dictionary<T3, T4> dictionary)
            {
                this.dictionary = dictionary;
            }

            /// <summary>
            /// Gets the associated key.
            /// </summary>
            /// <param name="index">Key.</param>
            /// <returns></returns>
            public T4 this[T3 index]
            {
                get { return dictionary[index]; }
                set { dictionary[index] = value; }
            }

            /// <summary>
            /// Gets the associated key.
            /// </summary>
            /// <param name="index">Key.</param>
            /// <param name="value">Associated value (other key).</param>
            /// <returns>Returns true if the specified key exists, otherwise returns false.</returns>
            public bool TryGetValue(T3 index, out T4 value)
            {
                return dictionary.TryGetValue(index, out value);
            }

            /// <summary>
            /// Determines whether the specified key exists.
            /// </summary>
            /// <param name="index">Key.</param>
            /// <returns>Returns true if the specified key exists otherwise returns false.</returns>
            public bool Contains(T3 index)
            {
                return dictionary.ContainsKey(index);
            }

            /// <summary>
            /// Return the enumerator that iterates through the collection.
            /// </summary>
            /// <returns>Collection enumerator.</returns>
            public IEnumerator<T3> GetEnumerator()
            {
                return dictionary.Keys.GetEnumerator();
            }

            /// <summary>
            /// Return the enumerator that iterates through the collection.
            /// </summary>
            /// <returns>Collection enumerator.</returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private Dictionary<T1, T2> forward = new Dictionary<T1, T2>();
        private Dictionary<T2, T1> reverse = new Dictionary<T2, T1>();

        /// <summary>
        /// Initializes a new instance of <see cref="MoreCollections.Map{T1, T2}"/>.
        /// </summary>
        public Map()
        {
            this.Forward = new Indexer<T1, T2>(forward);
            this.Reverse = new Indexer<T2, T1>(reverse);
        }

        /// <summary>
        /// Adds the specified association.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public void Add(T1 t1, T2 t2)
        {
            forward.Add(t1, t2);
            reverse.Add(t2, t1);
        }

        /// <summary>
        /// Removes the associations specified by the key.
        /// </summary>
        /// <param name="t1">Forward key.</param>
        /// <returns>True if the assocaitons exists, false otherwise.</returns>
        public bool TryRemove(T1 t1)
        {
            if (!forward.ContainsKey(t1))
                return false;

            var val = forward[t1];
            forward.Remove(t1);
            reverse.Remove(val);
            return true;
        }

        /// <summary>
        /// Removes the associations specified by the key.
        /// </summary>
        /// <param name="t2">Reverse key.</param>
        /// <returns>True if the assocaitons exists, false otherwise.</returns>
        public bool TryRemove(T2 t2)
        {
            if (!reverse.ContainsKey(t2))
                return false;

            var val = reverse[t2];
            reverse.Remove(t2);
            forward.Remove(val);
            return true;
        }

        /// <summary>
        /// Gets all associations with the first <typeparamref name="T1"/> key.
        /// </summary>
        public Indexer<T1, T2> Forward { get; private set; }
        /// <summary>
        /// Gets all associations with the second <typeparamref name="T2"/> key.
        /// </summary>
        public Indexer<T2, T1> Reverse { get; private set; }

        /// <summary>
        /// Removes all associations from the <see cref="Accord.Extensions.Map{T1, T2}"/>.
        /// </summary>
        public void Clear()
        {
            forward.Clear();
            reverse.Clear();
        }

        /// <summary>
        /// Gets the enumerator of the collections.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return forward.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator of the collections.
        /// </summary>
        /// <returns>Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return forward.GetEnumerator();
        }
    }
}
