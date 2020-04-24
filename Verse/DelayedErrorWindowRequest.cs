using System.Collections.Generic;

namespace Verse
{
	public static class DelayedErrorWindowRequest
	{
		private struct Request
		{
			public string text;

			public string title;
		}

		private static List<Request> requests = new List<Request>();

		public static void DelayedErrorWindowRequestOnGUI()
		{
			try
			{
				for (int i = 0; i < requests.Count; i++)
				{
					Find.WindowStack.Add(new Dialog_MessageBox(requests[i].text, "OK".Translate(), null, null, null, requests[i].title));
				}
			}
			finally
			{
				requests.Clear();
			}
		}

		public static void Add(string text, string title = null)
		{
			Request item = default(Request);
			item.text = text;
			item.title = title;
			requests.Add(item);
		}
	}
}
