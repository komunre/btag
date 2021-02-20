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
        private int streamCounter = 0;
        public void OpenStream(string path)
        {
            stream = File.OpenRead(path);
        }

        private bool ParseTag(byte[] title)
        {
            if (stream == null)
            {
                return false;
            }
            byte[] oneByte = new byte[1] { 0x01 };
            byte[] twoBytes = new byte[2];
            Tag newTag = new Tag(Encoding.Default.GetString(title));
            stream.Read(oneByte, 0, 1);
            if (oneByte[0] == 0x03)
            {
                stream.Read(twoBytes, 0, 2);
                byte[] value = new byte[BitConverter.ToInt16(twoBytes)];
                stream.Read(value, 0, BitConverter.ToInt16(twoBytes));
                newTag.value = value;
            }
            manager.AddChildToLast(newTag);
            return true;
        }

        /// <summary>
        /// Parse entire file. Use OpenStream before parsing
        /// </summary>
        /// <returns></returns>
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
                if (oneByte[0] == 0x01)
                {
                    stream.Read(oneByte, 0, 1);
                    byte[] title = new byte[oneByte[0]];
                    stream.Read(title, 0, oneByte[0]);
                    ParseTag(title);
                }
                if (oneByte[0] == 0x02)
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
