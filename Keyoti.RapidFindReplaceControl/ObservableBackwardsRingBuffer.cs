/*  
    RapidFindReplace WPF - a find/replace control for WPF applications
    Copyright (C) 2014-2015 Keyoti Inc.

    
    This program is licensed as either free software or commercial use: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, version 2 of the License.
    Alternatively you may purchase
    a commercial license at http://keyoti.com/products/rapidfindreplace/wpf/index.html

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/




using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Keyoti.RapidFindReplace.WPF
{

    /// <summary>
    /// A generic ring-buffer, that presents its item collection as last-in at the index 0 (start), and older items at subsequent indexes (end).
    /// </summary>
    public class ObservableBackwardsRingBuffer<T> : INotifyCollectionChanged, IEnumerable<T>
    {
        /// <summary>
        /// Event when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        // [0] <--- addPtr
        // [1] <--- start
        // [2] 
        // [3]
        // size = 4
        // addPtr = start + count % size
        // list item at pos x = [start + x % size]
        bool isForwards = false;
        T[] store;
        int size;
        int start = 0;
        int count = 0;
        //int[] runLengths;
        // int[] gobbled;

        /// <summary>
        /// Number of items in the collection
        /// </summary>
        public int Count { get { return count; } }
        /// <summary>
        /// Capacity
        /// </summary>
        public int Capacity { get { return size; } }

        /// <summary>
        /// New
        /// </summary>
        /// <param name="capacity"></param>
        public ObservableBackwardsRingBuffer(int capacity)
        {
            store = new T[capacity];

            this.size = capacity;
        }
        /// <summary>
        /// Adds an item to the ring-buffer
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(T item)
        {
            if (size == 0) return;
            int insertPosition = (start + count) % size;
            if (count >= size)
            {
                T gone = this[isForwards ? 0 : count - 1];
                start = (start + 1) % size;
                //need to notify element removed
                if (CollectionChanged != null)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, gone, isForwards ? 0 : count - 1));
            }
            store[insertPosition] = item;
            

            //runLengths[(start + count) % size] = run.Text.Length;
            if (count < size)
            {
                count++;
                if (CollectionChanged != null)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, isForwards ? count - 1 : 0));
            }
            else
            {
                if (CollectionChanged != null)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, isForwards ? count - 1 : 0));


                
                
            }

            
        }

        

        /// <summary>
        /// Removes item at index 0.
        /// </summary>
        /// <returns></returns>
        public T RemoveStartItem()
        {
            T bottom = store[start];
            start = (start + 1) % size;
            count--;

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, bottom, 0));

            return bottom;
        }

        internal T this[int ptr]
        {
            get { 
                if(isForwards)
                    return store[(start + ptr) % size]; 
                else
                    return store[(start + ((count-1)-ptr)) % size]; 
            }
            //set { store[(start + ptr) % size] = value; }
        }

        /// <summary>
        /// Gets an enumerator
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            int iteratorPos = 0;
            while(iteratorPos<Count)
                yield return this[iteratorPos++];
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Resizes the ring buffer.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        public void Resize(int newSize)
        {
            while (Count > newSize)
                RemoveStartItem();


            T[] newStore = new T[newSize];
            int lim = Math.Min(newSize, size);
            for (int i = 0; i < lim; i++)
            {
                newStore[i] = store[(start+i)%size];
            }

            store = newStore;

            size = newSize;
            start = 0;
            count = Math.Min(count, newSize);
        }
    }
    /*
    /// <summary>
    /// Bindable queue.
    /// </summary>
    public class ObservableQueue<T> : INotifyCollectionChanged, IEnumerable<T>
    {
        /// <summary>
        /// Collection change event.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private readonly Queue<T> queue = new Queue<T>();

        /// <summary>
        /// Enqueue as standard Queue
        /// </summary>
        public void Enqueue(T item)
        {

            queue.Enqueue(item);

            if (CollectionChanged != null)
                CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, item));
        }

        /// <summary>
        /// Dequeue as standard Queue
        /// </summary>
        public T Dequeue()
        {
           // queue.Reverse();
            var item = queue.Dequeue();
           // queue.Reverse();
            if (CollectionChanged != null)
                CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, item, 0));
            return item;
        }

        /// <summary>
        /// Number of items in collection
        /// </summary>
        public virtual int Count { get { return queue.Count; } }

        /// <summary>
        /// Gets an enumerator
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            
            return queue.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
     * */
}
