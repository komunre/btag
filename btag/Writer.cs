using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace btag
{
    public class Writer
    {
        int minus = 0;
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
                minus++;
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
                stream.Write(tag.value);
            }
            stream.Write(new byte[] { 0x00 });
            if (tag.Childes.Count == 0)
            {
                for (int i = 0; i < minus; i++)
                {
                    tag = tag.parent;
                }
                while (tag.Childes.Count - minus > 0 && minus > 0)
                {
                    stream.Write(new byte[] { 0x02 });
                    minus++;
                }
            }
        }

        public void CloseStream()
        {
            stream.Close();
        }
    }
}
