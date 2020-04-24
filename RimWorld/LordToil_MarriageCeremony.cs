using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_MarriageCeremony : LordToil
	{
		private Pawn firstPawn;

		private Pawn secondPawn;

		private IntVec3 spot;

		public static readonly IntVec3 OtherFianceNoMarriageSpotCellOffset = new IntVec3(-1, 0, 0);

		public LordToilData_MarriageCeremony Data => (LordToilData_MarriageCeremony)data;

		public LordToil_MarriageCeremony(Pawn firstPawn, Pawn secondPawn, IntVec3 spot)
		{
			this.firstPawn = firstPawn;
			this.secondPawn = secondPawn;
			this.spot = spot;
			data = new LordToilData_MarriageCeremony();
		}

		public override void Init()
		{
			base.Init();
			Data.spectateRect = CalculateSpectateRect();
			SpectateRectSide allowedSides = SpectateRectSide.All;
			if (Data.spectateRect.Width > Data.spectateRect.Height)
			{
				allowedSides = SpectateRectSide.Vertical;
			}
			else if (Data.spectateRect.Height > Data.spectateRect.Width)
			{
				allowedSides = SpectateRectSide.Horizontal;
			}
			Data.spectateRectAllowedSides = SpectatorCellFinder.FindSingleBestSide(Data.spectateRect, base.Map, allowedSides);
		}

		public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
		{
			if (IsFiance(p))
			{
				return DutyDefOf.MarryPawn.hook;
			}
			return DutyDefOf.Spectate.hook;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (IsFiance(pawn))
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.MarryPawn, FianceStandingSpotFor(pawn));
					continue;
				}
				PawnDuty pawnDuty = new PawnDuty(DutyDefOf.Spectate);
				pawnDuty.spectateRect = Data.spectateRect;
				pawnDuty.spectateRectAllowedSides = Data.spectateRectAllowedSides;
				pawn.mindState.duty = pawnDuty;
			}
		}

		private bool IsFiance(Pawn p)
		{
			if (p != firstPawn)
			{
				return p == secondPawn;
			}
			return true;
		}

		public IntVec3 FianceStandingSpotFor(Pawn pawn)
		{
			Pawn pawn2 = null;
			if (firstPawn == pawn)
			{
				pawn2 = secondPawn;
			}
			else
			{
				if (secondPawn != pawn)
				{
					Log.Warning("Called ExactStandingSpotFor but it's not this pawn's ceremony.");
					return IntVec3.Invalid;
				}
				pawn2 = firstPawn;
			}
			if (pawn.thingIDNumber < pawn2.thingIDNumber)
			{
				return spot;
			}
			if (GetMarriageSpotAt(spot) != null)
			{
				return FindCellForOtherPawnAtMarriageSpot(spot);
			}
			return spot + OtherFianceNoMarriageSpotCellOffset;
		}

		private Thing GetMarriageSpotAt(IntVec3 cell)
		{
			return cell.GetThingList(base.Map).Find((Thing x) => x.def == ThingDefOf.MarriageSpot);
		}

		private IntVec3 FindCellForOtherPawnAtMarriageSpot(IntVec3 cell)
		{
			CellRect cellRect = GetMarriageSpotAt(cell).OccupiedRect();
			for (int i = cellRect.minX; i <= cellRect.maxX; i++)
			{
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					if (cell.x != i || cell.z != j)
					{
						return new IntVec3(i, 0, j);
					}
				}
			}
			Log.Warning("Marriage spot is 1x1. There's no place for 2 pawns.");
			return IntVec3.Invalid;
		}

		private CellRect CalculateSpectateRect()
		{
			IntVec3 first = FianceStandingSpotFor(firstPawn);
			IntVec3 second = FianceStandingSpotFor(secondPawn);
			return CellRect.FromLimits(first, second);
		}
	}
}
