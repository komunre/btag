using System;
using System.Text;
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

            var superTag = parser.FindTag("super");
            Console.Out.WriteLine(superTag.value[0]);

            Writer writer = new Writer();
            writer.OpenStream("output.btag");
            writer.WriteAllList(parser.GetManagerRoot().Childes);
            writer.CloseStream();

            var main = new Tag("main");
            var first = new Tag("first");
            var second = new Tag("second");
            main.AddChild(first);
            first.AddChild(second);
            writer.OpenStream("output2.btag");
            writer.WriteAll(main);
            writer.CloseStream();
        }
    }
}
