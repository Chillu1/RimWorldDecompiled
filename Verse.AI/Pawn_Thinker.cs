namespace Verse.AI
{
	public class Pawn_Thinker
	{
		public Pawn pawn;

		public ThinkTreeDef MainThinkTree => pawn.RaceProps.thinkTreeMain;

		public ThinkNode MainThinkNodeRoot => pawn.RaceProps.thinkTreeMain.thinkRoot;

		public ThinkTreeDef ConstantThinkTree => pawn.RaceProps.thinkTreeConstant;

		public ThinkNode ConstantThinkNodeRoot => pawn.RaceProps.thinkTreeConstant.thinkRoot;

		public Pawn_Thinker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public T TryGetMainTreeThinkNode<T>() where T : ThinkNode
		{
			foreach (ThinkNode item in MainThinkNodeRoot.ChildrenRecursive)
			{
				T val = item as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		public T GetMainTreeThinkNode<T>() where T : ThinkNode
		{
			T val = TryGetMainTreeThinkNode<T>();
			if (val == null)
			{
				Log.Warning(pawn + " looked for ThinkNode of type " + typeof(T) + " and didn't find it.");
			}
			return val;
		}
	}
}
