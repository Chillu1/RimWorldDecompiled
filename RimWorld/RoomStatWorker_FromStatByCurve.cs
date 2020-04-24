using Verse;

namespace RimWorld
{
	public class RoomStatWorker_FromStatByCurve : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			return def.curve.Evaluate(room.GetStat(def.inputStat));
		}
	}
}
