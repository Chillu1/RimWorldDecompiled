using System;
using UnityEngine;

namespace Verse
{
	public class Command_Action : Command
	{
		public Action action;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			action();
		}
	}
}
