﻿#nullable enable
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
            byte[] twoBytes = new byte[2];
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
                if (oneByte[0] == 0x03)
                {
                    stream.Read(twoBytes, 0, 2);
                    byte[] value = new byte[BitConverter.ToInt16(twoBytes)];
                    stream.Read(value, 0, BitConverter.ToInt16(twoBytes));
                    if (newTag == null)
                    {
                        return false;
                    }
                    newTag.value = new byte[oneByte[0]];
                    newTag.value = value;
                }
                if (newTag != null)
                {
                    manager.AddChildToLast(newTag);
                }
                if (oneByte[0] == 0x02)
                {
                    manager.RemoveLast();
                }
            }
            return true;
        }

        public Tag? FindTag(string title)
        {
            return manager.FindTagFromRoot(title);
        }

        public Tag? FindTagLayerRoot(string title)
        {
            return manager.FindTagLayer(manager.GetRoot(), title);
        }

        public Tag? FindTagLayer(Tag layer, string title)
        {
            return manager.FindTagLayer(layer, title);
        }

        public Tag GetManagerRoot()
        {
            return manager.GetRoot();
        }

        public void Clear()
        {
            manager.Clear();
            stream.Close();
        }
    }
}