#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace btag
{
    public class Tag
    {
        public Tag parent;
        public string title = "";
        public byte[]? value;
        public List<Tag> Childes = new List<Tag>();
        public bool active = true;

        public Tag(string titleEx)
        {
            title = titleEx;
        }

        public void AddChild(Tag tag)
        {
            tag.parent = this;
            Childes.Add(tag);
        }

        public void LastDisable()
        {
            active = false;
        }
    }
}
