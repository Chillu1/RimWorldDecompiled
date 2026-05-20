using System.Xml;

namespace Verse
{
	public class LifeStageWorkSettings
	{
		public WorkTypeDef workType;

		public int minAge;

		public bool IsDisabled(Pawn pawn)
		{
			return pawn.ageTracker.AgeBiologicalYears < minAge;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "workType", xmlRoot.Name);
			minAge = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
		}
	}
}
