#region Licence and Terms
// More DotNET (MoreCollections)
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
using System.Linq;

namespace MoreCollections
{
    /// <summary>
    /// Stack-like structure with predefined capacity.
    /// <para>The collection contains the last n items, where n is the specified capacity. Upon element addition, the firstly added element is removed if the capacity is breached.</para>
    /// </summary>
    /// <typeparam name="T">Type object type.</typeparam>
    public class History<T> : ICloneable, IEnumerable<T>, IEnumerator<T>
    {
        /// <summary>
        /// Function delegate for adding an object.
        /// </summary>
        /// <param name="elem"></param>
        public delegate void AddElement(T elem);
        /// <summary>
        /// Represents an event that is fired when a new object is added.
        /// </summary>
        public event AddElement OnAddElement;

        private List<T> histElems;
        int maxNumOfElems;

        /// <summary>
        /// Creates a new collection.
        /// </summary>
        /// <param name="maxCount">Maximum number of elements.</param>
        public History(int maxCount = UInt16.MaxValue)
        { 
            this.histElems = new List<T>();
            this.maxNumOfElems = maxCount;
        }

        /// <summary>
        /// Adds element to the collection.
        /// </summary>
        /// <param name="elem">The specified element.</param>
        public void Add(T elem)
        { 
            if (histElems.Count == maxNumOfElems)
            {
                histElems.RemoveAt(0); //remove the oldest element
            }

            histElems.Add(elem);

            if (OnAddElement != null)
                OnAddElement(elem);
        }

        /// <summary>
        /// Gets or sets the element at specified history depth.
        /// </summary>
        /// <param name="histDepth">History depth. Zero means current state.</param>
        /// <returns>An element at specified index.</returns>
        public T this[int histDepth]
        {
            get
            {
                T elem;

                if (histDepth > histElems.Count - 1)
                    elem = histElems.FirstOrDefault(); //the oldest element
                else
                    elem = histElems[histElems.Count - histDepth - 1];

                return elem;
            }
            set
            {
                if (histElems.Count == 0)
                    this.Add(value);

                else if (histDepth > histElems.Count - 1)
                    histElems[0] = value;

                else
                    histElems[histElems.Count - histDepth - 1] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current element (depth zero).
        /// </summary>
        public T Current
        {
            get { return this[0]; }
            set { this[0] = value; }
        }

        /// <summary>
        /// Get or sets the oldest element (maximum depth).
        /// </summary>
        public T Oldest
        {
            get { return this[histElems.Count - 1]; }
            set { this[histElems.Count - 1] = value; }
        }

        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        public int Count { get { return this.histElems.Count; } }

        /// <summary>
        /// Get the number of history capacity.
        /// </summary>
        public int MaxCount { get { return this.maxNumOfElems; } }

        /// <summary>
        /// Removes all elements from the history.
        /// </summary>
        public void Clear()
        {
            this.histElems.Clear();
        }

        /// <summary>
        /// Returns part of the history. First element is the newest.
        /// </summary>
        public List<T> GetRange(int maxHistDepth)
        {
            int maxDepth = Math.Max(this.histElems.Count - maxHistDepth, 0);

            List<T> range = new List<T>();
            for (int i = this.histElems.Count - 1; i >= maxDepth; i--)
            {
                range.Add(this.histElems[i]);
            }

            return range;
        }

        /// <summary>
        /// Removes the specified range from the history.
        /// </summary>
        /// <param name="startDepth">Starting depth.</param>
        /// <param name="numOfElems">Number of elements to remove. If specified more than maximum, maximum elements will be removed.</param>
        public void RemoveRange(int startDepth, int numOfElems)
        {
            if (startDepth >= this.histElems.Count)
                return;

            int index = Math.Max(this.histElems.Count - 1 - startDepth - numOfElems, 0);
            int count = Math.Min(numOfElems, this.histElems.Count - 1 - index);

            this.histElems.RemoveRange(index, count);
        }

        /// <summary>
        /// Gets all elements.
        /// </summary>
        /// <returns>The list of elements.</returns>
        public List<T> GetAllElements()
        {
            return GetRange(this.Count);
        }

        /// <summary>
        /// Gets the string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            string str = "";
            foreach (var item in histElems)
            {
                str += item.ToString() + ", ";
            }
       
            str = str.Remove(str.Length - 3);
            return str;
        }

        /// <summary>
        /// Clones the history. The data is shared.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            History<T> hist = new History<T>(this.maxNumOfElems);
            hist.histElems.AddRange(this.histElems);
            return hist;
        }

        #region IEnumerable
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this;
        }

        T IEnumerator<T>.Current
        {
            get { return this[idx]; }
        }

        void IDisposable.Dispose()
        { }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        bool System.Collections.IEnumerator.MoveNext()
        {
            return (++idx < this.Count);
        }

        int idx = -1;
        void System.Collections.IEnumerator.Reset()
        {
            idx = -1;
        }
        #endregion
    }
}
