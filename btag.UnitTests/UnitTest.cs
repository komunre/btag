using Microsoft.VisualStudio.TestTools.UnitTesting;
using btag;
using System;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Text;

namespace btag.UnitTests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void WriteAndParseEquality()
        {
            var main = new Tag("main");
            main.AddChild(new Tag("first"));
            var second = new Tag("second");
            second.AddChild(new Tag("testingDeep"));
            main.AddChild(second);
            main.AddChild(new Tag("third"));

            var writer = new Writer();
            writer.OpenStream("equality.btag");
            writer.WriteAll(main);
            writer.CloseStream();

            var parser = new Parser();
            parser.OpenStream("equality.btag");
            parser.Parse();
            Assert.IsTrue(main.ChildrenEquals(parser.FindTagLayerRoot("main")));
        }

        [Ignore]
        public void TagGeneration(ref Tag tag)
        {
            var manager = new TagManager();
            manager.AddChildToLast(tag);
            for (int x = 0; x < 1000; x++)
            {
                Tag testTag = new Tag("test");
                Tag date = new Tag("date");
                date.value = Encoding.Default.GetBytes("30/03/2017");
                Tag open = new Tag("open");
                open.value = Converter.GetOptimized(3005);
                Tag high = new Tag("high");
                high.value = Converter.GetOptimized(3010);
                Tag low = new Tag("low");
                low.value = Converter.GetOptimized(2936);
                Tag close = new Tag("close");
                close.value = Converter.GetOptimized(2936);
                Tag volume = new Tag("volume");
                volume.value = Converter.GetOptimized(104000);
                testTag.AddChild(date);
                testTag.AddChild(open);
                testTag.AddChild(high);
                testTag.AddChild(low);
                testTag.AddChild(close);
                testTag.AddChild(volume);
                testTag.active = false;

                manager.AddChildToLast(testTag);
            }
        }

        [TestMethod]
        public void SpeedTest1()
        {
            
            var main = new Tag("main");
            TagGeneration(ref main);

            var writer = new Writer();
            writer.OpenStream("speedtest.btag");
            writer.WriteAll(main);
            writer.CloseStream();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SpeedTest2()
        {
            var parser = new Parser();
            parser.OpenStream("speedtest.btag");
            Assert.IsTrue(parser.Parse());
        }
    }
}
