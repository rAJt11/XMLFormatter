using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.XmlDiffPatch;
using System.Text;
using System;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using XMLFileDifference.Models;
using System.Collections.Generic;
using FSharp.Compiler;
using System.Web.UI.WebControls;
using System.Linq;

namespace XMLFileDifference.Controllers
{
    public class FileCompareController : Controller
    {
        public ActionResult CompareXML()
        {
            string filePath1 = Server.MapPath("~/App_Data/xmlfile1.xml");
            string filePath2 = Server.MapPath("~/App_Data/xmlfile2.xml");
            string OUTPUT_XML = Server.MapPath("~/App_Data/XMLFileOutput.xml");

            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder |
                                         XmlDiffOptions.IgnoreNamespaces |
                                         XmlDiffOptions.IgnorePrefixes);

            XmlWriter diffGramWriter = XmlWriter.Create(OUTPUT_XML);
            bool areEqual = xmldiff.Compare(filePath1, filePath2, false, diffGramWriter);
            diffGramWriter.Close();

            if (areEqual)
            {
                ViewBag.AreEqual = areEqual;
                ViewBag.DiffResult = "The XML files are identical.";
            }
            else
            {
                ViewBag.AreEqual = areEqual;
                ViewBag.DiffResult = "The XML files have differences:";
                ViewBag.Diffgram = xmldiff;
            }
            return View();
        }


        private SideBySideDiffModel Compare(XmlDocument doc1, XmlDocument doc2)
        {
            var differ = new Differ();
            var diffBuilder = new SideBySideDiffBuilder(differ);
            return diffBuilder.BuildDiffModel(doc1.InnerXml, doc2.InnerXml);
        }

        public ActionResult CompareXmlFiles()
        {
            string xmlfile1 = Server.MapPath("~/App_Data/xmlfile1.xml");
            string xmlfile2 = Server.MapPath("~/App_Data/xmlfile2.xml");

            if (xmlfile1 == null || xmlfile2 == null)
            {
                ModelState.AddModelError(string.Empty, "Please select two XML files to compare.");
                return View();
            }

            var doc1 = new XmlDocument();
            var doc2 = new XmlDocument();
            doc1.Load(xmlfile1);
            doc2.Load(xmlfile2);

            var comparisonResult = Compare(doc1, doc2);
            return View("Result", comparisonResult);
        }

        public ActionResult XMLFileCompare()
        {
            string INPUT_XML1 = Server.MapPath("~/temp/test.xml");
            string INPUT_XML2 = Server.MapPath("~/temp/test1.xml");
            string OUTPUT_XML = Server.MapPath("~/temp/diff.xml");
            string Patched = Server.MapPath("~/temp/patched.xml");
            GenerateDiffGram(INPUT_XML1, INPUT_XML2, XmlWriter.Create(OUTPUT_XML));
            PatchUp(INPUT_XML1, OUTPUT_XML, Patched);


            XDocument xmlDoc = XDocument.Load(OUTPUT_XML);

            var model = new XmlDiffModel
            {
                Version = xmlDoc.Root.Attribute("version").Value,
                SrcDocHash = xmlDoc.Root.Attribute("srcDocHash").Value,
                Options = xmlDoc.Root.Attribute("options").Value,
                Fragments = xmlDoc.Root.Attribute("fragments").Value,
                Nodes = ParseXmlDiffNodes(xmlDoc.Root)
            };

            return View(model);
        }

        private void GenerateDiffGram(string originalFile, string newFile, XmlWriter diffGramWriter)
        {
            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder |
                                             XmlDiffOptions.IgnoreNamespaces |
                                             XmlDiffOptions.IgnorePrefixes);
            _ = xmldiff.Compare(originalFile, newFile, false, diffGramWriter);
            diffGramWriter.Close();

        }

        private void PatchUp(string originalFile, String diffGramFile, String OutputFile)
        {
            XmlDocument sourceDoc = new XmlDocument(new NameTable());
            sourceDoc.Load(originalFile);
            XmlTextReader diffgramReader = new XmlTextReader(diffGramFile);
            XmlPatch xmlPatch = new XmlPatch();
            xmlPatch.Patch(sourceDoc, diffgramReader);
            XmlTextWriter output = new XmlTextWriter(OutputFile, Encoding.Unicode);
            sourceDoc.Save(output);
            output.Close();
        }

        //private List<XmlDiffNode> ParseXmlDiffNodes(XElement element)
        //{
        //    List<XmlDiffNode> nodes = new List<XmlDiffNode>();

        //    foreach (XElement nodeElement in element.Elements())
        //    {
        //        XmlDiffNode node = new XmlDiffNode
        //        {
        //            Match = int.Parse(nodeElement.Attribute("match").Value),
        //            ChangeType = null,
        //            AttributeName = null,
        //            OldValue = null,
        //            NewValue = null,
        //            Children = new List<XmlDiffNode>()
        //        };

        //        XNamespace xd = "http://schemas.microsoft.com/xmltools/2002/xmldiff";


        //        XElement changeElement = nodeElement.Element(xd + "change");
        //        //XElement addElement = nodeElement.Elements(xd + "add");

        //        //if (addElement != null)
        //        //{
        //        //    node.ChangeType = "add";

        //        //    XElement arrElement = addElement.Element("arr");

        //        //    if (arrElement != null)
        //        //    {
        //        //        node.OldValue = arrElement.Element("contains_value").Value;
        //        //        node.NewValue = arrElement.Element("converted_value").Value;
        //        //        node.AttributeName = arrElement.Element("key").Value;
        //        //    }
        //        //}
        //        if (changeElement != null)
        //        {
        //            node.ChangeType = "change";
        //            node.AttributeName = changeElement.Attribute("match").Value;
        //            node.OldValue = changeElement.Value;
        //            node.NewValue = changeElement.Value;
        //        }

        //        node.Children = ParseXmlDiffNodes(nodeElement);

        //        nodes.Add(node);
        //    }

        //    return nodes;
        //}


        //private List<XmlDiffNode> ParseXmlDiffNodes(XElement element)
        //{
        //    var nodes = new List<XmlDiffNode>();

        //    XNamespace xd = "http://schemas.microsoft.com/xmltools/2002/xmldiff";

        //    foreach (var nodeElement in element.Elements(xd + "node"))
        //    {
        //        var node = new XmlDiffNode
        //        {
        //            Match = int.Parse(nodeElement.Attribute("match").Value),
        //            Children = ParseXmlDiffNodes(nodeElement)
        //        };

        //        var addElement = nodeElement.Element(xd + "add");
        //        var changeElement = nodeElement.Element(xd + "change");

        //        if (addElement != null)
        //        {
        //            node.ChangeType = "add";
        //            var arrElement = addElement.Element("arr");
        //            node.OldValue = arrElement.Element("contains_value")?.Value;
        //            node.NewValue = arrElement.Element("converted_value")?.Value;
        //            node.AttributeName = arrElement.Element("key")?.Value;
        //        }
        //        else if (changeElement != null)
        //        {
        //            node.ChangeType = "change";
        //            node.AttributeName = changeElement.Attribute("match").Value;
        //            node.OldValue = changeElement.Value;
        //            node.NewValue = changeElement.Value;
        //        }

        //        nodes.Add(node);
        //    }

        //    return nodes;
        //}


        private List<XmlDiffNode> ParseXmlDiffNodes(XElement element)
        {
            var nodes = new List<XmlDiffNode>();

            XNamespace xd = "http://schemas.microsoft.com/xmltools/2002/xmldiff";

            foreach (var nodeElement in element.Elements(xd + "node"))
            {
                var node = new XmlDiffNode
                {
                    Match = nodeElement.Attribute("match") != null ? int.Parse(nodeElement.Attribute("match").Value) : 0
                };

                var addElement = nodeElement.Element(xd + "add");
                var changeElement = nodeElement.Element(xd + "change");

                if (addElement != null)
                {
                    node.ChangeType = "add";
                    var arrElement = addElement.Element("arr");

                    node.OldValue = arrElement?.Element("contains_value")?.Value;
                    node.NewValue = arrElement?.Element("converted_value")?.Value;
                    node.AttributeName = arrElement?.Element("key")?.Value;
                }
                else if (changeElement != null)
                {
                    node.ChangeType = "change";
                    node.AttributeName = changeElement.Attribute("match")?.Value;
                    node.OldValue = changeElement?.Value;
                    node.NewValue = changeElement?.Value;
                }
                else
                {
                    node.ChangeType = "match";
                }

                node.Children = ParseXmlDiffNodes(nodeElement);
                nodes.Add(node);
            }

            return nodes;
        }


    }
}
