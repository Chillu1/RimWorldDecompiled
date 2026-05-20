using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_AncientVentEmitter : CompProperties
{
	public GameConditionDef conditionToCause;

	public int onDurationMtbDays;

	public int offDurationMtbDays;

	public int heatToPush;

	public string startupMessageKey;

	public string shutdownMessageKey;

	public string activeInspectStringKey;

	public SoundDef activeSound;

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (conditionToCause == null)
		{
			yield return parentDef.defName + " has CompProperties_AncientVentEmitter with null conditionToCause.";
		}
		if (onDurationMtbDays <= 0)
		{
			yield return parentDef.defName + " has CompProperties_AncientVentEmitter with onDurationMtbDays <= 0.";
		}
		if (offDurationMtbDays <= 0)
		{
			yield return parentDef.defName + " has CompProperties_AncientVentEmitter with offDurationMtbDays <= 0.";
		}
	}
}
