using System.Collections.Generic;

namespace Verse
{
	public class FeedbackFloaters
	{
		protected List<FeedbackItem> feeders = new List<FeedbackItem>();

		public void AddFeedback(FeedbackItem newFeedback)
		{
			feeders.Add(newFeedback);
		}

		public void FeedbackUpdate()
		{
			for (int num = feeders.Count - 1; num >= 0; num--)
			{
				feeders[num].Update();
				if (feeders[num].TimeLeft <= 0f)
				{
					feeders.Remove(feeders[num]);
				}
			}
		}

		public void FeedbackOnGUI()
		{
			foreach (FeedbackItem feeder in feeders)
			{
				feeder.FeedbackOnGUI();
			}
		}
	}
}
