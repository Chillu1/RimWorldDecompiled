using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class QuestPart_BestowingCeremony : QuestPart_MakeLord
{
	public const int PreferredDistanceFromThrone = 3;

	public Pawn bestower;

	public Pawn target;

	public Thing shuttle;

	public string questTag;

	public override bool QuestPartReserves(Pawn p)
	{
		if (p != bestower)
		{
			return p == target;
		}
		return true;
	}

	public static bool TryGetCeremonySpot(Pawn pawn, Faction bestowingFaction, out LocalTargetInfo spot, out IntVec3 absoluteSpot)
	{
		Building_Throne throne;
		Room throneRoom;
		if (pawn != null)
		{
			RoyalTitleDef titleAwardedWhenUpdating = pawn.royalty.GetTitleAwardedWhenUpdating(bestowingFaction, pawn.royalty.GetFavor(bestowingFaction));
			if (titleAwardedWhenUpdating != null && titleAwardedWhenUpdating.throneRoomRequirements != null && pawn.ownership.AssignedThrone != null)
			{
				throne = pawn.ownership.AssignedThrone;
				throneRoom = throne.GetRoom();
				spot = throne;
				IntVec3 facingCell = spot.Thing.Rotation.FacingCell;
				absoluteSpot = spot.Thing.InteractionCell + facingCell * 3;
				bool flag = false;
				for (int i = 0; i < 3; i++)
				{
					if (ValidateSpot(absoluteSpot))
					{
						flag = true;
						break;
					}
					absoluteSpot -= facingCell;
				}
				if (flag)
				{
					return true;
				}
				absoluteSpot = spot.Thing.InteractionCell - facingCell * 3;
				for (int j = 0; j < 3; j++)
				{
					if (ValidateSpot(absoluteSpot))
					{
						flag = true;
						break;
					}
					absoluteSpot += facingCell;
				}
				if (flag)
				{
					return true;
				}
				if (throneRoom != null && throneRoom.Cells.Where((IntVec3 c) => ValidateSpot(c)).TryRandomElementByWeight((IntVec3 c) => c.DistanceTo(throne.Position), out absoluteSpot))
				{
					return true;
				}
			}
			if (pawn.Map != null && pawn.Map.IsPlayerHome && (RCellFinder.TryFindGatheringSpot(pawn, GatheringDefOf.Party, ignoreRequiredColonistCount: true, out var result) || RCellFinder.TryFindRandomSpotJustOutsideColony(pawn.Position, pawn.Map, out result)))
			{
				spot = (absoluteSpot = result);
				return true;
			}
		}
		spot = LocalTargetInfo.Invalid;
		absoluteSpot = IntVec3.Invalid;
		return false;
		bool ValidateSpot(IntVec3 s)
		{
			if (!s.InBounds(throne.Map))
			{
				return false;
			}
			if (!s.Standable(throne.Map))
			{
				return false;
			}
			if (s.GetRoom(throne.Map) != throneRoom)
			{
				return false;
			}
			bool flag2 = false;
			for (int k = 0; k < 4; k++)
			{
				if ((s + GenAdj.CardinalDirections[k]).Standable(pawn.Map))
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				return false;
			}
			return true;
		}
	}

	protected override Lord MakeLord()
	{
		if (!TryGetCeremonySpot(target, bestower.Faction, out var spot, out var absoluteSpot))
		{
			Log.Error("Cannot find ceremony spot for bestowing ceremony!");
			return null;
		}
		Lord lord = LordMaker.MakeNewLord(faction, new LordJob_BestowingCeremony(bestower, target, spot, absoluteSpot, shuttle, questTag + ".QuestEnded"), base.Map);
		QuestUtility.AddQuestTag(ref lord.questTags, questTag);
		return lord;
	}

	public override void Cleanup()
	{
		Find.SignalManager.SendSignal(new Signal(questTag + ".QuestEnded", quest.Named("SUBJECT")));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref bestower, "bestower");
		Scribe_References.Look(ref target, "target");
		Scribe_References.Look(ref shuttle, "shuttle");
		Scribe_Values.Look(ref questTag, "questTag");
	}
}
