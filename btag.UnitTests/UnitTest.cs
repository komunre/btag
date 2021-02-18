using Microsoft.VisualStudio.TestTools.UnitTesting;
using btag;
using System;

namespace btag.UnitTests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void SpeedTest()
        {
            var manager = new TagManager();
            var main = new Tag("main");
            manager.AddChildToLast(main);
            for (int x = 0; x < 2000; x++)
            {
                Tag testTag = new Tag("test" + x);
                var random = new Random();
                var randomNum = random.Next(3);
                if (randomNum == 1)
                {
                    Tag subTag = new Tag("subTag");
                    manager.AddChildToLast(subTag);
                    x++;
                }
                else if (randomNum == 2)
                {
                    Tag subTag = new Tag("subTagClosed");
                    subTag.active = false;
                    manager.AddChildToLast(subTag);
                    x++;
                }
                manager.AddChildToLast(testTag);
            }
            var writer = new Writer();
            writer.OpenStream("speedtest.btag");
            writer.WriteAll(main);
            writer.CloseStream();

            var parser = new Parser();
            parser.OpenStream("speedtest.btag");
            Assert.IsTrue(parser.Parse());
        }
    }
}
