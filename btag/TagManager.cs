#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace btag
{
    public class TagManager
    {
        private List<Tag> tags = new List<Tag>();
        Tag last;

        public TagManager()
        {
            tags.Add(new Tag("root"));
            last = tags[0];
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

        public bool TryFindTagLayer(Tag layer, string title, out Tag? tag)
        {
            if (layer.title == title)
            {
                tag = layer;
                return true;
            }
            foreach (var child in layer.Childes)
            {
                if (child.title == title)
                {
                    tag = child;
                    return true;
                }
            }
            tag = null;
            return false;
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
            while (curr.Childes.Count != 0) // if children is here
            {
                curr = curr.Childes[curr.Childes.Count - 1]; // check last child
                if (!curr.active) // return to parent
                {
                    curr = curr.parent;
                    break;
                }
            }
            return curr;
        }

        public void AddChildToLast(Tag tag)
        {
            last.AddChild(tag);
            last = tag;
        }

        public void DeactivateLast()
        {
            last.active = false;
            last = last.parent;
        }

        public void Clear()
        {
            tags = new List<Tag>();
            tags.Add(new Tag("root"));
        }
    }
}
