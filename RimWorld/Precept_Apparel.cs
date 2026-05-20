using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Precept_Apparel : Precept
{
	public ThingDef apparelDef;

	private Gender targetGender;

	private Gender? overrideGender;

	private static List<ThingDef> usedThingDefsTmp = new List<ThingDef>();

	public override bool SortByImpact => false;

	public override bool CanRegenerate => def.Worker.ThingDefs.Count() > 1;

	public override string TipLabel => UIInfoFirstLine + ": " + def.LabelCap;

	public override string UIInfoFirstLine
	{
		get
		{
			if (apparelDef != null)
			{
				return apparelDef.LabelCap;
			}
			return base.UIInfoFirstLine;
		}
	}

	public override string UIInfoSecondLine
	{
		get
		{
			if (TargetGender == Gender.None)
			{
				return "Everyone".Translate() + ": " + base.UIInfoSecondLine;
			}
			return TargetGender.GetLabel().CapitalizeFirst() + ": " + base.UIInfoSecondLine;
		}
	}

	public Gender TargetGender => overrideGender ?? targetGender;

	public override void Init(Ideo ideo, FactionDef generatingFor = null)
	{
		base.Init(ideo, generatingFor);
		for (int i = 0; i < def.comps.Count; i++)
		{
			if (def.comps[i] is PreceptComp_Apparel preceptComp_Apparel)
			{
				targetGender = preceptComp_Apparel.AffectedGender(ideo);
				break;
			}
		}
		InitDescription();
	}

	public override void Regenerate(Ideo ideo, FactionDef generatingFor = null)
	{
		IEnumerable<PreceptThingChance> enumerable = null;
		if (!def.canUseAlreadyUsedThingDef)
		{
			usedThingDefsTmp.Clear();
			foreach (Precept item in ideo.PreceptsListForReading)
			{
				if (item is Precept_Apparel precept_Apparel)
				{
					usedThingDefsTmp.Add(precept_Apparel.apparelDef);
				}
			}
			enumerable = from bd in def.Worker.ThingDefsForIdeo(ideo, generatingFor)
				where !usedThingDefsTmp.Contains(bd.def)
				select bd;
		}
		else
		{
			enumerable = def.Worker.ThingDefsForIdeo(ideo, generatingFor);
		}
		if (enumerable.Any() && enumerable.TryRandomElementByWeight((PreceptThingChance d) => d.chance, out var result))
		{
			apparelDef = result.def;
		}
		else
		{
			apparelDef = def.Worker.ThingDefsForIdeo(ideo, generatingFor).RandomElementByWeight((PreceptThingChance d) => d.chance).def;
			Log.Warning("Failed to generate a unique apparel for " + ideo.name);
		}
		if (ideo.SupremeGender == Gender.None && Rand.Value < 0.5f)
		{
			overrideGender = ((Rand.Value < 0.5f) ? Gender.Male : Gender.Female);
		}
		else
		{
			overrideGender = null;
		}
		base.Regenerate(ideo);
	}

	public override void DrawIcon(Rect rect)
	{
		Widgets.ThingIcon(rect, apparelDef, null, ideo.GetStyleFor(apparelDef), 1f, ideo.ApparelColor);
	}

	public override IEnumerable<FloatMenuOption> EditFloatMenuOptions()
	{
		string text = "SetTargetGender".Translate() + ": ";
		FloatMenuOption floatMenuOption = new FloatMenuOption(text + Gender.Male.GetLabel().CapitalizeFirst(), delegate
		{
			overrideGender = Gender.Male;
			InitDescription();
		});
		FloatMenuOption setFemale = new FloatMenuOption(text + Gender.Female.GetLabel().CapitalizeFirst(), delegate
		{
			overrideGender = Gender.Female;
			InitDescription();
		});
		FloatMenuOption setNone = new FloatMenuOption(text + "Everyone".Translate().CapitalizeFirst(), delegate
		{
			overrideGender = Gender.None;
			InitDescription();
		});
		switch (TargetGender)
		{
		case Gender.None:
			yield return floatMenuOption;
			yield return setFemale;
			break;
		case Gender.Male:
			yield return setFemale;
			yield return setNone;
			break;
		case Gender.Female:
			yield return floatMenuOption;
			yield return setNone;
			break;
		}
		if (def.apparelPreceptSwapDef != null)
		{
			yield return new FloatMenuOption("SetApparelType".Translate() + ": " + def.apparelPreceptSwapDef.LabelCap, delegate
			{
				def = def.apparelPreceptSwapDef;
			});
		}
	}

	private void InitDescription()
	{
		string arg = ((TargetGender != Gender.None) ? TargetGender.GetLabel() : ((string)"All".Translate()));
		descOverride = def.description.Formatted(arg.Named("GENDER"), NamedArgumentUtility.Named(apparelDef, "APPAREL")).CapitalizeFirst();
	}

	public override bool CompatibleWith(Precept other)
	{
		if (other.def.prefersNudity && (TargetGender == Gender.None || TargetGender == other.def.genderPrefersNudity))
		{
			return false;
		}
		if (other is Precept_Apparel precept_Apparel && def == other.def && (TargetGender == precept_Apparel.TargetGender || TargetGender == Gender.None || precept_Apparel.TargetGender == Gender.None) && !ApparelUtility.CanWearTogether(apparelDef, precept_Apparel.apparelDef, BodyDefOf.Human))
		{
			return false;
		}
		return base.CompatibleWith(other);
	}

	public override bool GetPlayerWarning(out string shortText, out string description)
	{
		foreach (Precept_Role item in ideo.RolesListForReading)
		{
			if ((TargetGender != Gender.None && item.restrictToSupremeGender && ideo.SupremeGender != Gender.None && ideo.SupremeGender != TargetGender) || item.ApparelRequirements == null)
			{
				continue;
			}
			foreach (PreceptApparelRequirement apparelRequirement in item.ApparelRequirements)
			{
				IEnumerable<ThingDef> source = apparelRequirement.requirement.AllRequiredApparel(ideo.SupremeGender);
				if (source.Any((ThingDef ap) => ap != apparelDef && !ApparelUtility.CanWearTogether(apparelDef, ap, BodyDefOf.Human)))
				{
					shortText = "MessageIdeoWarnRoleApparelOverlapsDesiredApparel".Translate(apparelDef.label.Named("DESIREDAPPAREL"), item.def.label.Named("ROLE"));
					description = "DescriptionIdeoWarnRoleApparelOverlapsDesiredApparel".Translate(apparelDef.label.Named("DESIREDAPPAREL"), item.def.label.Named("ROLE"), string.Join(", ", source.Select((ThingDef ap) => ap.label)).CapitalizeFirst().Named("ROLEAPPAREL")).CapitalizeFirst();
					return true;
				}
			}
		}
		shortText = null;
		description = null;
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref apparelDef, "apparelDef");
		Scribe_Values.Look(ref targetGender, "targetGender", Gender.None);
		Scribe_Values.Look(ref overrideGender, "overrideGender");
	}

	public override void CopyTo(Precept other)
	{
		base.CopyTo(other);
		Precept_Apparel obj = (Precept_Apparel)other;
		obj.apparelDef = apparelDef;
		obj.targetGender = targetGender;
		obj.overrideGender = overrideGender;
	}
}
