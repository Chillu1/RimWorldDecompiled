using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

public class LayoutWorkerComplex_Mechanitor : LayoutWorkerComplex
{
	private const string MechanitorCasketOpenedSignal = "MechanitorCasketOpened";

	private const string MechanitorCasketUnfoggedSignal = "MechanitorCasketUnfogged";

	private static readonly IntRange RandomMechanitorCorpseAge = new IntRange(0, 360000000);

	private readonly List<CellRect> tmpAllRoomRects = new List<CellRect>();

	public LayoutWorkerComplex_Mechanitor(LayoutDef def)
		: base(def)
	{
	}

	public override Faction GetFixedHostileFactionForThreats()
	{
		return Faction.OfMechanoids;
	}

	protected override void PreSpawnThreats(List<LayoutRoom> rooms, Map map, List<Thing> allSpawnedThings)
	{
		base.PreSpawnThreats(rooms, map, allSpawnedThings);
		tmpAllRoomRects.Clear();
		tmpAllRoomRects.AddRange(rooms.Where((LayoutRoom r) => r.requiredDef == null).SelectMany((LayoutRoom r) => r.rects));
		CellRect bounds = tmpAllRoomRects[0];
		for (int num = 0; num < tmpAllRoomRects.Count; num++)
		{
			bounds = bounds.Encapsulate(tmpAllRoomRects[num]);
		}
		bool flag = false;
		foreach (CellRect item in tmpAllRoomRects.OrderBy(OrderRoomsBy))
		{
			if (TryPlaceDeceasedMechanitor(item, map, out var casket))
			{
				allSpawnedThings.Add(casket);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Log.Error("Failed to place mechanitor in ancient mechanitor complex.");
		}
		tmpAllRoomRects.Clear();
		float OrderRoomsBy(CellRect r)
		{
			if (r.Contains(bounds.CenterCell))
			{
				return 0f;
			}
			return r.CenterCell.DistanceTo(bounds.CenterCell);
		}
	}

	private bool TryPlaceDeceasedMechanitor(CellRect room, Map map, out Building_AncientCryptosleepPod casket)
	{
		foreach (IntVec3 item in room.Cells.InRandomOrder())
		{
			if (CanPlaceCasketAt(item))
			{
				casket = (Building_AncientCryptosleepPod)GenSpawn.Spawn(ThingDefOf.AncientCryptosleepPod, item, map);
				casket.openedSignal = "MechanitorCasketOpened" + Find.UniqueIDsManager.GetNextSignalTagID();
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Mechanitor_Basic, Faction.OfAncients, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: true, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: true));
				pawn.Corpse.Age = RandomMechanitorCorpseAge.RandomInRange;
				pawn.relations.hidePawnRelations = true;
				pawn.Corpse.GetComp<CompRottable>().RotProgress += pawn.Corpse.Age;
				casket.TryAcceptThing(pawn.Corpse, allowSpecialEffects: false);
				SignalAction_Message obj = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
				obj.signalTag = casket.openedSignal;
				obj.message = "MessageMechanitorCasketOpened".Translate(pawn, HediffDefOf.MechlinkImplant);
				obj.messageType = MessageTypeDefOf.PositiveEvent;
				obj.lookTargets = pawn.Corpse;
				GenSpawn.Spawn(obj, item, map);
				SignalAction_Letter obj2 = (SignalAction_Letter)ThingMaker.MakeThing(ThingDefOf.SignalAction_Letter);
				obj2.signalTag = casket.openedSignal;
				obj2.letterDef = LetterDefOf.NeutralEvent;
				obj2.letterLabelKey = "LetterLabelMechanitorCasketOpened";
				obj2.letterMessageKey = "LetterMechanitorCasketOpened";
				obj2.fixedPawnReference = pawn;
				obj2.lookTargets = pawn.Corpse;
				GenSpawn.Spawn(obj2, item, map);
				SignalAction_SoundOneShot obj3 = (SignalAction_SoundOneShot)ThingMaker.MakeThing(ThingDefOf.SignalAction_SoundOneShot);
				obj3.signalTag = casket.openedSignal;
				obj3.sound = SoundDefOf.MechlinkCorpseReveal;
				GenSpawn.Spawn(obj3, item, map);
				TriggerUnfogged triggerUnfogged = (TriggerUnfogged)ThingMaker.MakeThing(ThingDefOf.TriggerUnfogged);
				triggerUnfogged.signalTag = "MechanitorCasketUnfogged" + Find.UniqueIDsManager.GetNextSignalTagID();
				GenSpawn.Spawn(triggerUnfogged, casket.Position, map);
				SignalAction_Letter obj4 = (SignalAction_Letter)ThingMaker.MakeThing(ThingDefOf.SignalAction_Letter);
				obj4.signalTag = triggerUnfogged.signalTag;
				obj4.letterDef = LetterDefOf.NeutralEvent;
				obj4.letterLabelKey = "LetterLabelMechanitorCasketFound";
				obj4.letterMessageKey = "LetterMechanitorCasketFound";
				GenSpawn.Spawn(obj4, item, map);
				ScatterDebrisUtility.ScatterFilthAroundThing(casket, map, ThingDefOf.Filth_MachineBits);
				return true;
			}
		}
		casket = null;
		return false;
		bool CanPlaceCasketAt(IntVec3 cell)
		{
			foreach (IntVec3 item2 in GenAdj.OccupiedRect(cell, Rot4.North, ThingDefOf.AncientCryptosleepPod.Size).ExpandedBy(1))
			{
				if (item2.GetEdifice(map) != null)
				{
					return false;
				}
			}
			return true;
		}
	}
}
