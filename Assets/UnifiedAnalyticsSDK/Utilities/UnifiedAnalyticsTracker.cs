using System.Collections.Generic;
using Facebook.Unity;
using GameAnalyticsSDK;

namespace UnifiedAnalyticsSDK.Utilities
{
    /// <summary>
    /// The main event tracker, responsible for processing <see cref="GameAnalytics"/> and <see cref="Facebook"/> events.
    /// </summary>
    public class UnifiedAnalyticsTracker
    {
        /// <summary>
        /// Sends a facebook app event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventParameters"></param>
        public void SendFacebookAppEvent(string eventName, Dictionary<string, object> eventParameters = null)
        {
            FB.LogAppEvent(eventName, parameters:eventParameters);
        }

        /// <summary>
        /// Sends a level start event (Uses GameAnalytics events).
        /// </summary>
        /// <param name="levelName"></param>
        public void SendLevelStartEvent(string levelName)
        {
            this.SendProgressionEvent(GAProgressionStatus.Start, levelName);
        }

        /// <summary>
        /// Sends a level completed event with result if player won or lost (Uses GameAnalytics events).
        /// </summary>
        /// <param name="levelName"></param>
        /// <param name="playerWon"></param>
        public void SendLevelCompletedEvent(string levelName, bool playerWon)
        {
            this.SendProgressionEvent(GAProgressionStatus.Complete, levelName, new Dictionary<string, object>
            {
                ["result"] = $"{playerWon}"
            });
        }

        /// <summary>
        /// Sends a level failed event with the reason why it failed (i.e player cancelled level) (Uses GameAnalytics events).
        /// </summary>
        /// <param name="levelName"></param>
        /// <param name="reason"></param>
        public void SendLevelFailedEvent(string levelName, string reason)
        {
            this.SendProgressionEvent(GAProgressionStatus.Fail, levelName, new Dictionary<string, object>
            {
                ["reason"] = reason
            });
        }

        /// <summary>
        /// Sends a progression event (Uses GameAnalytics events).
        /// </summary>
        /// <param name="progressionStatus"></param>
        /// <param name="progressionName"></param>
        /// <param name="parameters"></param>
        public void SendProgressionEvent(GAProgressionStatus progressionStatus, 
            string progressionName, IDictionary<string, object> parameters = null)
        {
            if (parameters == null)
            {
                GameAnalytics.NewProgressionEvent(progressionStatus, progressionName);
                return;
            }

            GameAnalytics.NewProgressionEvent(progressionStatus, progressionName, parameters);
        }
    }
}