using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServoMonitoring_with_Control
{
    public class CustomQueue                        // using for storing overloaded values from arduino
    {
        Queue bigQ;                  // big queue - based on default queue - for alarm
        Queue smallQ;                  // small queue - based on default queue - for charting
        uint big_size;                  // but with concrete size (big)
        uint small_size;                  // but with concrete size (small)
        float big_average;                  // keep current average of big queue values
        float small_average;            // keep current average of small queue values

        //Constructors:
        public CustomQueue(uint _bsize, uint _ssize)
        {
            big_size = _bsize;
            small_size = _ssize;
            bigQ = new Queue();
            smallQ = new Queue();
        }

        public void Add(int val)            // methods to adding data to queue
        {
            int big_temp = 0;
            int small_temp = 0;
            while (bigQ.Count>big_size)
            {
                bigQ.Dequeue();
            }
            while (smallQ.Count > small_size)
            {
                smallQ.Dequeue();
            }
            if (bigQ.Count == big_size) big_temp = (int)bigQ.Dequeue();
            if (smallQ.Count == small_size) small_temp = (int)smallQ.Dequeue();
            bigQ.Enqueue(val);
            smallQ.Enqueue(val);
            big_average = big_average + (val - big_temp) / bigQ.Count;
            small_average = small_average + (val - small_temp) / smallQ.Count;
        }

        public float BigAverage()                 // calculate averange based on all values in queue
        {
            return big_average;
        }

        public float SmallAverage()                 // calculate averange based on all values in queue
        {
            return small_average;
        }

        public Queue SyncQ()                        // method for copying queue for threads
        {
            return Queue.Synchronized(bigQ);
        }


    }
}
