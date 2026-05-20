using System.Runtime.CompilerServices;
using Verse.AI;

namespace Verse
{
	public static class ToilMaker
	{
		private const int MaxToilPoolSize = 5000;

		public static Toil MakeToil([CallerMemberName] string debugName = null)
		{
			Toil toil = SimplePool<Toil>.Get();
			if (!toil.inPool)
			{
				Log.Error("Toil was marked as not in pool while coming from pool");
			}
			toil.inPool = false;
			toil.debugName = debugName;
			return toil;
		}

		public static void ReturnToPool(this Toil toil)
		{
			toil.Clear();
			if (toil.inPool)
			{
				Log.Error("Toil was marked as already being in the pool while being returned to pool");
			}
			else if (SimplePool<Toil>.FreeItemsCount < 5000)
			{
				toil.inPool = true;
				SimplePool<Toil>.Return(toil);
			}
		}
	}
}
