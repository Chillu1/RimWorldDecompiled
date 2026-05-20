using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_StatFactor : ScenPart
{
	private StatDef stat;

	private float factor;

	private string factorBuf;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref stat, "stat");
		Scribe_Values.Look(ref factor, "factor", 0f);
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
		if (Widgets.ButtonText(scenPartRect.TopHalf(), stat.LabelCap))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (StatDef allDef in DefDatabase<StatDef>.AllDefs)
			{
				if (!allDef.forInformationOnly && allDef.CanShowWithLoadedMods())
				{
					StatDef localSd = allDef;
					list.Add(new FloatMenuOption(localSd.LabelForFullStatListCap, delegate
					{
						stat = localSd;
					}));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		Rect rect = scenPartRect.BottomHalf();
		Rect rect2 = rect.LeftHalf().Rounded();
		Rect rect3 = rect.RightHalf().Rounded();
		Text.Anchor = TextAnchor.MiddleRight;
		Widgets.Label(rect2, "multiplier".Translate());
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.TextFieldPercent(rect3, ref factor, ref factorBuf, 0f, 100f);
	}

	public override string Summary(Scenario scen)
	{
		return "ScenPart_StatFactor".Translate(stat.label, factor.ToStringPercent());
	}

	public override void Randomize()
	{
		stat = DefDatabase<StatDef>.AllDefs.Where((StatDef d) => d.scenarioRandomizable).RandomElement();
		factor = GenMath.RoundedHundredth(Rand.Range(0.1f, 3f));
	}

	public override bool TryMerge(ScenPart other)
	{
		if (other is ScenPart_StatFactor scenPart_StatFactor && scenPart_StatFactor.stat == stat)
		{
			factor *= scenPart_StatFactor.factor;
			return true;
		}
		return false;
	}

	public float GetStatFactor(StatDef stat)
	{
		if (stat == this.stat)
		{
			return factor;
		}
		return 1f;
	}

	public override bool HasNullDefs()
	{
		if (!base.HasNullDefs())
		{
			return stat == null;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((stat != null) ? stat.GetHashCode() : 0) ^ factor.GetHashCode();
	}
}
