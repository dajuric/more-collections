using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.Devices;

namespace MoreCollections
{
    /// <summary>
    /// Interface for the Lazy cache object item.
    /// </summary>
    /// <typeparam name="TObj">Object value.</typeparam>
    public interface ILazy<TObj>
    {
        /// <summary>
        /// Returns true if the object value is loaded in the memory.
        /// </summary>
        bool IsValueCreated { get; }

        /// <summary>
        /// Gets the object value.
        /// If the object value is not loaded, the object will be constructed.
        /// </summary>
        TObj Value { get; }
    }

    /// <summary>
    /// Lazy memory cache.
    /// Caches object constructor and destructor into RAM, so when a user requests an object by using appropriate key an object is loaded into memory.
    /// An object will be removed automatically from memory by using LRU strategy.
    /// 
    /// <para>Use this class for loading collections that can not fit into memory. 
    /// This class provides convenient interface where the cache itself can be represented as collection.
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="TKey">Object key.</typeparam>
    /// <typeparam name="TValue">Object value.</typeparam>
    public class LazyMemoryCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ILazy<TValue>>>
        where TValue : class
    {
        /// <summary>
        /// Represents key value pair cache collection which is using "Least Recently Used (LRU)" element replace when the capacity is reached.
        /// </summary>
        /// <typeparam name="K">Key.</typeparam>
        /// <typeparam name="V">Value.</typeparam>
        private class LRUCache<K, V> : IDictionary<K, V>
        {
            /// <summary>
            /// Occurs when the item is about to be removed.
            /// </summary>
            /// <param name="sender">LRU cache instance.</param>
            /// <param name="item">Item to be removed.</param>
            /// <param name="userRequested">Is the removal user request or is it performed automatically by cache.</param>
            public delegate void RemovedItem(LRUCache<K, V> sender, KeyValuePair<K, V> item, bool userRequested);

            /// <summary>
            /// Occurs when the LRUCache is about to discard its oldest item
            /// because its capacity has been reached and a new item is being added.  
            /// </summary>
            /// <remarks>The item has not been discarded yet, and thus is still contained in 
            /// the Oldest property.</remarks>
            public event RemovedItem OnRemoveItem;

            // The index into the list, used by Add, Remove, and Contains.
            Dictionary<K, V> dict;

            // The list of items in the cache.  New items are added to the end of the list;
            // existing items are moved to the end when added; the items thus appear in
            // the list in the order they were added/used, with the least recently used
            // item being the first.  This is internal because the LRUCacheEnumerator
            // needs to access it.

            LinkedList<K> queue = null;

            /// <summary>
            /// Add, Clear, CopyTo, and Remove lock on this object to keep them thread-safe.
            /// </summary>
            object syncObj = new object();

            /// <summary>
            /// User-defined function for getting an stopping condition. 
            /// Parameters: total cache size (user defined).
            /// </summary>
            Func<bool> isCapacityReached;

            /// <summary>
            /// Initializes a new instance of the LRUCache class that is empty and has the specified
            /// initial capacity.
            /// </summary>
            /// <param name="isCapacityReached">Func that return true if the capacity is reached.</param>
            public LRUCache(Func<bool> isCapacityReached)
            {
                dict = new Dictionary<K, V>();
                queue = new LinkedList<K>();

                this.isCapacityReached = isCapacityReached;
            }

            /// <summary>
            /// Gets the number of items contained in the LRUCache.
            /// </summary>
            public int Count
            {
                get { lock (syncObj) return dict.Count; }
            }

            /// <summary>
            /// The oldest (i.e. least recently used) item in the LRUCache.
            /// </summary>
            public KeyValuePair<K, V> Oldest
            {
                get
                {
                    var oldestKey = queue.First.Value;

                    V val;
                    dict.TryGetValue(oldestKey, out val);

                    return new KeyValuePair<K, V>(oldestKey, val);
                }
            }

            /// <summary>
            /// Add an item to the LRUCache, making it the newest item (i.e. the last
            /// item in the list). If the key is already in the LRUCache, its value is replaced.
            /// </summary>
            /// <param name="key">Key.</param>
            /// <param name="value">Value.</param>
            /// <remarks>If the LRUCache has a nonzero capacity, and it is at its capacity, this 
            /// method will discard the oldest item, raising the DiscardingOldestItem event before 
            /// it does so.</remarks>
            public void Add(K key, V value)
            {
                Add(new KeyValuePair<K, V>(key, value));
            }

            /// <summary>
            /// Add an item to the LRUCache, making it the newest item (i.e. the last
            /// item in the list). If the key is already in the LRUCache, an exception is thrown.
            /// </summary>
            /// <param name="pair">The item that is being used.</param>
            /// <remarks>If the LRUCache has a nonzero capacity, and it is at its capacity, this 
            /// method will discard the oldest item, raising the DiscardingOldestItem event before 
            /// it does so.</remarks>
            public void Add(KeyValuePair<K, V> pair)
            {
                bool contains = false;
                lock (syncObj)
                    contains = dict.ContainsKey(pair.Key);

                if (contains)
                    throw new Exception("The key already exist. Use AddOrUpdate command to update existing key.");
            }

            /// <summary>
            /// Add an item to the LRUCache, making it the newest item (i.e. the last
            /// item in the list). If the key is already in the LRUCache, its value is replaced.
            /// </summary>
            /// <param name="key">Data key value.</param>
            /// <param name="value">Value.</param>
            ///  /// <remarks>If the LRUCache has a nonzero capacity, and it is at its capacity, this 
            /// method will discard the oldest item, raising the DiscardingOldestItem event before 
            /// it does so.</remarks>
            public void AddOrUpdate(K key, V value)
            {
                lock (syncObj)
                {
                    //if the key is already inside remove it
                    tryRemove(key, false);

                    queue.AddLast(key);
                    dict[key] = value;

                    while (isCapacityReached() && dict.Count != 0)
                    {
                        // cache full, so re-use the oldest node
                        var node = queue.First;
                        tryRemove(node.Value, false);
                    }
                }
            }

            private bool tryRemove(K key, bool userRequested)
            {
                lock (syncObj)
                {
                    V val;
                    var success = dict.TryGetValue(key, out val);

                    if (success)
                    {
                        if (OnRemoveItem != null)
                            OnRemoveItem(this, this.Oldest, userRequested);

                        dict.Remove(key);
                        queue.Remove(key);
                    }

                    return success;
                }
            }

            /// <summary>
            /// Remove the specified item from the LRUCache.
            /// </summary>
            /// <param name="key">The key of the item to remove from the LRUCache.</param>
            /// <returns>true if the item was successfully removed from the LRUCache,
            /// otherwise false.  This method also returns false if the item was not
            /// found in the LRUCache.</returns>
            public bool Remove(K key)
            {
                return tryRemove(key, true);
            }

            /// <summary>
            /// Clear the contents of the LRUCache.
            /// </summary>
            public void Clear()
            {
                lock (syncObj)
                {
                    queue.Clear();
                    dict.Clear();
                }
            }

            /// <summary>
            /// Gets the value associated with the specified key.
            /// </summary>
            /// <param name="key">Key.</param>
            /// <param name="value">Value.</param>
            /// <returns> 
            /// True if the System.Collections.Generic.Dictionary{TKey,TValue} contains an 
            /// element with the specified key; otherwise, false.
            /// </returns>
            public bool TryGetValue(K key, out V value)
            {
                lock (syncObj)
                {
                    if (dict.TryGetValue(key, out value))
                    {
                        queue.Remove(key);
                        queue.AddLast(key);
                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// Determines whether the LRUCache contains a specific value.
            /// </summary>
            /// <param name="key">The key of the item to locate in the LRUCache.</param>
            /// <returns>true if the item is in the LRUCache, otherwise false.</returns>
            public bool ContainsKey(K key)
            {
                return dict.ContainsKey(key);
            }

            /// <summary>
            /// Returns keys.
            /// </summary>
            public ICollection<K> Keys
            {
                get { return dict.Keys; }
            }

            /// <summary>
            /// Returns values.
            /// </summary>
            public ICollection<V> Values
            {
                get { return dict.Values; }
            }

            /// <summary>
            /// Gets or sets value which is associated with specified key.
            /// If the key already exist a previous value will be updated. 
            /// The appropriate events will be fired.
            /// </summary>
            /// <param name="key">Object key.</param>
            /// <returns>Value.</returns>
            public V this[K key]
            {
                get
                {
                    lock (syncObj) return dict[key];
                }
                set
                {
                    AddOrUpdate(key, value);
                }
            }

            /// <summary>
            /// Determines whether the cache contains specified item or not.
            /// </summary>
            /// <param name="item">Specified item.</param>
            /// <returns>True if the item is in cache, false otherwise.</returns>
            public bool Contains(KeyValuePair<K, V> item)
            {
                lock (syncObj)
                {
                    V value;
                    if (!this.TryGetValue(item.Key, out value))
                        return false;

                    return EqualityComparer<V>.Default.Equals(value, item.Value);
                }
            }

            /// <summary>
            /// Copies the elements of the LRUCache to an array, starting at a particular 
            /// array index.
            /// </summary>
            /// <param name="array">The one-dimensional array that is the destination of
            /// items copied from the LRUCache.</param>
            /// <param name="arrayIndex">The index in array at which copying begins.</param>
            public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
            {
                lock (syncObj)
                {
                    if (array == null)
                        throw new ArgumentNullException("array");

                    if (arrayIndex < 0 || arrayIndex > array.Length)
                        throw new ArgumentOutOfRangeException("arrayIndex");

                    if ((array.Length - arrayIndex) < dict.Count)
                        throw new ArgumentException("Destination array is not large enough. Check array.Length and arrayIndex.");

                    foreach (var item in dict)
                        array[arrayIndex++] = item;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the LRUCache is read-only.
            /// </summary>
            public bool IsReadOnly
            {
                get { return false; }
            }

            /// <summary>
            /// Remove the specified item from the LRUCache.
            /// </summary>
            /// <param name="item">The item to remove from the LRUCache.</param>
            /// <returns>true if the item was successfully removed from the LRUCache,
            /// otherwise false. This method also returns false if the item was not
            /// found in the LRUCache.</returns>
            public bool Remove(KeyValuePair<K, V> item)
            {
                lock (syncObj)
                {
                    if (!dict.Contains(item))
                        return false;

                    return this.Remove(item.Key);
                }
            }

            /// <summary>
            /// Returns an enumerator that iterates through the items in the LRUCache.
            /// </summary>
            /// <returns>An IEnumerator object that may be used to iterate through the 
            /// LRUCache./></returns>
            public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            {
                LinkedListNode<K> node = queue.First;
                while (node != null)
                {
                    yield return new KeyValuePair<K, V>(node.Value, dict[node.Value]);
                    node = node.Next;
                }
            }

            /// <summary>
            /// Returns an enumerator that iterates through the items in the LRUCache.
            /// </summary>
            /// <returns>An IEnumerator object that may be used to iterate through the 
            /// LRUCache./></returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)dict.GetEnumerator();
            }
        }

        /// <summary>
        /// Represents lazy cache item. 
        /// The object value can be loaded on demand and also unloaded. The appropriate events are also given.
        /// </summary>
        /// <typeparam name="ObjKey">Object key.</typeparam>
        /// <typeparam name="TObj">Object value.</typeparam>
        private class LazyCacheItem<ObjKey, TObj> : ILazy<TObj>, IDisposable
               where TObj : class
        {
            public event EventHandler OnValueLoaded;
            public event EventHandler OnValueUnloaded;

            private Func<ObjKey, TObj> constructor;
            private Action<TObj> destructor;
            private TObj value = null;

            /// <summary>
            /// Constructs new lazy cache item.
            /// </summary>
            /// <param name="key">Key of the object.</param>
            /// <param name="constructor">Object constructor.</param>
            public LazyCacheItem(ObjKey key, Func<ObjKey, TObj> constructor)
                :this(key, constructor, obj => { })
            { }

            /// <summary>
            /// Constructs new lazy cache item.
            /// </summary>
            /// <param name="key">Key of the object.</param>
            /// <param name="constructor">Object constructor.</param>
            /// <param name="destructor">Object destructor.</param>
            public LazyCacheItem(ObjKey key, Func<ObjKey, TObj> constructor, Action<TObj> destructor)
            {
                this.constructor = constructor;
                this.destructor = destructor;
                this.Key = key;
            }

            /// <summary>
            /// Returns true if the object value is loaded in the memory.
            /// </summary>
            public bool IsValueCreated
            {
                get
                {
                    return value != null;
                }
            }

            /// <summary>
            /// Gets the object value.
            /// If the object value is not loaded, the object will be constructed.
            /// </summary>
            public TObj Value
            {
                get
                {
                    if (value == null)
                    {
                        lock (this)
                        {
                            value = constructor(this.Key);
                            if (OnValueLoaded != null)
                                OnValueLoaded(this, new EventArgs());
                        }
                    }

                    return value;
                }
            }

            /// <summary>
            /// Gets the object key.
            /// </summary>
            public ObjKey Key { get; private set; }

            /// <summary>
            /// Unloads object from the memory (destructs).
            /// </summary>
            public void Unload()
            {
                if (this.value != null)
                {
                    destructor(value);
                    value = null;
                    if (OnValueUnloaded != null)
                        OnValueUnloaded(this, new EventArgs());
                }
            }

            bool isDisposed = false;
            /// <summary>
            /// Disposes the contained object.
            /// </summary>
            public void Dispose()
            {
                if (!isDisposed)
                {
                    Unload();
                    isDisposed = true;
                }
            }
        }

        /// <summary>
        /// Structure that contains all objects (objects that consume memory + reference objects).
        /// </summary>
        private ConcurrentDictionary<TKey, ILazy<TValue>> cache;
        /// <summary>
        /// Management strategy (LRU) that is responsible for automatically object unloading.
        /// </summary>
        private LRUCache<TKey, LazyCacheItem<TKey, TValue>> managmentStrategy;
        /// <summary>
        /// Forces GC.Collect()  (user option).
        /// </summary>
        private bool forceCollectionOnRemoval;
        /// <summary>
        /// Sync object, needed for non-concurrent structures.
        /// </summary>
        object syncObj = new object();

        /// <summary>
        /// Constructs lazy memory cache which caches object constructor and destructor.
        /// <para>Value loading is handled in a lazy way (JIT), and it is automatically unloaded from memory when a specified capacity is reached.</para>
        /// <para>The memory management is handled by LRU strategy.</para>
        /// </summary>
        /// <param name="forceCollectionOnRemoval">
        /// <para>When set to true calls GC.Collect() when a value is unloaded due to capacity reach, but the CPU consumption can be high and accessing / adding elements can be temporary delay due to garbage collector.</para>
        /// <para>If false the GC.Collect() is not called which can lead to cache evict values more aggressively which could be avoided by setting this flag to true.
        /// Also the capacity will be probably breached but the memory overflow exception should not be thrown.
        /// </para>
        /// </param>
        public LazyMemoryCache(float maxMemoryOccupation = 0.8f, bool forceCollectionOnRemoval = true)
        {
            Initialize(isCacheReachedCapacity, forceCollectionOnRemoval);
            this.MaxMemoryOccupation = maxMemoryOccupation;
        }

        static ComputerInfo computerInfo = new ComputerInfo(); //reference to Microsoft.VisualBasic assembly.
        bool isCacheReachedCapacity()
        {
            var occupied = computerInfo.TotalPhysicalMemory - computerInfo.AvailablePhysicalMemory;
            var occupiedPercentage = (float)occupied / computerInfo.TotalPhysicalMemory;

            //WATCH OUT! You can get OutOfMemoryException although the RAM is not full:
            //when creating fields with zeros I assume there are some OS optimizations like block sharing
            //if you compile this as 32-bit (when it consumes 2 GiB it will throw OutOfMemoryException)
            if (occupiedPercentage > MaxMemoryOccupation)
                return true;

            return false;
        }

        /// <summary>
        /// Constructs lazy memory cache which caches object constructor and destructor.
        /// <para>Value loading is handled in a lazy way (JIT), and it is automatically unloaded from memory when a specified capacity is reached.</para>
        /// <para>The memory management is handled by LRU strategy.</para>
        /// </summary>
        /// <param name="isCapacityReached">Function that returns true if the cache limit is reached and the cache should start to unload items.</param>
        /// <param name="forceCollectionOnRemoval">
        /// <para>When set to true calls GC.Collect() when a value is unloaded due to capacity reach, but the CPU consumption can be high and accessing / adding elements can be temporary delay due to garbage collector.</para>
        /// <para>If false the GC.Collect() is not called which can lead to cache evict values more aggressively which could be avoided by setting this flag to true.
        /// Also the capacity will be probably breached but the memory overflow exception should not be thrown.
        /// </para>
        /// </param>
        private void Initialize(Func<bool> isCapacityReached, bool forceCollectionOnRemoval = true)
        {
            cache = new ConcurrentDictionary<TKey, ILazy<TValue>>();
            this.forceCollectionOnRemoval = forceCollectionOnRemoval;

            managmentStrategy = new LRUCache<TKey, LazyCacheItem<TKey, TValue>>(isCapacityReached);
            managmentStrategy.OnRemoveItem += managmentStrategy_OnRemoveItem;
        }

        float maxMemoryOccupation;
        /// <summary>
        /// Gets or sets the maximum memory occupation expressed as percentage [0..1] interval.
        /// </summary>
        public float MaxMemoryOccupation
        {
            get { return maxMemoryOccupation; }
            set { maxMemoryOccupation = Math.Min(1, Math.Max(0, value)); }
        }

        void managmentStrategy_OnRemoveItem(LRUCache<TKey, LazyCacheItem<TKey, TValue>> sender, KeyValuePair<TKey, LazyCacheItem<TKey, TValue>> item, bool userRequested)
        {
            item.Value.Unload();
        }

        /// <summary>
        /// Adds or updates the object value and the related cache statistics.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="constructor">Object constructor.</param>
        public void AddOrUpdate(TKey key, Func<TKey, TValue> constructor)
        {
            AddOrUpdate(key, constructor, obj => { });
        }

        /// <summary>
        /// Adds or updates the object value and the related cache statistics.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="constructor">Object constructor.</param>
        /// <param name="destructor">Object destructor.</param>
        public void AddOrUpdate(TKey key, Func<TKey, TValue> constructor, Action<TValue> destructor)
        {
            var item = new LazyCacheItem<TKey, TValue>(key, constructor, destructor);
            item.OnValueLoaded += item_OnValueLoaded;
            item.OnValueUnloaded += item_OnValueUnloaded;

            cache.GetOrAdd(key, item);
            managmentStrategy.Add(key, item);
        }

        /// <summary>
        /// Gets the lazy value from the cache.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Lazy value.</returns>
        public ILazy<TValue> this[TKey key]
        {
            get { return cache[key]; }
        }

        void item_OnValueUnloaded(object sender, EventArgs e)
        {
            //the item is automatically unloaded (LRU) or by the user
            if (forceCollectionOnRemoval)
                GC.Collect();
        }

        void item_OnValueLoaded(object sender, EventArgs e)
        {
            var lazyItem = sender as LazyCacheItem<TKey, TValue>;
            managmentStrategy.AddOrUpdate(lazyItem.Key, lazyItem); //update size information

            lock (syncObj) HardFaults++;
        }

        /// <summary>
        /// Unloads and removes the object from the cache.
        /// </summary>
        /// <param name="key">Object key.</param>
        public bool TryRemove(TKey key)
        {
            if (!cache.ContainsKey(key))
                return false;

            managmentStrategy.Remove(key);

            //if the key is the allocated object's key....
            ILazy<TValue> val;
            cache.TryGetValue(key, out val);

            //unload from memory
            (val as LazyCacheItem<TKey, TValue>).Dispose();

            //remove item
            cache.TryRemove(key, out val);

            return true;
        }

        /// <summary>
        /// Tries to get value under the specified key.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="value">Object key.</param>
        /// <returns>True if the specified key exist, false otherwise.</returns>
        public bool TryGetValue(TKey key, out ILazy<TValue> value)
        {
            ILazy<TValue> val;
            bool contains = cache.TryGetValue(key, out val);

            value = val;
            return contains;
        }

        /// <summary>
        /// Gets the enumerator for the cache.
        /// <para>By enumerating the collection objects are loaded only if the value property from <see cref="Accord.Extensions.Caching.ILazy{T}"/> is read.</para>
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<KeyValuePair<TKey, ILazy<TValue>>> GetEnumerator()
        {
            return cache.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator for the cache.
        /// <para>By enumerating the collection objects are loaded only if the value property from <see cref="Accord.Extensions.Caching.ILazy{T}"/> is read.</para>
        /// </summary>
        /// <returns>Enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// Gets the number of hard faults.
        /// (every time when an item is loaded the value is incremented by one)
        /// </summary>
        public int HardFaults
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of objects in the cache.
        /// </summary>
        public int Count
        {
            get { return cache.Count; }
        }
    }
}
