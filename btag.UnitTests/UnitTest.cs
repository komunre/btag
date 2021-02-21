using Microsoft.VisualStudio.TestTools.UnitTesting;
using btag;
using System;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Text;
using System.IO;

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

                tag.AddChild(testTag);
            }
        }

        [TestMethod]
        public void SpeedTest1()
        {

            var main = new Tag("main");
            if (!File.Exists("speedtest.btag"))
            {
                TagGeneration(ref main);
            }
            else
            {
                var parser = new Parser();
                parser.OpenStream("speedtest.btag");
                parser.Parse();
                main = parser.FindTagLayerRoot("main");
                parser.CloseStream();
            }

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

        [TestMethod]
        public void SPWManagerTest()
        {
            string date;
            int volume;
            PWManager manager = new PWManager("speedtest.btag");
            manager.FindOnLayer("main");
            manager.FindOnLayer("test");
            manager.FindOnLayer("date");
            date = manager.GetValueStr();
            manager.GoUpper();
            manager.FindOnLayer("volume");
            volume = manager.GetValueInt();
            Assert.IsTrue(date == "30/03/2017" && volume == 104000);
        }

        [TestMethod]
        public void SPWManagerWriteTest()
        {
            PWManager manager = new PWManager("speedtest.btag");
            manager.FindOnLayer("main");
            manager.FindOnLayer("test");
            manager.FindOnLayer("volume");
            var volume = 100;
            manager.SetValue(volume);
            Assert.IsTrue(manager.GetValueInt() == 100);
        }
    }
}
