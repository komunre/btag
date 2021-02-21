using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace btag
{
    public class Writer
    {
        BufferedStream stream;
        public void OpenStream(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            stream = new BufferedStream(File.Open(path, FileMode.OpenOrCreate));
        }

        /// <summary>
        /// Write sseveral main tags to file
        /// </summary>
        /// <param name="tags">List with main tags</param>
        public void WriteAllList(List<Tag> tags)
        {
            foreach(var tag in tags)
            {
                WriteAll(tag);
            }
        }

        /// <summary>
        /// Write main tag to file
        /// </summary>
        /// <param name="tag">Main (root) tag</param>
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
            var length = tag.title.Length;
            stream.Write(new byte[] { 0x01, (byte)length });
            stream.Write(Encoding.Default.GetBytes(tag.title));
            if (tag.value != null)
            {
                stream.Write(new byte[] { 0x03 });
                stream.Write(BitConverter.GetBytes((UInt16)tag.value.Length));
                stream.Write(tag.value);
            }
            stream.Write(new byte[] { 0x00 });
            if (tag.Childes.Count == 0)
            {
                var parent = tag;
                while (parent.parent != null && parent.parent.Childes[parent.parent.Childes.Count - 1] == parent)
                {
                    stream.Write(new byte[] { 0x02 });
                    parent = parent.parent;
                }
                stream.Write(new byte[]{ 0x02 });
            }

            stream.Flush();
        }

        

        public void CloseStream()
        {
            stream.Close();
        }
    }
}
