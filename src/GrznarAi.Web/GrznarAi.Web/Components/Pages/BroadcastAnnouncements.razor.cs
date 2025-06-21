using Microsoft.AspNetCore.Components;
using GrznarAi.Web.Api.Models;
using GrznarAi.Web.Services;
using System.Text.RegularExpressions;

namespace GrznarAi.Web.Components.Pages
{
    public partial class BroadcastAnnouncements : ComponentBase
    {
        [Inject] private IBroadcastAnnouncementService BroadcastService { get; set; } = null!;
        [Inject] private ILocalizationService Localizer { get; set; } = null!;

        private bool isLoading = true;
        private string? errorMessage;
        private List<BroadcastAnnouncementResponse>? announcements;
        private PagedBroadcastAnnouncementResponse? pagedResponse;
        private int currentPage = 1;

        protected override async Task OnInitializedAsync()
        {
            await LoadPage(1);
        }

        private async Task LoadPage(int page)
        {
            try
            {
                isLoading = true;
                errorMessage = null;
                
                StateHasChanged();

                var response = await BroadcastService.GetPagedAnnouncementsAsync(page);

                if (response != null)
                {
                    pagedResponse = response;
                    announcements = response.Announcements;
                    currentPage = response.CurrentPage;
                }
                else
                {
                    announcements = new List<BroadcastAnnouncementResponse>();
                    pagedResponse = new PagedBroadcastAnnouncementResponse
                    {
                        Announcements = new List<BroadcastAnnouncementResponse>(),
                        TotalCount = 0,
                        CurrentPage = 1,
                        PageSize = 10,
                        TotalPages = 0,
                        HasNextPage = false,
                        HasPreviousPage = false
                    };
                }
            }
            catch (Exception ex)
            {
                errorMessage = Localizer.GetString("BroadcastAnnouncements.Error.General");
                Console.WriteLine($"Error loading broadcast announcements: {ex.Message}");
                
                // Set empty response on error
                pagedResponse = new PagedBroadcastAnnouncementResponse
                {
                    Announcements = new List<BroadcastAnnouncementResponse>(),
                    TotalCount = 0,
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                };
                announcements = new List<BroadcastAnnouncementResponse>();
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private string FormatAnnouncementContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            // Convert line breaks to HTML
            var formattedContent = content.Replace("\n", "<br>").Replace("\r\n", "<br>");
            
            // Make URLs clickable
            var urlPattern = @"(https?://[^\s]+)";
            formattedContent = Regex.Replace(formattedContent, urlPattern, 
                "<a href=\"$1\" target=\"_blank\" rel=\"noopener noreferrer\">$1</a>");

            // Make email addresses clickable
            var emailPattern = @"([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})";
            formattedContent = Regex.Replace(formattedContent, emailPattern, 
                "<a href=\"mailto:$1\">$1</a>");

            return formattedContent;
        }
    }
} 