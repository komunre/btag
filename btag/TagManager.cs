#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace btag
{
    public class TagManager
    {
        public int lastTag = 0;
        private List<Tag> tags = new List<Tag>();

        public TagManager()
        {
            tags.Add(new Tag("root"));
        }

        public void AddTag(Tag tag)
        {
            tags.Add(tag);
            lastTag = tags.IndexOf(tag);
        }

        public Tag GetRoot()
        {
            return tags[0];
        }

        private Tag FindLast()
        {
            Tag curr = tags[0];
            var stop = false;
            while (curr.Childes.Count != 0 && !stop)
            {
                var minus = 1;
                curr = curr.Childes[curr.Childes.Count - minus];
                while (!curr.active)
                {
                    minus++;
                    if (minus > curr.parent.Childes.Count)
                    {
                        curr = curr.parent;
                        stop = true;
                        break;
                    }
                    curr = curr.parent.Childes[curr.parent.Childes.Count - minus];
                }
            }
            return curr;
        }

        public void AddChildToLast(Tag tag)
        {
            var curr = FindLast();
            curr.AddChild(tag);
        }

        public void RemoveLast()
        {
            var curr = FindLast();
            curr.LastDisable();
        }
    }
}
