using System;
using System.Collections.Generic;
using System.Text;

namespace btag
{
    public class PWManager
    {
        private Parser parser;
        private Writer writer;
        private string path;
        public Tag currentTag;

        /// <summary>
        /// Create PWManager and parse file
        /// </summary>
        /// <param name="path">path to file</param>
        public PWManager(string path) {
            parser = new Parser();
            ParseAll(path);

            writer = new Writer();
            this.path = path;
        }

        private void ParseAll(string path)
        {
            parser.OpenStream(path);
            parser.Parse();
            parser.CloseStream();
            currentTag = parser.GetManagerRoot();
        }

        /// <summary>
        /// Write tag to file
        /// </summary>
        /// <param name="tag">tag to write</param>
        public void Write(Tag tag)
        {
            writer.OpenStream(path);
            writer.WriteAll(tag);
            writer.CloseStream();
        }

        public void Write(List<Tag> tag)
        {
            writer.OpenStream(path);
            writer.WriteAllList(tag);
            writer.CloseStream();
        }

        /// <summary>
        /// Change file, clear parser and parse new file
        /// </summary>
        /// <param name="path">path to file</param>
        public void ChangeFile(string path)
        {
            this.path = path;
            parser.Clear();
            ParseAll(path);
        }

        /// <summary>
        /// Find tag on the current level
        /// </summary>
        /// <param name="title">title of child</param>
        public void FindOnLayer(string title)
        {
            currentTag = parser.FindTagLayer(currentTag, title);
        }

        /// <summary>
        /// Go upper in tree
        /// </summary>
        /// <returns>false if no parend and true if success</returns>
        public bool GoUpper()
        {
            if (currentTag.parent == null)
            {
                return false;
            }
            currentTag = currentTag.parent;
            return true;
        }

        /// <summary>
        /// Go to root of tree
        /// </summary>
        public void GoToRoot()
        {
            currentTag = parser.GetManagerRoot();
        }

        /// <summary>
        /// Get int from the value
        /// </summary>
        /// <returns>integer value</returns>
        public int GetValueInt()
        {
            return Converter.ToOptimized(currentTag.value);
        }

        /// <summary>
        /// Get string from the value
        /// </summary>
        /// <returns>string value</returns>
        public string GetValueStr()
        {
            return Converter.ToString(currentTag.value);
        }

        /// <summary>
        /// Set value of tag
        /// </summary>
        /// <param name="num">num value</param>
        public void SetValue(int num)
        {
            currentTag.value = Converter.GetOptimized(num);
        }
       
        /// <summary>
        /// Set value of tag
        /// </summary>
        /// <param name="str">string value</param>
        public void SetValue(string str)
        {
            currentTag.value = Converter.GetString(str);
        }
    }
}
