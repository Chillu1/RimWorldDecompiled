using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Need_RoomSize : Need_Seeker
	{
		private static List<Room> tempScanRooms = new List<Room>();

		private const float MinCramped = 0.01f;

		private const float MinNormal = 0.3f;

		private const float MinSpacious = 0.7f;

		public static readonly int SampleNumCells = GenRadial.NumCellsInRadius(7.9f);

		private static readonly SimpleCurve RoomCellCountSpaceCurve = new SimpleCurve
		{
			new CurvePoint(3f, 0f),
			new CurvePoint(9f, 0.25f),
			new CurvePoint(16f, 0.5f),
			new CurvePoint(42f, 0.71f),
			new CurvePoint(100f, 1f)
		};

		public override float CurInstantLevel => SpacePerceptibleNow();

		public RoomSizeCategory CurCategory
		{
			get
			{
				if (CurLevel < 0.01f)
				{
					return RoomSizeCategory.VeryCramped;
				}
				if (CurLevel < 0.3f)
				{
					return RoomSizeCategory.Cramped;
				}
				if (CurLevel < 0.7f)
				{
					return RoomSizeCategory.Normal;
				}
				return RoomSizeCategory.Spacious;
			}
		}

		public Need_RoomSize(Pawn pawn)
			: base(pawn)
		{
			threshPercents = new List<float>();
			threshPercents.Add(0.3f);
			threshPercents.Add(0.7f);
		}

		public float SpacePerceptibleNow()
		{
			if (!pawn.Spawned)
			{
				return 1f;
			}
			IntVec3 position = pawn.Position;
			tempScanRooms.Clear();
			for (int i = 0; i < 5; i++)
			{
				Room room = (position + GenRadial.RadialPattern[i]).GetRoom(pawn.Map);
				if (room != null)
				{
					if (i == 0 && room.PsychologicallyOutdoors)
					{
						return 1f;
					}
					if ((i == 0 || room.RegionType != RegionType.Portal) && !tempScanRooms.Contains(room))
					{
						tempScanRooms.Add(room);
					}
				}
			}
			float num = 0f;
			for (int j = 0; j < SampleNumCells; j++)
			{
				IntVec3 loc = position + GenRadial.RadialPattern[j];
				if (tempScanRooms.Contains(loc.GetRoom(pawn.Map)))
				{
					num += 1f;
				}
			}
			tempScanRooms.Clear();
			return RoomCellCountSpaceCurve.Evaluate(num);
		}
	}
}
