using System;
using SenseNet.Diagnostics;

namespace Gicogen
{
    internal class ConsoleTracer : ISnTracer
    {
        public void Write(string line)
        {
            var x = line.Split('\t');
            if (x[6] == "Start")
                Console.WriteLine("{0}   {1} starts", x[1].Substring(11), x[8]);
            else if (x[6] == "End")
                Console.WriteLine("{0}   {1} finished (duration: {2})", x[1].Substring(11), x[8], x[7]);
            else
                Console.WriteLine("{0}   {1}", x[1].Substring(11), x[8]);
        }

        public void Flush()
        {
            // do nothing
        }
    }
}
