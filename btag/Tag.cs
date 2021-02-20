#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace btag
{
    public class Tag
    {
        public Tag? parent {
            get;
            private set;
        }
        public string title = "";
        /// <summary>
        /// Tag value. Converter can help
        /// </summary>
        public byte[]? value;
        public List<Tag> Childes = new List<Tag>();
        public bool active = true;

        public Tag(string titleEx)
        {
            title = titleEx;
        }

        public Tag(string titleEx, byte[] valueEx)
        {
            title = titleEx;
            value = valueEx;
        }

        /// <summary>
        /// Adds child to tag
        /// </summary>
        /// <param name="tag">child to add</param>
        public void AddChild(Tag tag)
        {
            tag.parent = this;
            Childes.Add(tag);
        }

        public void LastDisable()
        {
            active = false;
        }

        /// <summary>
        /// FOR TESTING ONLY
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool ChildrenEquals(Tag tag)
        {
            for (var i = 0; i < tag.Childes.Count; i++) // one level only. Needs to be improved
            {
                if (Childes[i].title != tag.Childes[i].title) 
                {
                    return false;
                }
            }
            return true;
        }
    }
}
