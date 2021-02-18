#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace btag
{
    public class TagManager
    {
        private List<Tag> tags = new List<Tag>();

        public TagManager()
        {
            tags.Add(new Tag("root"));
        }

        public void AddTag(Tag tag)
        {
            tags.Add(tag);
        }

        public Tag GetRoot()
        {
            return tags[0];
        }

        public Tag? FindTagFromRoot(string title)
        {
            return FindTag(tags[0], title);
        }

        public Tag? FindTagLayer(Tag layer, string title)
        {
            if (layer.title == title)
            {
                return layer;
            }
            foreach (var child in layer.Childes)
            {
                if (child.title == title)
                {
                    return child;
                }
            }
            return null;
        }

        public Tag? FindTag(Tag parent, string title)
        {
            if (parent != null)
            {
                if (parent.title == title)
                {
                    return parent;
                }

                if (parent.Childes.Count > 0)
                {
                    foreach (var child in parent.Childes)
                    {
                        var result = FindTag(child, title);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        private Tag FindLast()
        {
            Tag curr = tags[0];
            var stop = false;
            while (curr.Childes.Count != 0 && !stop) // if children is here
            {
                var minus = 1;
                curr = curr.Childes[curr.Childes.Count - minus]; // check last child
                if (!curr.active) // return to parent
                {
                    curr = curr.parent;
                    stop = true;
                }
            }
            return curr;
        }

        public void AddChildToLast(Tag tag)
        {
            var curr = FindLast();
            curr.AddChild(tag);
        }

        public void DeactivateLast()
        {
            var curr = FindLast();
            curr.LastDisable();
        }

        public void Clear()
        {
            tags = new List<Tag>();
            tags.Add(new Tag("root"));
        }
    }
}
