using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using GrznarAi.Web.Data;
using GrznarAi.Web.Services;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GrznarAi.Web.Components.Pages.Admin
{
    public class GlobalSettingsAdminBase : ComponentBase
    {
        [Inject]
        protected IGlobalSettingsService GlobalSettingsService { get; set; }

        [Inject]
        protected ILocalizationService Localizer { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected List<GlobalSetting> Settings { get; set; } = new();
        protected GlobalSetting? SettingToDelete { get; set; }
        protected GlobalSettingModel CurrentSetting { get; set; } = new();
        protected bool IsLoading { get; set; } = true;
        protected bool IsModalVisible { get; set; } = false;
        protected bool IsEditMode { get; set; } = false;
        protected bool IsDeleteConfirmVisible { get; set; } = false;
        protected bool IsMessageVisible { get; set; } = false;
        protected string ModalTitle { get; set; } = "";
        protected string SearchText { get; set; } = "";
        protected string MessageTitle { get; set; } = "";
        protected string MessageContent { get; set; } = "";
        protected string MessageCssClass { get; set; } = "";
        protected int CurrentPage { get; set; } = 1;
        protected int PageSize { get; set; } = 10;
        protected int TotalCount { get; set; } = 0;
        protected int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        protected string SortColumn { get; set; } = "key";
        protected string SortDirection { get; set; } = "asc";

        protected override async Task OnInitializedAsync()
        {
            PageSize = GlobalSettingsService.GetInt("Admin.GlobalSettings.PageSize", 10);
            await LoadSettings();
        }

        protected async Task LoadSettings()
        {
            IsLoading = true;
            try
            {
                Settings = await GlobalSettingsService.GetAllSettingsAsync(
                    SearchText, 
                    SortColumn, 
                    SortDirection, 
                    CurrentPage, 
                    PageSize);
                TotalCount = await GlobalSettingsService.GetTotalSettingsCountAsync(SearchText);
            }
            catch (Exception ex)
            {
                ShowMessage("Chyba", $"Nastala chyba při načítání nastavení: {ex.Message}", "bg-danger");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task SearchSettings()
        {
            CurrentPage = 1;
            await LoadSettings();
        }

        protected async Task ClearSearch()
        {
            SearchText = "";
            CurrentPage = 1;
            await LoadSettings();
        }

        protected async Task PageSizeChanged(ChangeEventArgs e)
        {
            // Resetujeme aktuální stránku na 1 při změně počtu záznamů na stránku
            CurrentPage = 1;
            await LoadSettings();
        }

        protected async Task ChangePage(int page)
        {
            if (page < 1 || page > TotalPages)
                return;

            CurrentPage = page;
            await LoadSettings();
        }

        protected async Task SortTableByKey()
        {
            await SortTable("key");
        }

        protected async Task SortTableByValue()
        {
            await SortTable("value");
        }

        protected async Task SortTableByDataType()
        {
            await SortTable("datatype");
        }

        protected async Task SortTableByUpdatedAt()
        {
            await SortTable("updatedat");
        }

        private async Task SortTable(string column)
        {
            if (SortColumn == column)
            {
                SortDirection = SortDirection == "asc" ? "desc" : "asc";
            }
            else
            {
                SortColumn = column;
                SortDirection = "asc";
            }
            
            await LoadSettings();
        }

        protected string GetSortIcon(string column)
        {
            if (SortColumn != column)
                return "";

            return SortDirection == "asc" 
                ? "<i class=\"bi bi-arrow-up\"></i>" 
                : "<i class=\"bi bi-arrow-down\"></i>";
        }

        protected void ShowCreateModal()
        {
            IsEditMode = false;
            ModalTitle = "Vytvořit nové nastavení";
            CurrentSetting = new GlobalSettingModel();
            IsModalVisible = true;
        }

        protected async Task ShowEditModal(int id)
        {
            IsEditMode = true;
            ModalTitle = "Upravit nastavení";
            
            var setting = await GlobalSettingsService.GetSettingByIdAsync(id);
            if (setting != null)
            {
                CurrentSetting = new GlobalSettingModel
                {
                    Id = setting.Id,
                    Key = setting.Key,
                    Value = setting.Value,
                    DataType = setting.DataType,
                    Description = setting.Description
                };
                IsModalVisible = true;
            }
            else
            {
                ShowMessage("Chyba", "Nastavení nebylo nalezeno", "bg-danger");
            }
        }

        protected void CloseModal()
        {
            IsModalVisible = false;
        }

        protected async Task HandleValidSubmit()
        {
            try
            {
                if (IsEditMode)
                {
                    var setting = await GlobalSettingsService.GetSettingByIdAsync(CurrentSetting.Id);
                    if (setting != null)
                    {
                        setting.Value = CurrentSetting.Value;
                        setting.DataType = CurrentSetting.DataType;
                        setting.Description = CurrentSetting.Description;
                        
                        await GlobalSettingsService.UpdateSettingAsync(setting);
                        ShowMessage("Úspěch", "Nastavení bylo úspěšně aktualizováno", "bg-success");
                    }
                }
                else
                {
                    var setting = new GlobalSetting
                    {
                        Key = CurrentSetting.Key,
                        Value = CurrentSetting.Value,
                        DataType = CurrentSetting.DataType,
                        Description = CurrentSetting.Description
                    };
                    
                    await GlobalSettingsService.AddSettingAsync(setting);
                    ShowMessage("Úspěch", "Nové nastavení bylo úspěšně vytvořeno", "bg-success");
                }
                
                CloseModal();
                await LoadSettings();
            }
            catch (Exception ex)
            {
                ShowMessage("Chyba", $"Nastala chyba při ukládání nastavení: {ex.Message}", "bg-danger");
            }
        }

        protected async Task ConfirmDelete(int id)
        {
            SettingToDelete = await GlobalSettingsService.GetSettingByIdAsync(id);
            if (SettingToDelete != null)
            {
                IsDeleteConfirmVisible = true;
            }
        }

        protected void CancelDelete()
        {
            IsDeleteConfirmVisible = false;
            SettingToDelete = null;
        }

        protected async Task DeleteSetting()
        {
            if (SettingToDelete != null)
            {
                try
                {
                    await GlobalSettingsService.DeleteSettingAsync(SettingToDelete.Id);
                    ShowMessage("Úspěch", "Nastavení bylo úspěšně smazáno", "bg-success");
                }
                catch (Exception ex)
                {
                    ShowMessage("Chyba", $"Nastala chyba při mazání nastavení: {ex.Message}", "bg-danger");
                }
                
                CancelDelete();
                await LoadSettings();
            }
        }

        protected void ShowMessage(string title, string content, string cssClass)
        {
            MessageTitle = title;
            MessageContent = content;
            MessageCssClass = cssClass;
            IsMessageVisible = true;
        }

        protected void CloseMessage()
        {
            IsMessageVisible = false;
        }

        protected async Task HandleSearchKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await SearchSettings();
            }
        }

        public class GlobalSettingModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Klíč je povinný")]
            [StringLength(100, ErrorMessage = "Klíč může mít maximálně 100 znaků")]
            public string Key { get; set; } = string.Empty;

            [Required(ErrorMessage = "Hodnota je povinná")]
            public string Value { get; set; } = string.Empty;

            [StringLength(50, ErrorMessage = "Typ může mít maximálně 50 znaků")]
            public string? DataType { get; set; }

            public string? Description { get; set; }
        }
    }
} 