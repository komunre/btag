using Microsoft.VisualStudio.TestTools.UnitTesting;
using btag;
using System;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace btag.UnitTests
{
    [TestClass]
    public class UnitTest
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

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
            for (int x = 0; x < 100000; x++)
            {
                Tag testTag = new Tag("test" + x);
                Tag date = new Tag("date");
                date.value = Converter.GetOptimized(125825285);
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
            int date;
            int volume;
            PWManager manager = new PWManager("speedtest.btag");
            manager.FindOnLayer("main");
            manager.FindOnLayer("test0");
            manager.FindOnLayer("date");
            date = manager.GetValueInt();
            manager.GoUpper();
            manager.FindOnLayer("volume");
            volume = manager.GetValueInt();
            Assert.IsTrue(date == 125825285 && volume == 104000);
        }

        [TestMethod]
        public void SPWManagerWriteTest()
        {
            PWManager manager = new PWManager("speedtest.btag");
            manager.FindOnLayer("main");
            manager.FindOnLayer("test0");
            manager.FindOnLayer("volume");
            var volume = 100;
            manager.SetValue(volume);
            Assert.IsTrue(manager.GetValueInt() == 100);
        }

        [TestMethod]
        public void BtagVsJson()
        {
            string jsonStr = "";
            Dictionary<string, Dictionary<string, int>> tags = new Dictionary<string, Dictionary<string, int>>();
            for (int i = 0; i != 100000; i++)
            {
                Dictionary<string, int> things = new Dictionary<string, int>();
                int date = 125825285;
                var open = 3005;
                var high = 3010;
                var low = 2936;
                var close = 2936;
                var volume = 104000;
                things.Add("date", date);
                things.Add("open", open);
                things.Add("high", high);
                things.Add("low", low);
                things.Add("close", close);
                things.Add("volume", volume);
                tags.Add("test" + i, things);
            }
            jsonStr = JsonSerializer.Serialize(tags);
            File.WriteAllText("hell.json", jsonStr);
            var jsonStopwatch = new Stopwatch();
            jsonStopwatch.Start();
            var uh = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(jsonStr);
            jsonStopwatch.Stop();

            var btagStopWatch = new Stopwatch();
            btagStopWatch.Start();
            var parser = new Parser();
            parser.OpenStream("speedtest.btag");
            parser.Parse();
            parser.CloseStream();
            btagStopWatch.Stop();

            TestContext.Write("btag: " + btagStopWatch.ElapsedMilliseconds + "\njson: " + jsonStopwatch.ElapsedMilliseconds);
            Assert.IsTrue(btagStopWatch.ElapsedMilliseconds <= jsonStopwatch.ElapsedMilliseconds);
        }
    }
}
