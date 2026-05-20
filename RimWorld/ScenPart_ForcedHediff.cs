using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_ForcedHediff : ScenPart_PawnModifier
{
	private HediffDef hediff;

	private FloatRange severityRange;

	private float MaxSeverity
	{
		get
		{
			if (!(hediff.lethalSeverity > 0f))
			{
				return 1f;
			}
			return hediff.lethalSeverity * 0.99f;
		}
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f + 31f);
		if (Widgets.ButtonText(scenPartRect.TopPartPixels(ScenPart.RowHeight), hediff.LabelCap))
		{
			FloatMenuUtility.MakeMenu(PossibleHediffs(), (HediffDef hd) => hd.LabelCap, (HediffDef hd) => delegate
			{
				hediff = hd;
				if (severityRange.max > MaxSeverity)
				{
					severityRange.max = MaxSeverity;
				}
				if (severityRange.min > MaxSeverity)
				{
					severityRange.min = MaxSeverity;
				}
			});
		}
		Widgets.FloatRange(new Rect(scenPartRect.x, scenPartRect.y + ScenPart.RowHeight, scenPartRect.width, 31f), listing.CurHeight.GetHashCode(), ref severityRange, 0f, MaxSeverity, "ConfigurableSeverity");
		DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(ScenPart.RowHeight * 2f));
	}

	private IEnumerable<HediffDef> PossibleHediffs()
	{
		return DefDatabase<HediffDef>.AllDefsListForReading.Where((HediffDef x) => x.scenarioCanAdd);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref hediff, "hediff");
		Scribe_Values.Look(ref severityRange, "severityRange");
	}

	public override string Summary(Scenario scen)
	{
		return "ScenPart_PawnsHaveHediff".Translate(context.ToStringHuman(), chance.ToStringPercent(), hediff.label).CapitalizeFirst();
	}

	public override void Randomize()
	{
		base.Randomize();
		hediff = PossibleHediffs().RandomElement();
		severityRange.max = Rand.Range(MaxSeverity * 0.2f, MaxSeverity * 0.95f);
		severityRange.min = severityRange.max * Rand.Range(0f, 0.95f);
	}

	public override bool TryMerge(ScenPart other)
	{
		if (other is ScenPart_ForcedHediff scenPart_ForcedHediff && hediff == scenPart_ForcedHediff.hediff)
		{
			chance = GenMath.ChanceEitherHappens(chance, scenPart_ForcedHediff.chance);
			return true;
		}
		return false;
	}

	public override bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
	{
		if (!base.AllowPlayerStartingPawn(pawn, tryingToRedress, req))
		{
			return false;
		}
		if (hideOffMap)
		{
			if (!req.AllowDead && !req.ForceDead && pawn.health.WouldDieAfterAddingHediff(hediff, null, severityRange.max))
			{
				return false;
			}
			if (!req.AllowDowned && !req.ForceDead && pawn.health.WouldBeDownedAfterAddingHediff(hediff, null, severityRange.max))
			{
				return false;
			}
		}
		return true;
	}

	protected override void ModifyNewPawn(Pawn p)
	{
		AddHediff(p);
	}

	protected override void ModifyHideOffMapStartingPawnPostMapGenerate(Pawn p)
	{
		AddHediff(p);
	}

	private void AddHediff(Pawn p)
	{
		if (Find.GameInitData == null || !Find.GameInitData.QuickStarted || !Prefs.DisableQuickStartCryptoSickness || this.hediff != HediffDefOf.CryptosleepSickness)
		{
			Hediff hediff = HediffMaker.MakeHediff(this.hediff, p);
			hediff.Severity = severityRange.RandomInRange;
			p.health.AddHediff(hediff);
		}
	}

	public override bool HasNullDefs()
	{
		if (!base.HasNullDefs())
		{
			return hediff == null;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((hediff != null) ? hediff.GetHashCode() : 0) ^ severityRange.GetHashCode();
	}
}
