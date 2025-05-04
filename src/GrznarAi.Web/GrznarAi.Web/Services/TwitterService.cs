using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrznarAi.Web.Core.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Service for posting messages on Twitter using RestSharp
    /// </summary>
    public class TwitterService : ITwitterService
    {
        private readonly TwitterSettings _twitterSettings;
        private readonly ILogger<TwitterService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ILocalizationService _localizationService;
        private readonly RestClient _client;
        private readonly RestClient _uploadClient;

        public TwitterService(
            IOptions<TwitterSettings> twitterSettings,
            ILogger<TwitterService> logger,
            IWebHostEnvironment environment,
            ILocalizationService localizationService)
        {
            _twitterSettings = twitterSettings.Value;
            _logger = logger;
            _environment = environment;
            _localizationService = localizationService;
            
            // Create OAuth authenticator for Twitter API
            var authenticator = OAuth1Authenticator.ForProtectedResource(
                _twitterSettings.ApiKey,
                _twitterSettings.ApiKeySecret,
                _twitterSettings.AccessToken,
                _twitterSettings.AccessTokenSecret
            );
            
            // Create clients with authenticator
            _client = new RestClient(new RestClientOptions("https://api.twitter.com")
            {
                Authenticator = authenticator
            });
            
            _uploadClient = new RestClient(new RestClientOptions("https://upload.twitter.com")
            {
                Authenticator = authenticator
            });
        }

        /// <summary>
        /// Post a tweet with text only
        /// </summary>
        /// <param name="message">Message to post</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> PostTweetAsync(string message)
        {
            try
            {
                _logger.LogInformation("Preparing to post tweet: {Message}", message);
                
                var request = new RestRequest("/2/tweets", Method.Post);
                request.AddJsonBody(new { text = message });
                
                // Execute the request
                _logger.LogInformation("Sending request to Twitter API: POST tweets");
                var response = await _client.ExecuteAsync(request);
                
                _logger.LogInformation("Twitter API response: {Status} {Content}", 
                    response.StatusCode, response.Content);
                
                if (response.IsSuccessful)
                {
                    _logger.LogInformation("Tweet posted successfully!");
                    return true;
                }
                else
                {
                    _logger.LogError("Error posting tweet. Status: {Status}, Response: {Response}", 
                        response.StatusCode, response.Content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting tweet: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Post a tweet with text and image
        /// </summary>
        /// <param name="message">Message to post</param>
        /// <param name="imagePath">Path to image (relative or absolute)</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> PostTweetWithImageAsync(string message, string imagePath)
        {
            try
            {
                // Resolve the image path
                var fullImagePath = ResolveImagePath(imagePath);
                
                if (!File.Exists(fullImagePath))
                {
                    _logger.LogError("Image file not found: {ImagePath}", fullImagePath);
                    return false;
                }
                
                // First, upload the image to Twitter
                string mediaId = await UploadImageToTwitterAsync(fullImagePath);
                
                if (string.IsNullOrEmpty(mediaId))
                {
                    _logger.LogError("Failed to upload image to Twitter");
                    return false;
                }
                
                // Create the request to post tweet with media
                var request = new RestRequest("/2/tweets", Method.Post);
                
                // Add JSON body with media
                request.AddJsonBody(new
                {
                    text = message,
                    media = new
                    {
                        media_ids = new[] { mediaId }
                    }
                });
                
                // Execute the request
                _logger.LogInformation("Sending tweet with image");
                var response = await _client.ExecuteAsync(request);
                
                if (response.IsSuccessful)
                {
                    _logger.LogInformation("Tweet with image posted successfully: {Response}", response.Content);
                    return true;
                }
                else
                {
                    _logger.LogError("Error posting tweet with image. Status: {Status}, Response: {Response}",
                        response.StatusCode, response.Content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting tweet with image: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Post a tweet about new AI news
        /// </summary>
        /// <param name="newItemsCount">Number of new news items</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> PostNewAiNewsAnnouncementAsync(int newItemsCount)
        {
            try
            {
                if (newItemsCount <= 0)
                {
                    _logger.LogInformation("No new AI news items to announce");
                    return true; // Not an error, just nothing to post
                }
                
                // Build the message
                var messageCz = $"Máme {newItemsCount} nových AI novinek! Podívejte se na {_twitterSettings.AiNewsUrl} #AI #ArtificialIntelligence #News";
                var messageEn = $"We have {newItemsCount} new AI news articles! Check them out at {_twitterSettings.AiNewsUrl} #AI #ArtificialIntelligence #News";

                // Decide which language to use based on current culture
                string message = messageCz;

                // If image path is specified, try to post with image
                if (!string.IsNullOrEmpty(_twitterSettings.AiNewsImagePath))
                {
                    try
                    {
                        return await PostTweetWithImageAsync(message, _twitterSettings.AiNewsImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error posting AI news with image, falling back to text-only: {Message}", ex.Message);
                        // Fall back to text-only tweet
                    }
                }
                
                // Post text-only tweet
                return await PostTweetAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting AI news announcement: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Post a tweet about new blog post
        /// </summary>
        /// <param name="blogPostTitle">Blog post title</param>
        /// <param name="blogPostUrl">Blog post URL</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> PostNewBlogPostAnnouncementAsync(string blogPostTitle, string blogPostUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(blogPostTitle) || string.IsNullOrEmpty(blogPostUrl))
                {
                    _logger.LogError("Blog post title or URL is missing");
                    return false;
                }
                
                // Build the message
                var messageCz = $"Nový článek na blogu: {blogPostTitle} - {blogPostUrl} #Blog";
                var messageEn = $"New blog post: {blogPostTitle} - {blogPostUrl} #Blog";

                // Decide which language to use based on current culture
                string message = messageCz;
                
                // Post a simple tweet without image
                return await PostTweetAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting blog post announcement: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Upload an image to Twitter and return the media ID
        /// </summary>
        private async Task<string> UploadImageToTwitterAsync(string imagePath)
        {
            try
            {
                var request = new RestRequest("/1.1/media/upload.json", Method.Post);
                request.AlwaysMultipartFormData = true;
                request.AddFile("media", imagePath);
                
                // Execute the request
                _logger.LogInformation("Uploading image to Twitter: {ImagePath}", Path.GetFileName(imagePath));
                var response = await _uploadClient.ExecuteAsync(request);
                
                _logger.LogInformation("Upload response: {Status} {Content}", 
                    response.StatusCode, response.Content);
                
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    // Parse the response to get the media ID
                    var responseJson = JsonDocument.Parse(response.Content);
                    
                    if (responseJson.RootElement.TryGetProperty("media_id_string", out var mediaIdElement))
                    {
                        string mediaId = mediaIdElement.GetString();
                        _logger.LogInformation("Successfully uploaded image, media ID: {MediaId}", mediaId);
                        return mediaId;
                    }
                    else
                    {
                        _logger.LogError("Media ID not found in response");
                        return null;
                    }
                }
                else
                {
                    _logger.LogError("Error uploading image. Status: {Status}, Response: {Response}",
                        response.StatusCode, response.Content);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Resolves an image path to an absolute path
        /// </summary>
        private string ResolveImagePath(string imagePath)
        {
            if (Path.IsPathRooted(imagePath))
            {
                return imagePath;
            }
            
            return Path.Combine(_environment.ContentRootPath, imagePath);
        }
    }
} 