namespace GrznarAi.Web.Core.Options
{
    /// <summary>
    /// Settings for Twitter integration
    /// </summary>
    public class TwitterSettings
    {
        /// <summary>
        /// Twitter API key (Consumer Key)
        /// </summary>
        public string ApiKey { get; set; }
        
        /// <summary>
        /// Twitter API secret key (Consumer Secret)
        /// </summary>
        public string ApiKeySecret { get; set; }
        
        /// <summary>
        /// Twitter Access Token
        /// </summary>
        public string AccessToken { get; set; }
        
        /// <summary>
        /// Twitter Access Token Secret
        /// </summary>
        public string AccessTokenSecret { get; set; }
        
        /// <summary>
        /// URL to AI News section of the website
        /// </summary>
        public string AiNewsUrl { get; set; }
        
        /// <summary>
        /// Path to AI News image to be shared on Twitter
        /// </summary>
        public string AiNewsImagePath { get; set; }
    }
} 