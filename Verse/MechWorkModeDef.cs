using System;
using UnityEngine;

namespace Verse
{
	public class MechWorkModeDef : Def
	{
		[NoTranslate]
		public string iconPath;

		public Texture2D uiIcon;

		public int uiOrder;

		public bool ignoreGroupChargeLimits;

		public Type workerClass = typeof(WorkModeDrawer);

		[Unsaved(false)]
		private WorkModeDrawer workerInt;

		public WorkModeDrawer Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (WorkModeDrawer)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public override void PostLoad()
		{
			if (!string.IsNullOrEmpty(iconPath))
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					uiIcon = ContentFinder<Texture2D>.Get(iconPath);
				});
			}
		}
	}
}
