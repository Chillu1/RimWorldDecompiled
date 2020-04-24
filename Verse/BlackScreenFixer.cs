using UnityEngine;

namespace Verse
{
	internal class BlackScreenFixer : MonoBehaviour
	{
		private void Start()
		{
			if (Screen.width != 0 && Screen.height != 0)
			{
				Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);
			}
		}
	}
}
