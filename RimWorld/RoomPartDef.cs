using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomPartDef : Def
{
	protected Type workerClass;

	private RoomPartWorker cachedWorker;

	public RoomPartWorker Worker => cachedWorker ?? (cachedWorker = (RoomPartWorker)Activator.CreateInstance(workerClass, this));

	public override IEnumerable<string> ConfigErrors()
	{
		if (workerClass == null || !workerClass.IsSubclassOf(typeof(RoomPartWorker)))
		{
			yield return "workerClass must be a subclass of RoomPartWorker";
		}
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
	}
}
