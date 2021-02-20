using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace btag
{
    public class Writer
    {
        FileStream stream;
        public void OpenStream(string path)
        {
            stream = File.Open(path, FileMode.Truncate);
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
            foreach (var child in tag.Childes)
            {
                WriteAll(child);
            }
        }

        private void WriteTag(Tag tag)
        {
            List<byte> byteList = new List<byte>();
            var length = tag.title.Length;
            byteList.AddRange(new byte[] { 0x01, (byte)length });
            byteList.AddRange(Encoding.Default.GetBytes(tag.title));
            if (tag.value != null)
            {
                byteList.Add(0x03);
                byteList.AddRange(BitConverter.GetBytes((Int16)tag.value.Length));
                byteList.AddRange(tag.value);
            }
            byteList.Add(0x00);
            if (tag.Childes.Count == 0)
            {
                var parent = tag;
                while (parent.parent != null && parent.parent.Childes.LastIndexOf(parent) == parent.parent.Childes.Count - 1)
                {
                    byteList.Add(0x02);
                    parent = parent.parent;
                }
                byteList.Add(0x02);
            }

            stream.Write(byteList.ToArray());
        }

        

        public void CloseStream()
        {
            stream.Close();
        }
    }
}
