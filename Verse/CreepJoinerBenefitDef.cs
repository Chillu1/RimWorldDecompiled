using System.Collections.Generic;
using System.Xml;
using RimWorld;

namespace Verse;

public class CreepJoinerBenefitDef : CreepJoinerBaseDef
{
	public class SkillValue
	{
		public SkillDef skill;

		public IntRange range;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
			range = (xmlRoot.HasChildNodes ? ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value) : IntRange.Zero);
		}
	}

	[MustTranslate]
	public string letterExtra;

	public List<BackstoryTrait> traits = new List<BackstoryTrait>();

	public List<SkillValue> skills = new List<SkillValue>();

	public List<HediffDef> hediffs = new List<HediffDef>();

	public List<AbilityDef> abilities = new List<AbilityDef>();
}
