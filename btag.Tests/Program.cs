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
            parser.Parse();
            
        }
    }
}
