using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml.XPath;
using UnityEngine;

namespace Verse
{
	public static class LayerLoader
	{
		public static void LoadFileIntoList(TextAsset ass, List<DiaNodeMold> NodeListToFill, List<DiaNodeList> ListListToFill, DiaNodeType NodesType)
		{
			XPathNavigator xPathNavigator = new XPathDocument(new StringReader(ass.text)).CreateNavigator();
			xPathNavigator.MoveToFirst();
			xPathNavigator.MoveToFirstChild();
			foreach (XPathNavigator item2 in xPathNavigator.Select("Node"))
			{
				try
				{
					TextReader textReader = new StringReader(item2.OuterXml);
					DiaNodeMold diaNodeMold = (DiaNodeMold)new XmlSerializer(typeof(DiaNodeMold)).Deserialize(textReader);
					diaNodeMold.nodeType = NodesType;
					NodeListToFill.Add(diaNodeMold);
					textReader.Dispose();
				}
				catch (Exception ex)
				{
					Log.Message("Exception deserializing " + item2.OuterXml + ":\n" + ex.InnerException);
				}
			}
			foreach (XPathNavigator item3 in xPathNavigator.Select("NodeList"))
			{
				try
				{
					TextReader textReader2 = new StringReader(item3.OuterXml);
					DiaNodeList item = (DiaNodeList)new XmlSerializer(typeof(DiaNodeList)).Deserialize(textReader2);
					ListListToFill.Add(item);
				}
				catch (Exception ex2)
				{
					Log.Message("Exception deserializing " + item3.OuterXml + ":\n" + ex2.InnerException);
				}
			}
		}

		public static void MarkNonRootNodes(List<DiaNodeMold> NodeList)
		{
			foreach (DiaNodeMold Node in NodeList)
			{
				RecursiveSetIsRootFalse(Node);
			}
			foreach (DiaNodeMold Node2 in NodeList)
			{
				foreach (DiaNodeMold Node3 in NodeList)
				{
					foreach (DiaOptionMold option in Node3.optionList)
					{
						bool flag = false;
						foreach (string childNodeName in option.ChildNodeNames)
						{
							if (childNodeName == Node2.name)
							{
								flag = true;
							}
						}
						if (flag)
						{
							Node2.isRoot = false;
						}
					}
				}
			}
		}

		private static void RecursiveSetIsRootFalse(DiaNodeMold d)
		{
			foreach (DiaOptionMold option in d.optionList)
			{
				foreach (DiaNodeMold childNode in option.ChildNodes)
				{
					childNode.isRoot = false;
					RecursiveSetIsRootFalse(childNode);
				}
			}
		}
	}
}
