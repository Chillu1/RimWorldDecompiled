using System.Xml;
using Verse;

namespace RimWorld.QuestGen;

public class PrefixCapturedVar
{
	[NoTranslate]
	[TranslationHandle]
	public string name;

	public SlateRef<object> value;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		if (xmlRoot.ChildNodes.Count != 1)
		{
			Log.Error("Misconfigured PrefixCapturedVar: " + xmlRoot.OuterXml);
			return;
		}
		name = xmlRoot.Name;
		value = new SlateRef<object>(DirectXmlToObject.InnerTextWithReplacedNewlinesOrXML(xmlRoot));
		TKeySystem.MarkTreatAsList(xmlRoot.ParentNode);
	}
}
