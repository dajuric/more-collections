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
using System.Runtime.InteropServices;

namespace MoreCollections
{
    /// <summary>
    /// Pinned array where elements are blittable-type objects.
    /// </summary>
    /// <typeparam name="T">Element type. The structure must have blittable types.</typeparam>
    public class PinnedArray<T>: IDisposable, IEquatable<PinnedArray<T>> where T: struct
    {
        GCHandle handle;

        /// <summary>
        /// Constructs a new pinned array. (allocation)
        /// </summary>
        /// <param name="length">Number of elements.</param>
        public PinnedArray(int length)
        {
            Array = new T[length];
            initialize(Array);
        }

        /// <summary>
        /// Constructs a new pinned array from the specified array where data is shared.
        /// </summary>
        /// <param name="array">Input array</param>
        public PinnedArray(T[] array)
        {
            this.Array = array;
            initialize(Array);
        }

        /// <summary>
        /// Constructs pinned array. (data is copied from data source)
        /// </summary>
        /// <param name="length">Number of elements.</param>
        /// <param name="dataSource">Pointer to data.</param>
        public PinnedArray(int length, IntPtr dataSource)
        {
            Array = new T[length];
            initialize(Array);

            //use unmanaged extension
            unsafe 
            {
                byte* srcPtr = (byte*)dataSource;
                byte* dstPtr = (byte*)Data;

                for (int i = 0; i < this.SizeInBytes; i++)
                {
                    dstPtr[i] = srcPtr[i];
                }
            }
        }

        private void initialize(T[] array)
        {
            handle = GCHandle.Alloc(Array, GCHandleType.Pinned);

            this.Data = handle.AddrOfPinnedObject();
            this.SizeInBytes = Array.Length * Marshal.SizeOf(default(T));
        }

        /// <summary>
        /// Disposes pinned array (frees allocated handle). 
        /// </summary>
        public void Dispose()
        {
            if (handle.IsAllocated) //this function is called for the first time
            {
                handle.Free();
                this.Array = null;
                this.Data = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Destructs pinned array (releases pinning handle).
        /// </summary>
        ~PinnedArray()
        {
            Dispose();
        }

        /// <summary>
        /// Internal pinned array.
        /// </summary>
        public T[] Array { get; private set; }
        /// <summary>
        /// Length of the array in bytes.
        /// </summary>
        public int SizeInBytes { get; private set; }
        /// <summary>
        /// Unmanaged data pointer.
        /// </summary>
        public IntPtr Data { get; private set; }

        /// <summary>
        /// Compares two arrays by checking address and length. (no data comparing).
        /// </summary>
        /// <param name="other">Second array.</param>
        /// <returns>Are equal or not.</returns>
        public bool Equals(PinnedArray<T> other)
        {
            if (other.Data != null &&
                this.Data == other.Data &&
                this.SizeInBytes == other.SizeInBytes)
            {
                return true;
            }

            return false;
        }
    }
}
