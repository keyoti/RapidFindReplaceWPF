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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace Keyoti.RapidFindReplace.WPF
{
    /// <summary>
    /// Wrapper for a Run that is in a Queue.
    /// </summary>
    public class QueuedRun
    {
        /// <summary>
        /// New
        /// </summary>
        /// <param name="run">Run to wrap before putting it in a Queue</param>
        public QueuedRun(Run run)
        {
            this.run = run;
            this.length = run.Text.Length;
        }
        Run run;
        int length;
        int amountGobbled;
        /// <summary>
        /// Registers a hit against the run.
        /// </summary>
        /// <param name="lengthOfCharsUsed"></param>
        public void RegisterHit(int lengthOfCharsUsed)
        {
            amountGobbled = lengthOfCharsUsed;
        }
        /// <summary>
        /// Text that has not already been searched.  Text that has been searched will appear as newline chars.
        /// </summary>
        public string HitAvailableText
        {
            get {
                if (amountGobbled == 0) return run.Text;
                else
                    return run.Text.Substring(amountGobbled).PadLeft(length, '\n'); 
            }
        }
        /// <summary>
        /// Text to search, whether it has been searched already or not.
        /// </summary>
        public string Text
        {
            get
            {

                return run.Text;
            }
        }
        /// <summary>
        /// The Run to search.
        /// </summary>
        public Run Run { get { return run; } }

        /// <summary>
        /// The length of the text.
        /// </summary>
        public int Length { get { return length; } }
    }

    //like Queue<> but allows random access to items
    /// <summary>
    /// Queue of Run objects to search.
    /// </summary>
    public class RunQueue
    {
        // [0] <--- addPtr
        // [1] <--- start
        // [2] 
        // [3]
        // size = 4
        // addPtr = start + count % size
        // list item at pos x = [start + x % size]

        QueuedRun[] store;
        int size;
        int start = 0;
        int count = 0;
        //int[] runLengths;
       // int[] gobbled;
        /// <summary>
        /// Number of items in the queue.
        /// </summary>
        public int Count { get { return count; } }
        /// <summary>
        /// Capacity of the queue.
        /// </summary>
        public int Size { get { return size; } }

        /// <summary>
        /// New.
        /// </summary>
        /// <param name="size">Capacity of the queue.</param>
        public RunQueue(int size)
        {
            store = new QueuedRun[size];
            //runLengths = new int[size];
            //gobbled = new int[size];
            this.size = size;
        }

        /// <summary>
        /// Add a Run to the queue.
        /// </summary>
        /// <param name="run"></param>
        public void Enqueue(Run run)
        {
            store[(start + count) % size] = new QueuedRun(run);
            //runLengths[(start + count) % size] = run.Text.Length;
            if (count < size)
                count++;
            else
                start = (start+1)%size;
        }

        /// <summary>
        /// Peek at an item in the queue.
        /// </summary>
        public QueuedRun this[int ptr]
        {
            get { return store[(start + ptr) % size]; }
            set { store[(start + ptr) % size] = value; }
        }

        /// <summary>
        /// Peek a the character length of any Run in the queue.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public int GetRunLength(int ptr)
        {
            return this[ptr].Length;
        }
        /// <summary>
        /// Removes a Run in the queue.
        /// </summary>
        public Run RemoveRunHoldingPosition(int ptr, out int ptrOffset, out int remainingChars)
        {
            int consumedLength = 0;
            for (int i = 0; i <= Count; i++)
            {
                consumedLength += GetRunLength(i);
                if (consumedLength > ptr)
                {
                    ptrOffset = ptr - (consumedLength - GetRunLength(i));
                    remainingChars = GetRunLength(i) - ptrOffset;
                    //we have to remove, so shuffle down
                    return RemoveAt(i);

                }
            }
            ptrOffset = -1;
            remainingChars = 0;
            return null;
        }

        /// <summary>
        /// Gets the Run for a hit (measured relative to the queue start).
        /// </summary>
        /// <param name="ptr">Hit start relative to queue start</param>
        /// <param name="hitLength">Hit length</param>
        /// <param name="ptrOffset">Hit start relative to Run start</param>
        /// <param name="gobbledChars">Number of characters hit in the Run, if a hit spans multiple Runs then <c>gobbledChars</c>&lt;<c>hitLength</c>.</param>
        /// <returns></returns>
        public Run HitPosition(int ptr, int hitLength, out int ptrOffset, out int gobbledChars)
        {
            //List<Run> runs = new List<Run>(2);
            int consumedLength = 0;
            for (int i = 0; i <= Count; i++)
            {
                consumedLength += GetRunLength(i);
                if (consumedLength > ptr)
                {
                    ptrOffset = ptr - (consumedLength - GetRunLength(i));
                    gobbledChars = GetRunLength(i) - ptrOffset;
                    if (hitLength < gobbledChars)
                        this[i].RegisterHit(ptrOffset + hitLength);
                    else
                    {
                        //only a partial match, will need to get remaining runs
                        this[i].RegisterHit(ptrOffset + gobbledChars);

                    }
                    return this[i].Run;

                }
            }
            ptrOffset = -1;
            gobbledChars = 0;
            return null;
        }

        private Run RemoveAt(int i)
        {
            Run ret = this[i].Run;
            int removedLength = GetRunLength(i);
            //add removedLength to next run, so our indices are correct
           // runLengths[(start + i+1) % size] +=  removedLength;
            for (int p = i; p < Count-1; p++)
            {
                this[p] = this[p + 1];
                //adjust subsequent run lengths to include this now missing run
                //[(start + p) % size] = runLengths[(start + p+1) % size];
            }
            count--;
            return ret;
        }

        /// <summary>
        /// Clones this.
        /// </summary>
        public RunQueue Clone()
        {
            RunQueue clone = new RunQueue(Size);
            for (int i = 0; i < store.Length; i++)
            {
                clone.store[i] = store[i];
               // clone.runLengths[i] = runLengths[i];
                clone.start = start;
                clone.count = count;

            }
            return clone;
        }


        /// <summary>
        /// Clear the queue.
        /// </summary>
        public void Clear()
        {
            start = 0;
            count = 0;
        }

        /// <summary>
        /// Remove bottom queued run.
        /// </summary>
        /// <returns>The botttom queued run.</returns>
        public QueuedRun Dequeue()
        {
            QueuedRun bottom = store[start];
            start=(start + 1) % size;
            count--;
            return bottom;
        }
    }
}
