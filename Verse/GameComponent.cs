namespace Verse
{
	public abstract class GameComponent : IExposable
	{
		public virtual void GameComponentUpdate()
		{
		}

		public virtual void GameComponentTick()
		{
		}

		public virtual void GameComponentOnGUI()
		{
		}

		public virtual void ExposeData()
		{
		}

		public virtual void FinalizeInit()
		{
		}

		public virtual void StartedNewGame()
		{
		}

		public virtual void LoadedGame()
		{
		}
	}
}
