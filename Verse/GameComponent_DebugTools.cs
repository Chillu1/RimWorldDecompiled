using System;
using System.Collections.Generic;

namespace Verse
{
	public class GameComponent_DebugTools : GameComponent
	{
		private List<Func<bool>> callbacks = new List<Func<bool>>();

		public GameComponent_DebugTools(Game game)
		{
		}

		public override void GameComponentUpdate()
		{
			if (callbacks.Count > 0 && callbacks[0]())
			{
				callbacks.RemoveAt(0);
			}
		}

		public void AddPerFrameCallback(Func<bool> callback)
		{
			callbacks.Add(callback);
		}
	}
}
