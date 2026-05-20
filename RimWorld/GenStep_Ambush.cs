using Verse;

namespace RimWorld;

public abstract class GenStep_Ambush : GenStep
{
	public FloatRange defaultPointsRange = new FloatRange(180f, 340f);

	public override void Generate(Map map, GenStepParams parms)
	{
		if (SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out var rectToDefend, out var singleCellToSpawnNear, map))
		{
			SpawnTrigger(rectToDefend, singleCellToSpawnNear, map, parms);
		}
	}

	private void SpawnTrigger(CellRect rectToDefend, IntVec3 root, Map map, GenStepParams parms)
	{
		string signalTag = "ambushActivated-" + Find.UniqueIDsManager.GetNextSignalTagID();
		CellRect rect = ((!root.IsValid) ? rectToDefend.ExpandedBy(12) : CellRect.CenteredOn(root, 17));
		SignalAction_Ambush signalAction_Ambush = MakeAmbushSignalAction(rectToDefend, root, parms);
		signalAction_Ambush.signalTag = signalTag;
		GenSpawn.Spawn(signalAction_Ambush, rect.CenterCell, map);
		RectTrigger rectTrigger = MakeRectTrigger();
		rectTrigger.signalTag = signalTag;
		rectTrigger.Rect = rect;
		GenSpawn.Spawn(rectTrigger, rect.CenterCell, map);
		TriggerUnfogged obj = (TriggerUnfogged)ThingMaker.MakeThing(ThingDefOf.TriggerUnfogged);
		obj.signalTag = signalTag;
		GenSpawn.Spawn(obj, rect.CenterCell, map);
	}

	protected virtual RectTrigger MakeRectTrigger()
	{
		return (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
	}

	protected virtual SignalAction_Ambush MakeAmbushSignalAction(CellRect rectToDefend, IntVec3 root, GenStepParams parms)
	{
		SignalAction_Ambush signalAction_Ambush = (SignalAction_Ambush)ThingMaker.MakeThing(ThingDefOf.SignalAction_Ambush);
		if (parms.sitePart != null)
		{
			signalAction_Ambush.points = parms.sitePart.parms.threatPoints;
		}
		else
		{
			signalAction_Ambush.points = defaultPointsRange.RandomInRange;
		}
		int num = Rand.RangeInclusive(0, 2);
		if (num == 0)
		{
			signalAction_Ambush.ambushType = SignalActionAmbushType.Manhunters;
		}
		else if (num == 1 && PawnGroupMakerUtility.CanGenerateAnyNormalGroup(Faction.OfMechanoids, signalAction_Ambush.points))
		{
			signalAction_Ambush.ambushType = SignalActionAmbushType.Mechanoids;
		}
		else
		{
			signalAction_Ambush.ambushType = SignalActionAmbushType.Normal;
		}
		return signalAction_Ambush;
	}
}
