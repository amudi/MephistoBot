using System;
using System.Collections.Generic;
using System.Drawing;

namespace mephisto
{
    public class HPNodeMinHeap
    {
        // change this field type accordingly
        private HPNode[] heap;
        private int size;

        // create heap with specified capacity
        public HPNodeMinHeap(int initialCapacity)
        {
            heap = new HPNode[initialCapacity + 1];
            size = 0;
        }

        // create default heap with capacity 10
        public HPNodeMinHeap()
            : this(10)
        {}

        // return true iff heap is empty
        public bool IsEmpty
        {
            get { return (this.size == 0); }
        }

        // return no. of elements in heap
        public int Size
        {
            get { return this.size; }
        }

        // clear elements
        public void Clear()
        {
            for (int i = 1; i <= size; i++)
                heap[i] = null;

            size = 0;
        }

        // return min element (root)
        // null if heap is empty
        public HPNode GetMin()
        {
            return (size == 0 ? null : heap[1]);
        }

        // put element to heap
        public void Put(HPNode n)
        {
            // find the appropriate place for element
            // start as new leaf and moves up accordingly
            int currentNode = ++this.size;
            while (currentNode != 1		// not at root
                && heap[currentNode / 2].cost > n.cost // current node is > new node
                )
            {
                heap[currentNode] = heap[currentNode / 2];	// move current node down (cos its greater)
                currentNode /= 2;	// move to the parent of current
            }

            // we found a place
            heap[currentNode] = n;
        }

        // remove the root (min element)
        public HPNode RemoveMin()
        {
            // if empty return null
            if (this.size == 0) return null;

            HPNode minElement = heap[1];	// save min elem

            // --- re-heapify
            HPNode lastElement = heap[size--];

            // find place for lastElement starting from root
            int currentNode = 1,	// start at root
                child = 2;			// child of currentNode
            while (child <= size)
            {
                // heap[child] should be < child of currentNode
                if (child < size &&
                    heap[child].cost > heap[child + 1].cost
                    )
                    child++;

                // can we put lastElem in heap[currentNode]?
                if (
                    lastElement.cost <= heap[child].cost
                    )
                    break;	//yes

                // no
                heap[currentNode] = heap[child];	// move child up
                currentNode = child;				// move currentNode down
                child *= 2;
            }
            heap[currentNode] = lastElement;

            return minElement;
        }
    }
}
