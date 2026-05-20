using System;
using System.Collections.Generic;

namespace Verse;

public class DeathActionProperties
{
	public Type workerClass = typeof(DeathActionWorker_Simple);

	private DeathActionWorker workerInt;

	public DeathActionWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = Activator.CreateInstance(workerClass) as DeathActionWorker;
				workerInt.props = this;
			}
			return workerInt;
		}
	}

	public virtual IEnumerable<string> ConfigErrors()
	{
		yield break;
	}
}
