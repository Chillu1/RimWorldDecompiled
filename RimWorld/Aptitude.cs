using System.Xml;
using Verse;

namespace RimWorld
{
	public class Aptitude
	{
		public SkillDef skill;

		public int level;

		public Aptitude()
		{
		}

		public Aptitude(SkillDef skill, int level)
		{
			this.skill = skill;
			this.level = level;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name, null, null, typeof(SkillDef));
			level = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
		}
	}
}
