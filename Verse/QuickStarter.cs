using UnityEngine.SceneManagement;

namespace Verse
{
	public static class QuickStarter
	{
		private static bool quickStarted;

		public static bool CheckQuickStart()
		{
			if (GenCommandLine.CommandLineArgPassed("quicktest") && !quickStarted && GenScene.InEntryScene)
			{
				quickStarted = true;
				SceneManager.LoadScene("Play");
				return true;
			}
			return false;
		}
	}
}
