using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompObelisk_Abductor : CompObelisk_ExplodingSpawner
{
	private int cooldownTick;

	private Map labyrinthMap;

	private int nextTeleportTick;

	private int lastTeleportTick;

	private int interactSkipTick;

	private int colonists;

	private int others;

	private Pawn instigator;

	private Dictionary<Pawn, Effecter> effects = new Dictionary<Pawn, Effecter>();

	private Effecter buildingEffecter;

	private List<Pawn> teleporting = new List<Pawn>();

	private LabyrinthMapComponent mapComp;

	private bool generating;

	private const int LabyrinthSize = 90;

	private const int LastTeleportExplosionDelay = 180;

	private const int InteractSkipDelayTicks = 240;

	private const int ActivityTeleportDelayTicks = 240;

	private const float ColonistFactor = 0.5f;

	private const float OtherFactor = 0.2f;

	private const float HostilePointsFactor = 0.15f;

	private static readonly FloatRange CooldownDaysRange = new FloatRange(15f, 80f);

	private static readonly IntRange TeleportDelayTicksRange = new IntRange(10, 30);

	private static readonly List<Pawn> tmpRemoveEffectors = new List<Pawn>();

	public LabyrinthMapComponent Labyrinth => mapComp ?? (mapComp = labyrinthMap.GetComponent<LabyrinthMapComponent>());

	public override void TriggerInteractionEffect(Pawn interactor, bool triggeredByPlayer = false)
	{
		if (!triggeredByPlayer)
		{
			if (GenTicks.TicksGame < cooldownTick)
			{
				return;
			}
			cooldownTick = GenTicks.TicksGame + Mathf.RoundToInt(CooldownDaysRange.RandomInRange * 60000f);
		}
		if (labyrinthMap != null)
		{
			Labyrinth.TeleportToLabyrinth(interactor);
			SendTriggeredLetter(interactor);
			return;
		}
		instigator = interactor;
		interactor.stances.stunner.StunFor(240, null, addBattleLog: false, showMote: false);
		interactSkipTick = GenTicks.TicksGame + 240;
		effects[interactor] = EffecterDefOf.AbductWarmup_Pawn.SpawnAttached(interactor, interactor.MapHeld);
		if (buildingEffecter == null)
		{
			buildingEffecter = EffecterDefOf.AbductWarmup_Monolith.SpawnAttached(parent, parent.MapHeld);
		}
		Find.TickManager.slower.SignalForceNormalSpeedShort();
		SendDisappearingLetter(interactor);
	}

	public override void OnActivityActivated()
	{
		base.OnActivityActivated();
		Find.LetterStack.ReceiveLetter("ObeliskAbductorLetterLabel".Translate(), "ObeliskAbductorLetter".Translate(), LetterDefOf.ThreatBig, parent);
		pointsRemaining *= 0.15f;
		int num = 0;
		int num2 = 0;
		foreach (Pawn item in parent.MapHeld.mapPawns.AllPawnsSpawned)
		{
			if (IsValidColonist(item))
			{
				num++;
			}
			else if (IsValidOther(item))
			{
				num2++;
			}
		}
		colonists = Mathf.Max(Mathf.FloorToInt((float)num * 0.5f), 1);
		others = Mathf.Max(Mathf.FloorToInt((float)num2 * 0.2f), 1);
		foreach (Pawn item2 in parent.MapHeld.mapPawns.AllPawnsSpawned)
		{
			if (IsValidColonist(item2) && colonists > 0)
			{
				colonists--;
				teleporting.Add(item2);
				item2.stances.stunner.StunFor(2400, null, addBattleLog: false, showMote: false);
				effects[item2] = EffecterDefOf.AbductWarmup_Pawn.SpawnAttached(item2, item2.MapHeld);
				if (buildingEffecter == null)
				{
					buildingEffecter = EffecterDefOf.AbductWarmup_Monolith.SpawnAttached(parent, parent.MapHeld);
				}
			}
			else if (IsValidOther(item2) && others > 0)
			{
				others--;
				teleporting.Add(item2);
			}
		}
		instigator = null;
		nextTeleportTick = GenTicks.TicksGame + 240;
		lastTeleportTick = nextTeleportTick;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (interactSkipTick != 0 && GenTicks.TicksGame >= interactSkipTick)
		{
			interactSkipTick = 0;
			if (labyrinthMap == null)
			{
				GenerateLabyrinth(instigator);
			}
			SendTriggeredLetter(instigator);
			instigator = null;
		}
		VFXTick();
		if (activated)
		{
			ActivatedTick();
		}
	}

	private void VFXTick()
	{
		foreach (var (pawn2, effecter2) in effects)
		{
			if (pawn2.Map == parent.Map)
			{
				effecter2.EffectTick(pawn2, pawn2);
				continue;
			}
			effecter2.Cleanup();
			tmpRemoveEffectors.Add(pawn2);
		}
		effects.RemoveRange(tmpRemoveEffectors);
		tmpRemoveEffectors.Clear();
		if (effects.Count == 0 && buildingEffecter != null)
		{
			buildingEffecter.Cleanup();
			buildingEffecter = null;
		}
	}

	private void ActivatedTick()
	{
		if (explodeTick > 0 || GenTicks.TicksGame < nextTeleportTick)
		{
			return;
		}
		if (labyrinthMap == null && !generating)
		{
			lastTeleportTick = GenTicks.TicksGame;
			GenerateLabyrinth(null);
			return;
		}
		if (labyrinthMap == null)
		{
			nextTeleportTick = GenTicks.TicksGame + TeleportDelayTicksRange.RandomInRange;
			lastTeleportTick = GenTicks.TicksGame;
			return;
		}
		nextTeleportTick = GenTicks.TicksGame + TeleportDelayTicksRange.RandomInRange;
		if (explodeTick < 0 && GenTicks.TicksGame >= lastTeleportTick + 180)
		{
			PrepareExplosion();
		}
		for (int num = teleporting.Count - 1; num >= 0; num--)
		{
			if (teleporting[num].DestroyedOrNull())
			{
				teleporting.RemoveAt(num);
			}
		}
		if (!teleporting.Empty())
		{
			Pawn pawn = teleporting.RandomElement();
			teleporting.Remove(pawn);
			if (IsValidColonist(pawn))
			{
				colonists--;
			}
			else if (IsValidHostile(pawn))
			{
				pointsRemaining -= pawn.kindDef.combatPower;
			}
			else
			{
				others--;
			}
			Labyrinth.TeleportToLabyrinth(pawn);
			lastTeleportTick = GenTicks.TicksGame;
			pawn.stances.stunner.StopStun();
		}
	}

	private void GenerateLabyrinth(Pawn interactor)
	{
		generating = true;
		LongEventHandler.QueueLongEvent(delegate
		{
			labyrinthMap = PocketMapUtility.GeneratePocketMap(new IntVec3(90, 1, 90), MapGeneratorDefOf.Labyrinth, null, parent.MapHeld);
			LabyrinthMapComponent component = labyrinthMap.GetComponent<LabyrinthMapComponent>();
			component.abductorObelisk = (Building)parent;
			component.sourceMap = parent.Map;
		}, "GeneratingLabyrinth", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap, showExtraUIInfo: false, forceHideUI: false, delegate
		{
			generating = false;
			if (interactor != null)
			{
				Labyrinth.TeleportToLabyrinth(interactor);
			}
			CameraJumper.TryJump(interactor, CameraJumper.MovementMode.Cut);
		});
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		if (labyrinthMap != null && !labyrinthMap.mapPawns.AnyColonistSpawned && !labyrinthMap.mapPawns.SlavesOfColonySpawned.Any())
		{
			PocketMapUtility.DestroyPocketMap(labyrinthMap);
			Notify_MapDestroyed();
		}
	}

	public void Notify_MapDestroyed()
	{
		labyrinthMap = null;
		mapComp = null;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref cooldownTick, "cooldownTick", 0);
		Scribe_Values.Look(ref nextTeleportTick, "nextTeleport", 0);
		Scribe_Values.Look(ref lastTeleportTick, "lastTeleport", 0);
		Scribe_Values.Look(ref interactSkipTick, "interactSendTick", 0);
		Scribe_Values.Look(ref colonists, "colonists", 0);
		Scribe_Values.Look(ref others, "others", 0);
		Scribe_Collections.Look(ref teleporting, "teleporting", LookMode.Reference);
		Scribe_References.Look(ref labyrinthMap, "labyrinthMap");
		Scribe_References.Look(ref instigator, "storedInstigator");
	}

	private static bool IsValidColonist(Pawn pawn)
	{
		if (!pawn.IsColonist && !pawn.IsSlaveOfColony)
		{
			return pawn.IsPrisonerOfColony;
		}
		return true;
	}

	private static bool IsValidHostile(Pawn pawn)
	{
		if (pawn.HostileTo(Faction.OfPlayer))
		{
			return !pawn.IsOnHoldingPlatform;
		}
		return false;
	}

	private static bool IsValidOther(Pawn pawn)
	{
		if (!pawn.IsAnimal)
		{
			return pawn.IsColonyMech;
		}
		return true;
	}

	private static void SendDisappearingLetter(Pawn interactor)
	{
		TaggedString label = "ObeliskAbductedDisappearingLetterLabel".Translate(interactor.Named("PAWN"));
		TaggedString text = "ObeliskAbductedDisappearingLetter".Translate(interactor.Named("PAWN"));
		Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatBig, interactor);
	}

	private static void SendTriggeredLetter(Pawn interactor)
	{
		TaggedString label = "ObeliskAbductedLetterLabel".Translate(interactor.Named("PAWN"));
		TaggedString text = "ObeliskAbductedLetter".Translate(interactor.Named("PAWN"));
		Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, interactor);
	}
}
