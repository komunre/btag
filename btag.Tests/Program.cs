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
            var subSecond = new Tag("subsecond");
            var third = new Tag("third");
            var subSecond2 = new Tag("subsecond2");
            var deeper = new Tag("deeper");
            second.value = new byte[]{ 0x62, 0x69, 0x67, 0x20, 0x74, 0x65, 0x73, 0x74, 0x20, 0x28, 0x62, 0x69, 0x67, 0x29 };
            main.AddChild(first);
            main.AddChild(second);
            second.AddChild(subSecond);
            main.AddChild(third);
            second.AddChild(subSecond2);
            subSecond.AddChild(deeper);

            writer.OpenStream("output2.btag");
            writer.WriteAll(main);
            writer.CloseStream();
            parser.Clear();
            parser.OpenStream("output2.btag");
            var success2 = parser.Parse();
            if (!success2)
            {
                throw new Exception("No success.");
            }
            var mainLayer = parser.FindTagLayerRoot("main");
            var secondLayer = parser.FindTagLayer(mainLayer, "second");
            Console.WriteLine(Encoding.Default.GetString(secondLayer.value));
        }
    }
}
