using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Pawn_NeedsTracker : IExposable
{
	private Pawn pawn;

	private List<Need> needs = new List<Need>();

	public Need_Mood mood;

	public Need_Food food;

	public Need_MechEnergy energy;

	public Need_Rest rest;

	public Need_Joy joy;

	public Need_Beauty beauty;

	public Need_RoomSize roomsize;

	public Need_Outdoors outdoors;

	public Need_Indoors indoors;

	public Need_Chemical_Any drugsDesire;

	public Need_Comfort comfort;

	public Need_Learning learning;

	public Need_Play play;

	private List<Need> needsMisc = new List<Need>(0);

	public List<Need> AllNeeds => needs;

	public List<Need> MiscNeeds => needsMisc;

	public bool PrefersOutdoors
	{
		get
		{
			Need need;
			return !TryGetNeed(NeedDefOf.Indoors, out need);
		}
	}

	public bool PrefersIndoors
	{
		get
		{
			Need need;
			return TryGetNeed(NeedDefOf.Indoors, out need);
		}
	}

	public Pawn_NeedsTracker()
	{
	}

	public Pawn_NeedsTracker(Pawn newPawn)
	{
		pawn = newPawn;
		AddOrRemoveNeedsAsAppropriate();
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref needs, "needs", LookMode.Deep, pawn);
		if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (needs.RemoveAll((Need x) => x?.def == null) != 0)
			{
				Log.Error("Pawn " + pawn.ToStringSafe() + " had some null needs after loading.");
			}
			BindDirectNeedFields();
			CacheMiscNeeds();
		}
		BackCompatibility.PostExposeData(this);
	}

	public void BindDirectNeedFields()
	{
		TryGetNeed(out mood);
		TryGetNeed(out food);
		TryGetNeed(out energy);
		TryGetNeed(out rest);
		TryGetNeed(out joy);
		TryGetNeed(out beauty);
		TryGetNeed(out comfort);
		TryGetNeed(out roomsize);
		TryGetNeed(out outdoors);
		TryGetNeed(out indoors);
		TryGetNeed(out drugsDesire);
		TryGetNeed(out learning);
		TryGetNeed(out play);
	}

	private void CacheMiscNeeds()
	{
		needsMisc.Clear();
	}

	public void NeedsTrackerTickInterval(int delta)
	{
		if (pawn.IsHashIntervalTick(150, delta))
		{
			for (int i = 0; i < needs.Count; i++)
			{
				needs[i].NeedInterval();
			}
		}
	}

	public T TryGetNeed<T>() where T : Need
	{
		TryGetNeed(out T need);
		return need;
	}

	public bool TryGetNeed<T>(out T need) where T : Need
	{
		for (int i = 0; i < needs.Count; i++)
		{
			if (needs[i].GetType() == typeof(T))
			{
				need = (T)needs[i];
				return true;
			}
		}
		need = null;
		return false;
	}

	public Need TryGetNeed(NeedDef def)
	{
		TryGetNeed(def, out var need);
		return need;
	}

	public bool TryGetNeed(NeedDef def, out Need need)
	{
		for (int i = 0; i < needs.Count; i++)
		{
			if (needs[i].def == def)
			{
				need = needs[i];
				return true;
			}
		}
		need = null;
		return false;
	}

	public void SetInitialLevels()
	{
		pawn.GetStatValue(StatDefOf.MaxNutrition, applyPostProcess: true, 0);
		for (int i = 0; i < needs.Count; i++)
		{
			needs[i].SetInitialLevel();
		}
	}

	public void AddOrRemoveNeedsAsAppropriate()
	{
		List<NeedDef> allDefsListForReading = DefDatabase<NeedDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			try
			{
				NeedDef needDef = allDefsListForReading[i];
				Need need;
				if (ShouldHaveNeed(needDef))
				{
					if (!TryGetNeed(needDef, out need))
					{
						AddNeed(needDef);
					}
				}
				else if (TryGetNeed(needDef, out need))
				{
					RemoveNeed(needDef);
				}
			}
			catch (Exception arg)
			{
				Log.Error($"Error while determining if {pawn.ToStringSafe()} should have Need {allDefsListForReading[i].ToStringSafe()}: {arg}");
			}
		}
	}

	private bool ShouldHaveNeed(NeedDef nd)
	{
		if ((int)pawn.RaceProps.intelligence < (int)nd.minIntelligence)
		{
			return false;
		}
		if (!nd.developmentalStageFilter.Has(pawn.DevelopmentalStage))
		{
			return false;
		}
		if (nd.colonistsOnly && (pawn.Faction == null || !pawn.Faction.IsPlayer))
		{
			return false;
		}
		if (nd.playerMechsOnly && (!pawn.RaceProps.IsMechanoid || pawn.Faction != Faction.OfPlayer || pawn.OverseerSubject == null))
		{
			return false;
		}
		if (nd.colonistAndPrisonersOnly && (pawn.Faction == null || !pawn.Faction.IsPlayer) && (pawn.HostFaction == null || pawn.HostFaction != Faction.OfPlayer))
		{
			return false;
		}
		if (pawn.health.hediffSet.DisablesNeed(nd))
		{
			return false;
		}
		if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.DisablesNeed(nd))
		{
			return false;
		}
		if (pawn.story?.traits != null && pawn.story.traits.DisablesNeed(nd))
		{
			return false;
		}
		if (pawn.Ideo != null && pawn.Ideo.DisablesNeed(nd))
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		if (nd.onlyIfCausedByHediff)
		{
			flag = true;
			if (pawn.health.hediffSet.EnablesNeed(nd))
			{
				flag2 = true;
			}
		}
		if (ModsConfig.BiotechActive && nd.onlyIfCausedByGene)
		{
			flag = true;
			if (pawn.genes != null && pawn.genes.EnablesNeed(nd))
			{
				flag2 = true;
			}
		}
		if (ModsConfig.IdeologyActive && nd.onlyIfCausedByIdeo)
		{
			flag = true;
			if (pawn.Ideo != null && pawn.Ideo.EnablesNeed(nd))
			{
				flag2 = true;
			}
		}
		if (nd.onlyIfCausedByTrait)
		{
			flag = true;
			if (pawn.story?.traits != null && pawn.story.traits.EnablesNeed(nd))
			{
				flag2 = true;
			}
		}
		if (flag && !flag2)
		{
			return false;
		}
		if (nd.neverOnPrisoner && pawn.IsPrisoner)
		{
			return false;
		}
		if (nd.neverOnSlave && pawn.IsSlave)
		{
			return false;
		}
		if (pawn.IsMutant && pawn.mutant.Def.disableNeeds && (pawn.mutant.Def.needWhitelist == null || !pawn.mutant.Def.needWhitelist.Contains(nd)))
		{
			return false;
		}
		if (nd.titleRequiredAny != null)
		{
			if (pawn.royalty == null)
			{
				return false;
			}
			bool flag3 = false;
			foreach (RoyalTitle item in pawn.royalty.AllTitlesInEffectForReading)
			{
				if (nd.titleRequiredAny.Contains(item.def))
				{
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				return false;
			}
		}
		if (nd.nullifyingPrecepts != null && pawn.Ideo != null)
		{
			bool flag4 = false;
			foreach (PreceptDef nullifyingPrecept in nd.nullifyingPrecepts)
			{
				if (pawn.Ideo.HasPrecept(nullifyingPrecept))
				{
					flag4 = true;
					break;
				}
			}
			if (flag4)
			{
				return false;
			}
		}
		if (nd.hediffRequiredAny != null)
		{
			bool flag5 = false;
			foreach (HediffDef item2 in nd.hediffRequiredAny)
			{
				if (pawn.health.hediffSet.HasHediff(item2))
				{
					flag5 = true;
					break;
				}
			}
			if (!flag5)
			{
				return false;
			}
		}
		if (nd.defName == "Authority")
		{
			return false;
		}
		if (nd.slavesOnly && !pawn.IsSlave)
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && nd.requiredComps != null)
		{
			foreach (CompProperties requiredComp in nd.requiredComps)
			{
				if (pawn.TryGetComp(requiredComp) == null)
				{
					return false;
				}
			}
		}
		if (nd == NeedDefOf.Food)
		{
			return pawn.RaceProps.EatsFood;
		}
		if (nd == NeedDefOf.Rest)
		{
			return pawn.RaceProps.needsRest;
		}
		return true;
	}

	private void AddNeed(NeedDef nd)
	{
		Need need = (Need)Activator.CreateInstance(nd.needClass, pawn);
		need.def = nd;
		needs.Add(need);
		need.SetInitialLevel();
		BindDirectNeedFields();
	}

	private void RemoveNeed(NeedDef nd)
	{
		if (TryGetNeed(nd, out var need))
		{
			need.OnNeedRemoved();
			needs.Remove(need);
			BindDirectNeedFields();
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		if (DebugSettings.ShowDevGizmos && energy != null)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Mech energy +5%",
				action = delegate
				{
					energy.CurLevelPercentage += 0.05f;
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "DEV: Mech energy -5%",
				action = delegate
				{
					energy.CurLevelPercentage -= 0.05f;
				}
			};
		}
	}
}
