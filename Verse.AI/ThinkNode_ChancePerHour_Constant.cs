namespace Verse.AI
{
	public class ThinkNode_ChancePerHour_Constant : ThinkNode_ChancePerHour
	{
		private float mtbHours = -1f;

		private float mtbDays = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ChancePerHour_Constant obj = (ThinkNode_ChancePerHour_Constant)base.DeepCopy(resolve);
			obj.mtbHours = mtbHours;
			obj.mtbDays = mtbDays;
			return obj;
		}

		protected override float MtbHours(Pawn Pawn)
		{
			if (mtbDays > 0f)
			{
				return mtbDays * 24f;
			}
			return mtbHours;
		}
	}
}
