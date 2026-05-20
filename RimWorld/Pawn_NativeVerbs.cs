using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Pawn_NativeVerbs : IVerbOwner, IExposable
{
	private Pawn pawn;

	public VerbTracker verbTracker;

	private Verb_BeatFire cachedBeatFireVerb;

	private Verb_Ignite cachedIgniteVerb;

	private List<VerbProperties> cachedVerbProperties;

	public Verb_BeatFire BeatFireVerb
	{
		get
		{
			if (cachedBeatFireVerb == null)
			{
				cachedBeatFireVerb = (Verb_BeatFire)verbTracker.GetVerb(VerbCategory.BeatFire);
			}
			return cachedBeatFireVerb;
		}
	}

	public Verb_Ignite IgniteVerb
	{
		get
		{
			if (cachedIgniteVerb == null)
			{
				cachedIgniteVerb = (Verb_Ignite)verbTracker.GetVerb(VerbCategory.Ignite);
			}
			return cachedIgniteVerb;
		}
	}

	VerbTracker IVerbOwner.VerbTracker => verbTracker;

	List<VerbProperties> IVerbOwner.VerbProperties
	{
		get
		{
			CheckCreateVerbProperties();
			return cachedVerbProperties;
		}
	}

	List<Tool> IVerbOwner.Tools => null;

	ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

	Thing IVerbOwner.ConstantCaster => pawn;

	private Thing ConstantCaster { get; }

	string IVerbOwner.UniqueVerbOwnerID()
	{
		return "NativeVerbs_" + pawn.ThingID;
	}

	bool IVerbOwner.VerbsStillUsableBy(Pawn p)
	{
		return p == pawn;
	}

	public Pawn_NativeVerbs(Pawn pawn)
	{
		this.pawn = pawn;
		verbTracker = new VerbTracker(this);
	}

	public void NativeVerbsTick()
	{
		verbTracker.VerbsTick();
	}

	public bool TryStartIgnite(Thing target)
	{
		if (IgniteVerb == null)
		{
			Log.ErrorOnce(pawn?.ToString() + " tried to ignite " + target?.ToString() + " but has no ignite verb.", 76453432);
			return false;
		}
		if (pawn.stances.FullBodyBusy)
		{
			return false;
		}
		return IgniteVerb.TryStartCastOn(target);
	}

	public bool TryBeatFire(Fire targetFire)
	{
		if (BeatFireVerb == null)
		{
			Log.ErrorOnce(pawn?.ToString() + " tried to beat fire " + targetFire?.ToString() + " but has no beat fire verb.", 935137531);
			return false;
		}
		if (pawn.stances.FullBodyBusy)
		{
			return false;
		}
		return BeatFireVerb.TryStartCastOn(targetFire);
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
		BackCompatibility.PostExposeData(this);
	}

	private void CheckCreateVerbProperties()
	{
		if (cachedVerbProperties == null && ((int)pawn.RaceProps.intelligence >= 1 || pawn.RaceProps.giveNonToolUserBeatFireVerb))
		{
			cachedVerbProperties = new List<VerbProperties>();
			cachedVerbProperties.Add(NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.BeatFire));
			if (!pawn.RaceProps.IsMechanoid && !pawn.RaceProps.disableIgniteVerb && (int)pawn.RaceProps.intelligence >= 1)
			{
				cachedVerbProperties.Add(NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.Ignite));
			}
		}
	}
}
