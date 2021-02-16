using System;
using btag;

namespace btag.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser();
            parser.OpenStream("example");
            var success = parser.Parse();
            if (!success)
            {
                throw new Exception("No success.");
            }
        }
    }
}
