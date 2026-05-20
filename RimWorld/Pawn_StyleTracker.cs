using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Pawn_StyleTracker : IExposable
{
	public Pawn pawn;

	public BeardDef beardDef;

	public int nextStyleChangeAttemptTick = -99999;

	private TattooDef faceTattoo;

	private TattooDef bodyTattoo;

	private bool lookChangeDesired;

	public HairDef nextHairDef;

	public BeardDef nextBeardDef;

	public TattooDef nextFaceTattooDef;

	public TattooDef nextBodyTatooDef;

	public Color? nextHairColor;

	private static readonly IntRange AutoStyleChangeTicksOffsetRange = new IntRange(15000, 30000);

	private const float StyleChangeMTBDays = 20f;

	private const int LookChangeCheckInterval = 2500;

	public TattooDef FaceTattoo
	{
		get
		{
			return faceTattoo;
		}
		set
		{
			if (ModLister.CheckIdeology("Tattoos"))
			{
				faceTattoo = value;
			}
		}
	}

	public TattooDef BodyTattoo
	{
		get
		{
			return bodyTattoo;
		}
		set
		{
			if (ModLister.CheckIdeology("Tattoos"))
			{
				bodyTattoo = value;
			}
		}
	}

	public bool ShouldSpawnHairFilth
	{
		get
		{
			if (nextHairDef == null && nextBeardDef == null)
			{
				return false;
			}
			if (pawn.story.hairDef == nextHairDef)
			{
				return beardDef != nextBeardDef;
			}
			return true;
		}
	}

	public bool CanWantBeard
	{
		get
		{
			if (pawn.gender == Gender.Female && (pawn.genes == null || !pawn.genes.CanHaveBeard))
			{
				return false;
			}
			if (!pawn.DevelopmentalStage.Adult())
			{
				return false;
			}
			return true;
		}
	}

	public bool LookChangeDesired
	{
		get
		{
			if (lookChangeDesired)
			{
				return CanDesireLookChange;
			}
			return false;
		}
	}

	public bool CanDesireLookChange
	{
		get
		{
			if (ModsConfig.IdeologyActive && pawn.IsColonistPlayerControlled && !pawn.guest.IsSlave && !pawn.IsQuestLodger())
			{
				return !pawn.IsCreepJoiner;
			}
			return false;
		}
	}

	public bool HasAnyUnwantedStyleItem
	{
		get
		{
			if (!HasUnwantedHairStyle && !HasUnwantedBeard && !HasUnwantedFaceTattoo)
			{
				return HasUnwantedBodyTattoo;
			}
			return true;
		}
	}

	public bool HasUnwantedHairStyle => !PawnStyleItemChooser.WantsToUseStyle(pawn, pawn.story.hairDef);

	public bool HasUnwantedBeard => !PawnStyleItemChooser.WantsToUseStyle(pawn, beardDef);

	public bool HasUnwantedFaceTattoo => !PawnStyleItemChooser.WantsToUseStyle(pawn, faceTattoo, TattooType.Face);

	public bool HasUnwantedBodyTattoo => !PawnStyleItemChooser.WantsToUseStyle(pawn, bodyTattoo, TattooType.Body);

	public Pawn_StyleTracker()
	{
	}

	public Pawn_StyleTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void StyleTrackerTickInterval(int delta)
	{
		if (!lookChangeDesired && pawn.IsHashIntervalTick(2500, delta) && CanDesireLookChange && HasAnyUnwantedStyleItem && Rand.MTBEventOccurs(20f, 60000f, 2500f))
		{
			RequestLookChange();
		}
	}

	public void RequestLookChange()
	{
		Find.LetterStack.ReceiveLetter("LetterWantLookChange".Translate() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true), "LetterWantLookChangeDesc".Translate(pawn.Named("PAWN")), LetterDefOf.NeutralEvent, new LookTargets(pawn), null, null, new List<ThingDef> { ThingDefOf.StylingStation });
		lookChangeDesired = true;
		ResetNextStyleChangeAttemptTick();
	}

	public void ResetNextStyleChangeAttemptTick()
	{
		nextStyleChangeAttemptTick = Find.TickManager.TicksGame + AutoStyleChangeTicksOffsetRange.RandomInRange;
	}

	public void Notify_StyleItemChanged()
	{
		if (!HasAnyUnwantedStyleItem)
		{
			lookChangeDesired = false;
		}
		nextHairDef = null;
		nextBeardDef = null;
		nextFaceTattooDef = null;
		nextBodyTatooDef = null;
		pawn.Drawer.renderer.SetAllGraphicsDirty();
	}

	public void FinalizeHairColor()
	{
		if (nextHairColor.HasValue)
		{
			pawn.story.HairColor = nextHairColor.Value;
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}

	public void MakeHairFilth()
	{
		foreach (IntVec3 item in GenRadial.RadialCellsAround(pawn.Position, 1f, useCenter: true))
		{
			if (item.InBounds(pawn.Map) && Rand.Value < 0.5f)
			{
				FilthMaker.TryMakeFilth(item, pawn.Map, ThingDefOf.Filth_Hair, pawn.LabelIndefinite(), Rand.Range(1, 3));
			}
		}
	}

	public void SetupNextLookChangeData(HairDef hair = null, BeardDef beard = null, TattooDef faceTatoo = null, TattooDef bodyTattoo = null, Color? hairColor = null)
	{
		nextHairDef = hair;
		nextBeardDef = beard;
		nextFaceTattooDef = faceTatoo;
		nextBodyTatooDef = bodyTattoo;
		if (hairColor.HasValue)
		{
			if (hairColor.Value == pawn.story.HairColor)
			{
				nextHairColor = null;
			}
			else
			{
				nextHairColor = hairColor.Value;
			}
		}
	}

	public void SetupTattoos_NoIdeology()
	{
		faceTattoo = TattooDefOf.NoTattoo_Face;
		bodyTattoo = TattooDefOf.NoTattoo_Body;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref lookChangeDesired, "lookChangeDesired", defaultValue: false);
		Scribe_Values.Look(ref nextStyleChangeAttemptTick, "nextStyleChangeAttemptTick", -99999);
		Scribe_Values.Look(ref nextHairColor, "nextHairColor");
		Scribe_Defs.Look(ref beardDef, "beardDef");
		Scribe_Defs.Look(ref faceTattoo, "faceTattoo");
		Scribe_Defs.Look(ref bodyTattoo, "bodyTattoo");
		Scribe_Defs.Look(ref nextHairDef, "nextHairDef");
		Scribe_Defs.Look(ref nextBeardDef, "nextBeardDef");
		Scribe_Defs.Look(ref nextFaceTattooDef, "nextFaceTattooDef");
		Scribe_Defs.Look(ref nextBodyTatooDef, "nextBodyTattooDef");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (beardDef == null)
			{
				beardDef = BeardDefOf.NoBeard;
			}
			if (!CanWantBeard && beardDef != BeardDefOf.NoBeard)
			{
				beardDef = BeardDefOf.NoBeard;
				Log.Error(pawn.LabelShort + " had a beard. Removed.");
			}
			if (faceTattoo == null)
			{
				faceTattoo = TattooDefOf.NoTattoo_Face;
			}
			if (bodyTattoo == null)
			{
				bodyTattoo = TattooDefOf.NoTattoo_Body;
			}
		}
	}
}
