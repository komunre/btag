using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace btag
{
    public class Writer
    {
        int layer = 1;
        FileStream stream;
        public void OpenStream(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            stream = File.OpenWrite(path);
        }

        public void WriteAllList(List<Tag> tags)
        {
            foreach(var tag in tags)
            {
                WriteAll(tag);
            }
        }
        public void WriteAll(Tag tag)
        {  
            WriteTag(tag);
            if (tag.Childes.Count > 0)
            {
                layer++;
            }
            foreach (var child in tag.Childes)
            {
                WriteAll(child);
            }
        }

        private void WriteTag(Tag tag)
        {
            var length = tag.title.Length;
            stream.Write(new byte[] { 0x01, (byte)length });
            stream.Write(Encoding.Default.GetBytes(tag.title));
            if (tag.value != null)
            {
                stream.Write(new byte[] { 0x03 });
                stream.Write(BitConverter.GetBytes((Int16)tag.value.Length));
                stream.Write(tag.value);
            }
            stream.Write(new byte[] { 0x00 });
            if (tag.Childes.Count == 0)
            {
                if (tag.parent.Childes.LastIndexOf(tag) != tag.parent.Childes.Count - 1)
                {
                    stream.Write(new byte[] { 0x02 });
                    layer--;
                    tag = tag.parent;
                }
                else
                {
                    stream.Write(new byte[] { 0x02, 0x02 });
                    layer -= 2;
                    tag = tag.parent.parent;
                }
            }
        }

        

        public void CloseStream()
        {
            stream.Close();
            layer = 1;
        }
    }
}
