using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServoMonitoring_with_Control
{
    public class CustomQueue                        // using for storing overloaded values from arduino
    {
        Queue myQ;                  // based on default queue
        uint size;                  // but with concrete size
        
        //Constructors:
        public CustomQueue(uint _size)
        {
            size = _size;
            myQ = new Queue();
        }

        public void Add(int val)            // methods to adding data to queue
        {
            while (myQ.Count>size)
            {
                myQ.Dequeue();
            }
            if (myQ.Count == size) myQ.Dequeue();
            myQ.Enqueue(val);
        }

        public float Averange()                 // calculate averange based on all values in queue
        {
            int sum = 0;
            foreach (int obj in myQ)
                sum += obj;
            return sum / size;
        }

        public Queue SyncQ()                        // method for copying queue for threads
        {
            return Queue.Synchronized(myQ);
        }


    }
}
