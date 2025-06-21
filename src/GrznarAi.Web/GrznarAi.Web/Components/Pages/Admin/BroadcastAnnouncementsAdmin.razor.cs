using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using GrznarAi.Web.Api.Models;
using GrznarAi.Web.Services;

namespace GrznarAi.Web.Components.Pages.Admin
{
    public partial class BroadcastAnnouncementsAdmin : ComponentBase
    {
        [Inject] private IBroadcastAnnouncementService BroadcastService { get; set; } = null!;
        [Inject] private ILocalizationService Localizer { get; set; } = null!;
        private bool isLoading = true;
        private bool isDeleting = false;
        private string? errorMessage;
        private List<BroadcastAnnouncementResponse>? announcements;
        private PagedBroadcastAnnouncementResponse? pagedResponse;
        private int currentPage = 1;
        private string searchTerm = string.Empty;
        private string currentSearchTerm = string.Empty;

        // Modal states
        private BroadcastAnnouncementResponse? announcementToDelete;
        private bool showDeleteAllModal = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadPage(1);
        }

        private async Task LoadPage(int page, string? search = null)
        {
            try
            {
                isLoading = true;
                errorMessage = null;
                StateHasChanged();

                var searchQuery = search ?? currentSearchTerm;
                var response = await BroadcastService.GetPagedAnnouncementsForAdminAsync(page, 20, searchQuery);

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
                        PageSize = 20,
                        TotalPages = 0,
                        HasNextPage = false,
                        HasPreviousPage = false
                    };
                }
            }
            catch (Exception ex)
            {
                errorMessage = Localizer.GetString("Administration.BroadcastAnnouncements.Error.General");
                Console.WriteLine($"Error loading admin broadcast announcements: {ex.Message}");
                
                // Set empty response on error
                announcements = new List<BroadcastAnnouncementResponse>();
                pagedResponse = new PagedBroadcastAnnouncementResponse
                {
                    Announcements = new List<BroadcastAnnouncementResponse>(),
                    TotalCount = 0,
                    CurrentPage = 1,
                    PageSize = 20,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                };
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task RefreshData()
        {
            await LoadPage(currentPage, currentSearchTerm);
        }

        private async Task SearchAnnouncements()
        {
            currentSearchTerm = searchTerm.Trim();
            await LoadPage(1, currentSearchTerm);
        }

        private async Task ClearSearch()
        {
            searchTerm = string.Empty;
            currentSearchTerm = string.Empty;
            await LoadPage(1);
        }

        private async Task OnSearchKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await SearchAnnouncements();
            }
        }

        private void ShowDeleteModal(BroadcastAnnouncementResponse announcement)
        {
            announcementToDelete = announcement;
        }

        private void HideDeleteModal()
        {
            announcementToDelete = null;
        }

        private void ShowDeleteAllModal()
        {
            showDeleteAllModal = true;
        }

        private void HideDeleteAllModal()
        {
            showDeleteAllModal = false;
        }

        private async Task DeleteAnnouncement()
        {
            if (announcementToDelete == null)
                return;

            try
            {
                isDeleting = true;
                StateHasChanged();

                var success = await BroadcastService.DeleteAnnouncementAsync(announcementToDelete.Id);

                if (success)
                {
                    // Remove from local list
                    if (announcements != null)
                    {
                        announcements.RemoveAll(a => a.Id == announcementToDelete.Id);
                    }

                    // Update totals
                    if (pagedResponse != null)
                    {
                        pagedResponse.TotalCount = Math.Max(0, pagedResponse.TotalCount - 1);
                        pagedResponse.TotalPages = (int)Math.Ceiling((double)pagedResponse.TotalCount / pagedResponse.PageSize);
                    }

                    // Refresh if current page is empty and not the first page
                    if (announcements?.Count == 0 && currentPage > 1)
                    {
                        await LoadPage(currentPage - 1, currentSearchTerm);
                    }
                    
                    errorMessage = null;
                }
                else
                {
                    errorMessage = Localizer.GetString("Administration.BroadcastAnnouncements.DeleteError");
                }
            }
            catch (Exception ex)
            {
                errorMessage = Localizer.GetString("Administration.BroadcastAnnouncements.DeleteError");
                Console.WriteLine($"Error deleting announcement: {ex.Message}");
            }
            finally
            {
                isDeleting = false;
                HideDeleteModal();
                StateHasChanged();
            }
        }

        private async Task DeleteAllAnnouncements()
        {
            try
            {
                isDeleting = true;
                StateHasChanged();

                var deletedCount = await BroadcastService.DeleteAllAnnouncementsAsync();

                // Clear local lists
                announcements = new List<BroadcastAnnouncementResponse>();
                pagedResponse = new PagedBroadcastAnnouncementResponse
                {
                    Announcements = new List<BroadcastAnnouncementResponse>(),
                    TotalCount = 0,
                    CurrentPage = 1,
                    PageSize = 20,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                };

                currentPage = 1;
                errorMessage = null;
            }
            catch (Exception ex)
            {
                errorMessage = Localizer.GetString("Administration.BroadcastAnnouncements.DeleteAllError");
                Console.WriteLine($"Error deleting all announcements: {ex.Message}");
            }
            finally
            {
                isDeleting = false;
                HideDeleteAllModal();
                StateHasChanged();
            }
        }
    }
} 