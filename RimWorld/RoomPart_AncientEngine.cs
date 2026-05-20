using System.Linq;
using Verse;

namespace RimWorld;

public class RoomPart_AncientEngine : RoomPartWorker
{
	public RoomPart_AncientEngine(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		if (ModsConfig.OdysseyActive)
		{
			CellRect cellRect = room.rects.Where((CellRect r) => r.Width >= 5 && r.Height >= 5).RandomElementByWeight((CellRect r) => r.Area);
			if (cellRect == default(CellRect))
			{
				Log.Error("Failed to place ancient grav engine.");
				return;
			}
			Thing engine = SpawnEngine(map, cellRect);
			SpawnSignal(map, room, engine);
		}
	}

	private static Thing SpawnEngine(Map map, CellRect largest)
	{
		IntVec3 zero = IntVec3.Zero;
		if (largest.Width % 2 == 0)
		{
			zero.x = 1;
		}
		if (largest.Height % 2 == 0)
		{
			zero.z = 1;
		}
		return GenSpawn.Spawn(ThingDefOf.AncientGravEngine, largest.CenterCell - zero, map);
	}

	private static void SpawnSignal(Map map, LayoutRoom room, Thing engine)
	{
		string signalTag = $"ThingDiscovered_{Find.UniqueIDsManager.GetNextSignalTagID()}";
		SignalAction_Letter signalAction_Letter = (SignalAction_Letter)ThingMaker.MakeThing(ThingDefOf.SignalAction_Letter);
		signalAction_Letter.signalTag = signalTag;
		signalAction_Letter.letterDef = LetterDefOf.NeutralEvent;
		signalAction_Letter.letterLabelKey = "LetterAncientGravEngineDiscoveredLabel";
		signalAction_Letter.letterMessageKey = "LetterAncientGravEngineDiscovered";
		signalAction_Letter.lookTargets = engine;
		signalAction_Letter.requireLookTargetsNotDestroyed = true;
		GenSpawn.Spawn(signalAction_Letter, engine.Position, map);
		room.SpawnRectTriggersForAction(signalAction_Letter, map);
	}
}
