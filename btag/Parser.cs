#nullable enable
using System;
using System.IO;
using System.Text;

namespace btag
{
    public class Parser
    {
        private FileStream? stream;
        private TagManager manager = new TagManager();
        public void OpenStream(string path)
        {
            stream = File.OpenRead(path);
        }

        public bool Parse()
        {
            if (stream == null)
            {
                return false;
            }
            byte[] oneByte = new byte[1] { 0x01 };
            while (true)
            {
                var result = stream.Read(oneByte, 0, 1);
                if (result == 0)
                {
                    break;
                }
                Tag? newTag = null;
                if (oneByte[0] == 0x01) {
                    stream.Read(oneByte, 0, 1);
                    byte[] title = new byte[oneByte[0]];
                    stream.Read(title, 0, oneByte[0]);
                    newTag = new Tag(Encoding.Default.GetString(title));
                    stream.Read(oneByte, 0, 1);
                }
                if (oneByte[0] == 0x02)
                {
                    manager.RemoveLast();
                }
                if (oneByte[0] == 0x03)
                {
                    stream.Read(oneByte, 0, 1);
                    byte[] value = new byte[oneByte[0]];
                    stream.Read(value, 0, oneByte[0]);
                    if (newTag == null)
                    {
                        return false;
                    }
                    newTag.value = new byte[oneByte[0]];
                    newTag.value = value;
                }
                if (newTag == null)
                {
                    continue;
                }
                manager.AddChildToLast(newTag);
            }
            return true;
        }
    }
}
