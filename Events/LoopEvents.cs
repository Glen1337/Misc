using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Misc
{
    namespace Examples
    {
        // declare delegate
        //public delegate void LoopHitEvent(object sender, loopEventArgs e, int threadid);

        // Custom event args
        public class loopEventArgs : EventArgs
        {
            public String Message;

            public loopEventArgs(String inmessage)
            {
                this.Message = inmessage;
            }
        }

        class _Main
        {
            static void Main(string[] args)
            {
                Looper myLooper = new Looper();

                // Have two different callbacks/event handlers subscribe to LoopHit event
                myLooper.OnLoopHit += Looper.Looper_OnLoopHit;
                myLooper.OnLoopHit += Looper.Looper_SecondSubscriber;

                // second way
                //myLooper.listen();

                // Used named parameters
                myLooper.loop(stopOn: 2, range: 10);

                Console.ReadKey();
            }
        }

        public class Looper
        {
            // declare event of type LoopHitEvent delegate
            public event LoopHitEvent OnLoopHit;
            public delegate void LoopHitEvent(object sender, loopEventArgs e, int threadid);

            public void loop(int stopOn = 6, int range = 10)
            {
                // Run loop in multiple threads
                Parallel.For(0, range, i =>
                {
                    Console.WriteLine("Iteration " + i + " on thread: " + Thread.CurrentThread.ManagedThreadId);
                    // Make sure event has at least one suscriber, then raise event (invoke delegate) when # is hit
                    if ((i == stopOn) && (OnLoopHit != null))
                    {
                        // Invoke event two different ways
                        //OnLoopHit(this, new loopEventArgs("Loop hit " + i), Thread.CurrentThread.ManagedThreadId);
                        OnLoopHit.Invoke(this, new loopEventArgs("Loop hit " + i), Thread.CurrentThread.ManagedThreadId);
                    }
                });

            }

            public void listen()
            {
                OnLoopHit += Looper_OnLoopHit;
            }

            // Callback
            public static void Looper_OnLoopHit(object sender, loopEventArgs e, int threadid)
            {
                Console.WriteLine("HIT: " + e.Message + " on thread " + threadid.ToString());
            }

            // callback
            public static void Looper_SecondSubscriber(object sender, loopEventArgs e, int i)
            {
                Console.WriteLine("2nd callback");
            }
        }

    }
}
