using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceCounters
{
    class Program
    {
        public static PerformanceCounter[] counters;
        public static PerformanceCounter all;
        public static PerformanceCounter persec;
        static void Main(string[] args)
        {
            //  Create two performance Counters
            //  One is TOTAL...the other is Per/Second
            var countercreate 
                = new CounterCreationData("Orders/Sec", "none", PerformanceCounterType.RateOfCountsPerSecond32);
            var counterTotal
                = new CounterCreationData("TotalOrders", "none", PerformanceCounterType.NumberOfItems64);
            if (!PerformanceCounterCategory.Exists("PerfCounter Samples"))
            {
                //  Make sure the performance category exists on this machine
                //  also creates the counters underneath
                var ctrs 
                    = PerformanceCounterCategory.Create("PerfCounter Samples", 
                                    "None", 
                                    PerformanceCounterCategoryType.SingleInstance, 
                                new CounterCreationDataCollection(new CounterCreationData[] { countercreate, counterTotal }));

                //  Now go get a reference to each of these counters for I can move them
                all = new PerformanceCounter("PerfCounter Samples", "TotalOrders", false);
                persec = new PerformanceCounter("PerfCounter Samples", "Orders/Sec", false);

                //  Create a new thread that will simulate
                //  Random activity
                Thread worker = new Thread(Simulator);
                worker.Start();

                //  Open the performance monitor
                System.Diagnostics.Process.Start("perfmon.exe");
                Console.WriteLine("======================================================================");
                Console.WriteLine("=                 Performance Monitor should be open                 =");
                Console.WriteLine("=        Click on the Add button and find \"PerfCounter Samples\"      =");
                Console.WriteLine("----------------------------------------------------------------------");
                Console.WriteLine("=                          - Add and watch                           =");
                Console.WriteLine("======================================================================");
                Console.WriteLine("===========               Hit Enter to stop                ===========");
                Console.WriteLine("======================================================================");
                Console.ReadLine();

                //  If we got here, user pressed enter...kill thread
                worker.Abort();

                //  Remove the performance counter (cleanup)
                PerformanceCounterCategory.Delete("PerfCounter Samples");
            }
            else
                //  program might have died.  Didn't expect to see this counter
                //  Delete it and restart
                PerformanceCounterCategory.Delete("PerfCounter Samples");
        }

        private static void Simulator(object obj )
        {
            while (true)
            {
                //  increment each counter
                all.Increment();
                persec.Increment();

                //  Generate a random number between 0 and 50.
                var rand = new Random((int)DateTime.Now.Ticks);
                var wait = rand.Next(0, 50);

                //  Pause that many milliseconds
                Thread.Sleep(wait);
            }

        }

    }
}
