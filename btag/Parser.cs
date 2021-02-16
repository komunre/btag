using System;
using System.IO;
using System.Text;

namespace btag
{
    public class Parser
    {
        private FileStream stream;
        private TagManager manager = new TagManager();
        public void OpenStream(string path)
        {
            stream = File.OpenRead(path);
        }

        public bool Parse()
        {
            byte[] oneByte = new byte[1] { 0x01 };
            while (true)
            {
                var result = stream.Read(oneByte, 0, 1);
                if (result == 0)
                {
                    break;
                }
                switch (oneByte[0])
                {
                    case 0x01:
                        stream.Read(oneByte, 0, 1);
                        byte[] title = new byte[oneByte[0]];
                        stream.Read(title, 0, oneByte[0]);
                        manager.AddChildToLast(new Tag(Encoding.Default.GetString(title)));
                        break;

                    case 0x02:
                        manager.RemoveLast();
                        break;
                }
            }
            return true;
        }
    }
}
