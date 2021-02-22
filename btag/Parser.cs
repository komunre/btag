#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace btag
{
    public class Parser
    {
        private FileStream? stream;
        private TagManager manager = new TagManager();
        private byte[] bytes = new byte[2];

        public void OpenStream(string path)
        {
            stream = File.OpenRead(path);
        }

        private void ParseTag(byte[] title)
        {
            Tag newTag = new Tag(Encoding.Default.GetString(title));
            stream.Read(bytes, 0, 1);
            if (bytes[0] == 0x03)
            {
                stream.Read(bytes, 0, 2);
                var size = BitConverter.ToUInt16(bytes);
                newTag.value = new byte[size];
                stream.Read(newTag.value, 0, size);
            }
            manager.AddChildToLast(newTag);
        }

        /// <summary>
        /// Parse entire file. Use OpenStream before parsing
        /// </summary>
        /// <returns></returns>
        public bool Parse()
        {
            while (stream.Read(bytes, 0, 1) != 0)
            {
                if (bytes[0] == 0x01)
                {
                    stream.Read(bytes, 0, 1);
                    byte[] title = new byte[bytes[0]];
                    stream.Read(title, 0, bytes[0]);
                    ParseTag(title);
                }
                else
                {
                    manager.DeactivateLast();
                }
            }
            return true;
        }

        public Tag? FindTag(string title)
        {
            return manager.FindTagFromRoot(title);
        }

        /// <summary>
        /// Find tag from root
        /// </summary>
        /// <param name="title">title of tag</param>
        /// <returns></returns>
        public Tag? FindTagLayerRoot(string title)
        {
            return manager.FindTagLayer(manager.GetRoot(), title);
        }

        /// <summary>
        /// Find child of tag
        /// </summary>
        /// <param name="layer">parent</param>
        /// <param name="title">child</param>
        /// <returns></returns>
        public Tag? FindTagLayer(Tag layer, string title)
        {
            return manager.FindTagLayer(layer, title);
        }

        public Tag GetManagerRoot()
        {
            return manager.GetRoot();
        }

        public bool TryFindTagLayer(Tag layer, string title, out Tag? tag)
        {
            return manager.TryFindTagLayer(layer, title, out tag);
        }

        private void ResetStream()
        {
            if (stream == null)
            {
                return;
            }
            stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Clear entire parser
        /// </summary>
        public void Clear()
        {
            if (stream == null)
            {
                return;
            }
            manager.Clear();
            ResetStream();
        }

        public void CloseStream()
        {
            if (stream == null)
            {
                return;
            }
            stream.Close();
        }
    }
}
