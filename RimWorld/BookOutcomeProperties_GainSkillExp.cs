using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RimWorld;

public class BookOutcomeProperties_GainSkillExp : BookOutcomeProperties
{
	public class BookStatReward
	{
		public SkillDef skill;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
		}
	}

	public List<BookStatReward> skills = new List<BookStatReward>();

	public override Type DoerClass => typeof(BookOutcomeDoerGainSkillExp);
}
