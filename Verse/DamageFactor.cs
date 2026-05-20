using System.Xml;

namespace Verse
{
	public class DamageFactor
	{
		public DamageDef damageDef;

		public float factor = 1f;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "damageDef", xmlRoot.Name);
			factor = (xmlRoot.HasChildNodes ? ParseHelper.FromString<float>(xmlRoot.FirstChild.Value) : 1f);
		}
	}
}
