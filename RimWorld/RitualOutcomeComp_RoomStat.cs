using System;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_RoomStat : RitualOutcomeComp_Quality
{
	public RoomStatDef statDef;

	public override bool DataRequired => true;

	public override RitualOutcomeComp_Data MakeData()
	{
		return new RitualOutcomeComp_DataRoomStatCached();
	}

	public override void Tick(LordJob_Ritual ritual, RitualOutcomeComp_Data data, float progressAmount)
	{
		base.Tick(ritual, data, progressAmount);
		RitualOutcomeComp_DataRoomStatCached ritualOutcomeComp_DataRoomStatCached = (RitualOutcomeComp_DataRoomStatCached)data;
		if (ritualOutcomeComp_DataRoomStatCached != null && !ritualOutcomeComp_DataRoomStatCached.startingVal.HasValue)
		{
			Room room = ritual.Spot.GetRoom(ritual.Map);
			if (room != null)
			{
				ritualOutcomeComp_DataRoomStatCached.startingVal = PostProcessedRoomStat(room);
			}
		}
	}

	private float PostProcessedRoomStat(Room room)
	{
		return GenMath.RoundTo(Math.Min(room.GetStat(statDef), curve.Points[curve.PointsCount - 1].x), 0.1f);
	}

	public override bool Applies(LordJob_Ritual ritual)
	{
		return ritual.Spot.GetRoom(ritual.Map) != null;
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		RitualOutcomeComp_DataRoomStatCached ritualOutcomeComp_DataRoomStatCached = (RitualOutcomeComp_DataRoomStatCached)data;
		if (ritualOutcomeComp_DataRoomStatCached != null && ritualOutcomeComp_DataRoomStatCached.startingVal.HasValue)
		{
			return ritualOutcomeComp_DataRoomStatCached.startingVal.Value;
		}
		Room room = ritual.Spot.GetRoom(ritual.Map);
		if (room != null)
		{
			return PostProcessedRoomStat(room);
		}
		return 0f;
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		float x = 0f;
		if (ritualTarget.Map != null)
		{
			Room room = ritualTarget.Cell.GetRoom(ritualTarget.Map);
			RitualOutcomeComp_DataRoomStatCached ritualOutcomeComp_DataRoomStatCached = (RitualOutcomeComp_DataRoomStatCached)data;
			x = ((room == null) ? 0f : (ritualOutcomeComp_DataRoomStatCached?.startingVal ?? PostProcessedRoomStat(room)));
			if (room != null)
			{
				_ = !room.PsychologicallyOutdoors;
			}
			else
				_ = 0;
		}
		float num = curve.Evaluate(x);
		return new QualityFactor
		{
			label = label.CapitalizeFirst(),
			count = x + " / " + base.MaxValue,
			qualityChange = ExpectedOffsetDesc(positive: true, num),
			quality = num,
			positive = (num > 0f),
			priority = 0f
		};
	}
}
