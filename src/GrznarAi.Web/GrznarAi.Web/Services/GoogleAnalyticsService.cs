using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    /// <summary>
    /// Service for interacting with Google Analytics in a Blazor application
    /// </summary>
    public class GoogleAnalyticsService
    {
        private readonly IJSRuntime _jsRuntime;

        public GoogleAnalyticsService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Track a page view in Google Analytics
        /// </summary>
        /// <param name="pagePath">The page path to track</param>
        /// <param name="pageTitle">The page title</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task TrackPageViewAsync(string pagePath, string pageTitle)
        {
            // Volání JS funkce pro sledování stránky
            await _jsRuntime.InvokeVoidAsync("trackPage", pagePath, pageTitle);
        }

        /// <summary>
        /// Ensure SPA tracking is initialized
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task EnsureSpaTrackingInitializedAsync()
        {
            await _jsRuntime.InvokeVoidAsync("analyticsHelpers.initSpaTracking");
        }

        /// <summary>
        /// Track a custom event in Google Analytics
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="eventParams">Event parameters</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task TrackEventAsync(string eventName, object eventParams)
        {
            await _jsRuntime.InvokeVoidAsync("gtag", "event", eventName, eventParams);
        }

        /// <summary>
        /// Track button click event
        /// </summary>
        /// <param name="buttonText">Text of the button</param>
        /// <param name="category">Category of the action</param>
        /// <param name="location">Location where action occurred</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task TrackButtonClickAsync(string buttonText, string category, string location)
        {
            await TrackEventAsync("button_click", new 
            { 
                button_text = buttonText, 
                category, 
                location 
            });
        }

        /// <summary>
        /// Track content selection (e.g. tab, dropdown item) using the helper JavaScript function
        /// </summary>
        /// <param name="contentName">Name of the selected content</param>
        /// <param name="contentType">Type of content (tab, dropdown, etc.)</param>
        /// <param name="location">Location where action occurred</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task TrackContentSelectionAsync(string contentName, string contentType, string location)
        {
            await _jsRuntime.InvokeVoidAsync("analyticsHelpers.trackContentSelection", contentName, contentType, location);
        }

        /// <summary>
        /// Track search action
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="searchType">Type of search (blog, site, etc.)</param>
        /// <param name="resultsCount">Number of results found</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task TrackSearchAsync(string searchTerm, string searchType, int resultsCount)
        {
            await TrackEventAsync("search", new 
            { 
                search_term = searchTerm, 
                search_type = searchType, 
                results_count = resultsCount 
            });
        }

        /// <summary>
        /// Track download event
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileType">Type of file (PDF, ZIP, etc.)</param>
        /// <param name="location">Location where download initiated</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task TrackDownloadAsync(string fileName, string fileType, string location)
        {
            await TrackEventAsync("file_download", new 
            { 
                file_name = fileName, 
                file_type = fileType, 
                location 
            });
        }

        /// <summary>
        /// Track external link click using the helper JavaScript function
        /// </summary>
        /// <param name="linkUrl">URL of the link</param>
        /// <param name="linkText">Text of the link</param>
        /// <param name="location">Location where link was clicked</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task TrackExternalLinkAsync(string linkUrl, string linkText, string location)
        {
            await _jsRuntime.InvokeVoidAsync("analyticsHelpers.trackExternalLink", linkUrl, linkText, location);
        }

        /// <summary>
        /// Track form submission
        /// </summary>
        /// <param name="formName">Name of the form</param>
        /// <param name="formType">Type of form (contact, login, etc.)</param>
        /// <param name="success">Whether submission was successful</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task TrackFormSubmissionAsync(string formName, string formType, bool success)
        {
            await TrackEventAsync("form_submission", new 
            { 
                form_name = formName, 
                form_type = formType, 
                success 
            });
        }
    }
} 