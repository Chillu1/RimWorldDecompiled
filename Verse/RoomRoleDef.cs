using System;
using System.Collections.Generic;

namespace Verse
{
	public class RoomRoleDef : Def
	{
		public Type workerClass;

		private List<RoomStatDef> relatedStats;

		[Unsaved(false)]
		private RoomRoleWorker workerInt;

		public RoomRoleWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (RoomRoleWorker)Activator.CreateInstance(workerClass);
				}
				return workerInt;
			}
		}

		public bool IsStatRelated(RoomStatDef def)
		{
			if (relatedStats == null)
			{
				return false;
			}
			for (int i = 0; i < relatedStats.Count; i++)
			{
				if (relatedStats[i] == def)
				{
					return true;
				}
			}
			return false;
		}
	}
}
