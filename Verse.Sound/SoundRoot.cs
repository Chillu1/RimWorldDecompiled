namespace Verse.Sound
{
	public class SoundRoot
	{
		public AudioSourcePool sourcePool;

		public SampleOneShotManager oneShotManager;

		public SustainerManager sustainerManager;

		public SoundRoot()
		{
			sourcePool = new AudioSourcePool();
			sustainerManager = new SustainerManager();
			oneShotManager = new SampleOneShotManager();
		}

		public void Update()
		{
			sustainerManager.SustainerManagerUpdate();
			oneShotManager.SampleOneShotManagerUpdate();
		}
	}
}
