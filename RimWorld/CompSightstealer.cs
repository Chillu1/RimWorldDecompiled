using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class CompSightstealer : ThingComp
{
	private const float BaseVisibleRadius = 14f;

	private const int UndetectedTimeout = 1200;

	private const int CheckDetectedIntervalTicks = 7;

	private const float FirstDetectedRadius = 30f;

	private const int RevealedLetterDelayTicks = 6;

	private const int AmbushCallMTBTicks = 600;

	[Unsaved(false)]
	private HediffComp_Invisibility invisibility;

	private int lastDetectedTick = -99999;

	private static float lastNotified = -99999f;

	private const float NotifyCooldownSeconds = 60f;

	private Pawn Sightstealer => (Pawn)parent;

	private HediffComp_Invisibility Invisibility => invisibility ?? (invisibility = Sightstealer.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.HoraxianInvisibility)?.TryGetComp<HediffComp_Invisibility>());

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref lastDetectedTick, "lastDetectedTick", 0);
	}

	public override void CompTick()
	{
		if (Sightstealer.IsShambler)
		{
			return;
		}
		if (Invisibility == null)
		{
			Sightstealer.health.AddHediff(HediffDefOf.HoraxianInvisibility);
		}
		if (!Sightstealer.Spawned)
		{
			return;
		}
		if (Sightstealer.IsHashIntervalTick(7))
		{
			if (Find.TickManager.TicksGame > lastDetectedTick + 1200)
			{
				CheckDetected();
			}
			if (Find.TickManager.TicksGame > lastDetectedTick + 1200)
			{
				Invisibility.BecomeInvisible();
			}
		}
		Lord lord = Sightstealer.GetLord();
		if (lord != null && Rand.MTBEventOccurs(600f, 1f, 1f) && (Sightstealer.CurJob?.def == JobDefOf.Wait || lord.LordJob is LordJob_EntitySwarm))
		{
			Sightstealer.caller?.DoCall();
		}
	}

	private void CheckDetected()
	{
		foreach (Pawn item in Sightstealer.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
		{
			if (PawnCanDetect(item))
			{
				if (!Invisibility.PsychologicallyVisible)
				{
					Invisibility.BecomeVisible();
				}
				lastDetectedTick = Find.TickManager.TicksGame;
			}
		}
	}

	private bool PawnCanDetect(Pawn pawn)
	{
		if (pawn.Faction == Faction.OfEntities || pawn.Downed || !pawn.Awake())
		{
			return false;
		}
		if (pawn.IsAnimal)
		{
			return false;
		}
		if (!Sightstealer.Position.InHorDistOf(pawn.Position, GetPawnSightRadius(pawn, Sightstealer)))
		{
			return false;
		}
		return GenSight.LineOfSightToThing(pawn.Position, Sightstealer, parent.Map);
	}

	private static float GetPawnSightRadius(Pawn pawn, Pawn sightstealer)
	{
		float num = 14f;
		if (pawn.genes == null || pawn.genes.AffectedByDarkness)
		{
			float t = sightstealer.Map.glowGrid.GroundGlowAt(sightstealer.Position);
			num *= Mathf.Lerp(0.33f, 1f, t);
		}
		return num * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
	}

	public override void Notify_UsedVerb(Pawn pawn, Verb verb)
	{
		base.Notify_UsedVerb(pawn, verb);
		if (!Sightstealer.IsShambler)
		{
			Invisibility.BecomeVisible();
			lastDetectedTick = Find.TickManager.TicksGame;
		}
	}

	public override void Notify_BecameVisible()
	{
		SoundDefOf.Pawn_Sightstealer_Howl.PlayOneShotOnCamera();
		foreach (Pawn item in Sightstealer.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
		{
			if (item.kindDef == PawnKindDefOf.Sightstealer && item != Sightstealer && item.Position.InHorDistOf(Sightstealer.Position, 30f) && !item.IsPsychologicallyInvisible() && GenSight.LineOfSight(item.Position, Sightstealer.Position, item.Map))
			{
				return;
			}
		}
		if (RealTime.LastRealTime > lastNotified + 60f)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelSightstealerRevealed".Translate(), "LetterSightstealerRevealed".Translate(), LetterDefOf.ThreatBig, Sightstealer, null, null, null, null, 6);
		}
		else
		{
			Messages.Message("MessageSightstealerRevealed".Translate(), Sightstealer, MessageTypeDefOf.ThreatBig);
		}
		lastNotified = RealTime.LastRealTime;
		lastDetectedTick = Find.TickManager.TicksGame;
	}
}
