using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Verse;

public static class DirectXmlSaveFormatter
{
	public static void AddWhitespaceFromRoot(XElement root)
	{
		if (!root.Elements().Any())
		{
			return;
		}
		foreach (XElement item in root.Elements().ToList())
		{
			XText content = new XText("\n");
			item.AddAfterSelf(content);
		}
		root.Elements().First().AddBeforeSelf(new XText("\n"));
		root.Elements().Last().AddAfterSelf(new XText("\n"));
		foreach (XElement item2 in root.Elements().ToList())
		{
			IndentXml(item2, 1);
		}
	}

	private static void IndentXml(XElement element, int depth)
	{
		element.AddBeforeSelf(new XText(IndentString(depth, startWithNewline: true)));
		bool startWithNewline = element.NextNode == null;
		element.AddAfterSelf(new XText(IndentString(depth - 1, startWithNewline)));
		foreach (XElement item in element.Elements().ToList())
		{
			IndentXml(item, depth + 1);
		}
	}

	private static string IndentString(int depth, bool startWithNewline)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (startWithNewline)
		{
			stringBuilder.Append("\n");
		}
		for (int i = 0; i < depth; i++)
		{
			stringBuilder.Append("  ");
		}
		return stringBuilder.ToString();
	}
}
