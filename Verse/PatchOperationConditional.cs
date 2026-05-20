using System.Xml;

namespace Verse;

public class PatchOperationConditional : PatchOperationPathed
{
	private PatchOperation match;

	private PatchOperation nomatch;

	protected override bool ApplyWorker(XmlDocument xml)
	{
		if (xml.SelectSingleNode(xpath) != null)
		{
			if (match != null)
			{
				return match.Apply(xml);
			}
		}
		else if (nomatch != null)
		{
			return nomatch.Apply(xml);
		}
		if (match == null)
		{
			return nomatch != null;
		}
		return true;
	}
}
