using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XMLFileDifference.Models
{
    [XmlRoot("Files")]
    public class XMLFiles
    {
        [XmlElement("File1")]
        public string File1 { get; set; }

        [XmlElement("File2")]
        public string File2 { get; set; }

    }


    public class XmlComparisonResultModel
    {
        public SideBySideDiffModel DiffModel { get; set; }
        public string OldXml { get; set; }
        public string NewXml { get; set; }
    }


    public class XmlDiffModel
    {
        public string Version { get; set; }
        public string SrcDocHash { get; set; }
        public string Options { get; set; }
        public string Fragments { get; set; }
        public List<XmlDiffNode> Nodes { get; set; }
    }

    public class XmlDiffNode
    {
        public int Match { get; set; }
        public List<XmlDiffNode> Children { get; set; }
        public string ChangeType { get; set; }
        public string AttributeName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

}