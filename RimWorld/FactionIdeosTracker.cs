using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class FactionIdeosTracker : IExposable
{
	private Faction faction;

	private Ideo primaryIdeo;

	private List<Ideo> ideosMinor = new List<Ideo>();

	private const float MajorIdeoSelectionWeight = 4f;

	private const float MinorIdeoSelectionWeight = 1f;

	private const float ChanceToReuseExistingIdeo = 0.2f;

	private const int MaxIdeos = 10;

	private static Dictionary<Ideo, int> tmpPlayerIdeos = new Dictionary<Ideo, int>();

	public List<Ideo> IdeosMinorListForReading => ideosMinor;

	public Ideo PrimaryIdeo => primaryIdeo;

	public IEnumerable<Ideo> AllIdeos
	{
		get
		{
			if (primaryIdeo != null)
			{
				yield return primaryIdeo;
			}
			for (int i = 0; i < ideosMinor.Count; i++)
			{
				yield return ideosMinor[i];
			}
		}
	}

	public CultureDef PrimaryCulture
	{
		get
		{
			if (PrimaryIdeo == null)
			{
				return null;
			}
			return PrimaryIdeo.culture;
		}
	}

	public Ideo FluidIdeo
	{
		get
		{
			if (primaryIdeo != null && primaryIdeo.Fluid)
			{
				return primaryIdeo;
			}
			for (int i = 0; i < ideosMinor.Count; i++)
			{
				if (ideosMinor[i].Fluid)
				{
					return ideosMinor[i];
				}
			}
			return null;
		}
	}

	public FactionIdeosTracker(Faction faction)
	{
		this.faction = faction;
	}

	public FactionIdeosTracker()
	{
	}

	public bool Has(Ideo ideo)
	{
		if (ideo != primaryIdeo)
		{
			return ideosMinor.Contains(ideo);
		}
		return true;
	}

	public bool HasAnyIdeoWithMeme(MemeDef meme)
	{
		if (primaryIdeo.memes.Contains(meme))
		{
			return true;
		}
		foreach (Ideo item in ideosMinor)
		{
			if (item.memes.Contains(meme))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPrimary(Ideo ideo)
	{
		return ideo == primaryIdeo;
	}

	public bool IsMinor(Ideo ideo)
	{
		return ideosMinor.Contains(ideo);
	}

	public void Notify_MemberGainedOrLost()
	{
		if (faction.IsPlayer)
		{
			RecalculateIdeosBasedOnPlayerPawns();
		}
	}

	public void Notify_ColonistChangedIdeo()
	{
		RecalculateIdeosBasedOnPlayerPawns();
	}

	public void RecalculateIdeosBasedOnPlayerPawns()
	{
		if (!ModsConfig.IdeologyActive || Current.ProgramState != ProgramState.Playing || Find.WindowStack.IsOpen<Dialog_ConfigureIdeo>())
		{
			return;
		}
		ideosMinor.Clear();
		List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
		tmpPlayerIdeos.Clear();
		for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count; i++)
		{
			if (allMapsCaravansAndTravellingTransporters_Alive_FreeColonists[i].HomeFaction == Faction.OfPlayer)
			{
				Ideo ideo = allMapsCaravansAndTravellingTransporters_Alive_FreeColonists[i].Ideo;
				if (ideo != null)
				{
					tmpPlayerIdeos.Increment(ideo);
				}
			}
		}
		int num = 0;
		Ideo ideo2 = null;
		foreach (KeyValuePair<Ideo, int> tmpPlayerIdeo in tmpPlayerIdeos)
		{
			if (tmpPlayerIdeo.Value > num)
			{
				num = tmpPlayerIdeo.Value;
				ideo2 = tmpPlayerIdeo.Key;
			}
		}
		if (ideo2 != null && num > tmpPlayerIdeos.TryGetValue(primaryIdeo, 0))
		{
			if (primaryIdeo != null)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelNewPrimaryIdeo".Translate(primaryIdeo.Named("OLDIDEO"), ideo2.Named("NEWIDEO")), "LetterNewPrimaryIdeo".Translate(primaryIdeo.Named("OLDIDEO"), ideo2.Named("NEWIDEO"), Faction.OfPlayer.Named("FACTION")), LetterDefOf.NeutralEvent);
				primaryIdeo.Notify_NotPrimaryAnymore(ideo2);
			}
			if (primaryIdeo != null && ideo2.ColonistBelieverCountCached < Ideo.MinBelieversToEnableObligations)
			{
				Find.LetterStack.ReceiveLetter("LetterTitleObligationsActivated".Translate(ideo2), "LetterTitleObligationsActivatedIdeoBecameMajor".Translate(ideo2.Named("IDEO")), LetterDefOf.NeutralEvent);
			}
			primaryIdeo = ideo2;
		}
		foreach (KeyValuePair<Ideo, int> tmpPlayerIdeo2 in tmpPlayerIdeos)
		{
			if (tmpPlayerIdeo2.Key != primaryIdeo && !ideosMinor.Contains(tmpPlayerIdeo2.Key))
			{
				ideosMinor.Add(tmpPlayerIdeo2.Key);
			}
		}
		tmpPlayerIdeos.Clear();
	}

	public void SetPrimary(Ideo ideo)
	{
		primaryIdeo = ideo;
	}

	public void ChooseOrGenerateIdeo(IdeoGenerationParms parms)
	{
		primaryIdeo = null;
		ideosMinor.Clear();
		Ideo result;
		if (!ModsConfig.IdeologyActive || parms.forceNoExpansionIdeo)
		{
			Ideo ideo = (primaryIdeo = IdeoGenerator.GenerateNoExpansionIdeo((!faction.def.allowedCultures.NullOrEmpty()) ? faction.def.allowedCultures[0] : DefDatabase<CultureDef>.AllDefs.RandomElement(), parms));
			Find.IdeoManager.Add(ideo);
		}
		else if (parms.fixedIdeo)
		{
			Ideo ideo2 = IdeoGenerator.MakeFixedIdeo(parms);
			ideo2.primaryFactionColor = faction.Color;
			primaryIdeo = ideo2;
			Find.IdeoManager.Add(ideo2);
		}
		else if (Find.IdeoManager.classicMode && Faction.OfPlayer != null && Faction.OfPlayer.ideos.PrimaryIdeo != null)
		{
			primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
		}
		else if ((Rand.Chance(0.2f) || Find.IdeoManager.IdeosListForReading.Count((Ideo i) => !i.solid) >= 10 || faction.def.hidden) && Find.IdeoManager.IdeosListForReading.Where((Ideo x) => IdeoUtility.CanUseIdeo(faction.def, x, parms) && !x.solid).TryRandomElement(out result))
		{
			primaryIdeo = result;
		}
		else
		{
			Ideo ideo3 = IdeoGenerator.GenerateIdeo(parms);
			ideo3.primaryFactionColor = faction.Color;
			primaryIdeo = ideo3;
			Find.IdeoManager.Add(ideo3);
		}
	}

	public Ideo GetRandomIdeoForNewPawn()
	{
		return AllIdeos.RandomElementByWeightWithFallback((Ideo x) => (!IsPrimary(x)) ? 1f : 4f);
	}

	public Precept GetPrecept(PreceptDef precept)
	{
		foreach (Ideo allIdeo in AllIdeos)
		{
			foreach (Precept item in allIdeo.PreceptsListForReading)
			{
				if (item.def == precept)
				{
					return item;
				}
			}
		}
		return null;
	}

	public bool AnyPreceptWithRequiredScars()
	{
		foreach (Ideo allIdeo in AllIdeos)
		{
			if (allIdeo.RequiredScars > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static IdeoGenerationParms IdeoGenerationParmsForFaction_BackCompatibility(FactionDef factionDef, bool forceNoExpansion = false)
	{
		IdeoGenerationParms result = new IdeoGenerationParms(factionDef, forceNoExpansion);
		if (factionDef.isPlayer)
		{
			result.disallowedPrecepts = DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef x) => x.impact == PreceptImpact.High).ToList();
			result.disallowedMemes = DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => !x.allowDuringTutorial).ToList();
		}
		return result;
	}

	public void RemoveAll()
	{
		primaryIdeo = null;
		ideosMinor.Clear();
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref primaryIdeo, "primaryIdeo");
		Scribe_Collections.Look(ref ideosMinor, "ideosMinor", LookMode.Reference);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (ideosMinor.RemoveAll((Ideo x) => x == null) != 0)
		{
			Log.Error("Some ideoligion references were null after loading.");
		}
		if (primaryIdeo == null)
		{
			Log.Error("Faction had no ideoligions after loading. Adding random one.");
			if (Find.IdeoManager.IdeosListForReading.TryRandomElement(out var result))
			{
				primaryIdeo = result;
			}
		}
		if (primaryIdeo != null && primaryIdeo.createdFromNoExpansionGame && ModsConfig.IdeologyActive)
		{
			IdeoFoundationDef ideoFoundationDef = DefDatabase<IdeoFoundationDef>.AllDefs.RandomElement();
			primaryIdeo.foundation = (IdeoFoundation)Activator.CreateInstance(ideoFoundationDef.foundationClass);
			primaryIdeo.foundation.def = ideoFoundationDef;
			primaryIdeo.foundation.ideo = primaryIdeo;
			primaryIdeo.foundation.Init(IdeoGenerationParmsForFaction_BackCompatibility(faction.def));
			primaryIdeo.createdFromNoExpansionGame = false;
		}
	}
}
