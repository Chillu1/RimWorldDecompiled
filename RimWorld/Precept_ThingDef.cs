using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Precept_ThingDef : Precept
{
	private ThingDef thingDef;

	private static List<StyleCategoryPair> tmpStylesForBuilding = new List<StyleCategoryPair>();

	private static List<ThingDef> usedThingDefsTmp = new List<ThingDef>();

	public override string TipLabel => def.tipLabelOverride ?? ((string)(def.issue.LabelCap + ": " + ThingDef.LabelCap));

	public ThingDef ThingDef
	{
		get
		{
			return thingDef;
		}
		set
		{
			if (thingDef != value)
			{
				thingDef = value;
				Notify_ThingDefSet();
			}
		}
	}

	public static List<StyleCategoryPair> AllPossibleStylesForBuilding(ThingDef building)
	{
		tmpStylesForBuilding.Clear();
		foreach (StyleCategoryDef item in DefDatabase<StyleCategoryDef>.AllDefsListForReading)
		{
			foreach (ThingDefStyle thingDefStyle in item.thingDefStyles)
			{
				if (thingDefStyle.ThingDef == building)
				{
					tmpStylesForBuilding.Add(new StyleCategoryPair
					{
						category = item,
						styleDef = thingDefStyle.StyleDef
					});
				}
			}
		}
		return tmpStylesForBuilding;
	}

	public override void Init(Ideo ideo, FactionDef generatingFor = null)
	{
		base.Init(ideo);
		IEnumerable<PreceptThingChance> enumerable = null;
		if (!def.canUseAlreadyUsedThingDef)
		{
			usedThingDefsTmp.Clear();
			foreach (Precept item in ideo.PreceptsListForReading)
			{
				if (item != this && item is Precept_ThingStyle precept_ThingStyle)
				{
					usedThingDefsTmp.Add(precept_ThingStyle.ThingDef);
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
		if (ThingDef == null)
		{
			if (enumerable.Any() && enumerable.TryRandomElementByWeight((PreceptThingChance d) => d.chance, out var result))
			{
				ThingDef = result.def;
			}
			else
			{
				ThingDef = def.Worker.ThingDefsForIdeo(ideo, generatingFor).RandomElementByWeight((PreceptThingChance d) => d.chance).def;
				Log.Warning("Failed to generate a unique building for " + ideo.name + " for precept " + def.defName);
			}
		}
		if (UsesGeneratedName)
		{
			RegenerateName();
		}
		Notify_ThingDefSet();
	}

	protected virtual void Notify_ThingDefSet()
	{
		ideo.style.ResetStyleForThing(ThingDef);
		if (ThingDef.canEditAnyStyle && ideo.GetStyleAndCategoryFor(ThingDef) == null)
		{
			StyleCategoryPair styleAndCat = AllPossibleStylesForBuilding(ThingDef).RandomElement();
			ideo.style.SetStyleForThingDef(ThingDef, styleAndCat);
		}
	}

	public override string GenerateNameRaw()
	{
		return name;
	}

	public override void DrawIcon(Rect rect)
	{
		Widgets.DefIcon(rect, ThingDef, GenStuff.DefaultStuffFor(ThingDef), 1f, ideo.GetStyleFor(ThingDef));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref thingDef, "thingDef");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && ThingDef == null)
		{
			Log.Error(GetType().Name + " had null thingDef after loading.");
			ThingDef = def.Worker.ThingDefs.RandomElement().def;
		}
	}

	public override void CopyTo(Precept other)
	{
		base.CopyTo(other);
		((Precept_ThingDef)other).thingDef = thingDef;
	}
}
