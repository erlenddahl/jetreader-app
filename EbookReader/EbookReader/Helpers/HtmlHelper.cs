using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EbookReader.Helpers
{
    public static class HtmlHelper
    {
        public static void StripHtmlTags(HtmlDocument doc, params string[] tagsToRemove)
        {
            var nodesToRemove = doc.DocumentNode
                .Descendants()
                .Where(o => tagsToRemove.Contains(o.Name))
                .ToList();

            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        public static HtmlNode GetBody(HtmlDocument doc)
        {
            return doc.DocumentNode.Descendants("body").First();
        }
    }
}
