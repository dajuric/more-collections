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
using System.Collections.Generic;

namespace MoreCollections
{
    /// <summary>
    /// Read-only indirect indexing collection.
    /// Represents a container for a collection and its index collection.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public class IndexedCollection<T>: IReadOnlyList<T>
    {
        IList<T> collection;
        IList<int> indices;

        /// <summary>
        /// Creates a new indexed collection.
        /// </summary>
        /// <param name="collection">Collection.</param>
        /// <param name="indexes">Indexes.</param>
        public IndexedCollection(IList<T> collection, IList<int> indexes)
        {
            if (collection.Count < indexes.Count)
                throw new Exception("Collection and indexes collections must have the same number of elements.");

            this.collection = collection;
            this.indices = indexes;
        }

        /// <summary>
        /// Gets the element of the provided collection.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>Collection element.</returns>
        public T this[int index]
        {
            get 
            {
                var realIdx = indices[index];
                return collection[realIdx];
            }
        }

        /// <summary>
        /// Gets the number of elements within collection.
        /// </summary>
        public int Count
        {
            get { return indices.Count; }
        }

        /// <summary>
        /// Gets the enumerator for the indexed collection.
        /// </summary>
        /// <returns>Enumerator for the indexed collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new IndexedCollectionEnumerator(this.collection, this.indices);
        }

        /// <summary>
        /// Gets the enumerator for the indexed collection.
        /// </summary>
        /// <returns>Enumerator for the indexed collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Indexed collection enumerator.
        /// </summary>
        class IndexedCollectionEnumerator: IEnumerator<T>
        {
            IList<T> collection;
            IList<int> indexes;
            int index = -1;

            /// <summary>
            /// Creates new indexed collection enumerator.
            /// </summary>
            /// <param name="collection">Collection.</param>
            /// <param name="indices">Indexes.</param>
            public IndexedCollectionEnumerator(IList<T> collection, IList<int> indices)
            {
                if (collection.Count < indices.Count)
                    throw new Exception("The number of elements within the collection must be greater that the number of elements within indexes.");

                this.collection = collection;
                this.indexes = indices;
            }

            /// <summary>
            /// Gets the current element.
            /// </summary>
            public T Current
            {
                get
                {
                    var realIdx = indexes[index];
                    return collection[realIdx];
                }
            }

            /// <summary>
            /// Disposes the indexed collection enumerator instance.
            /// </summary>
            public void Dispose()
            {}

            /// <summary>
            /// Gets the current element.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            /// <summary>
            /// Moves the index to the next element.
            /// </summary>
            /// <returns>True if the move operation is valid, false otherwise.</returns>
            public bool MoveNext()
            {
                index++;
                return index < indexes.Count;
            }

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            public void Reset()
            {
                index = -1;
            }
        }
    }
}
