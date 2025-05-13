using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using GrznarAi.Web.Services;

namespace GrznarAi.Web.Components.Pages.Admin
{
    public partial class ImageManager
    {
        private List<ImageInfo> images = new();
        private List<string> directories = new();
        private List<IBrowserFile> selectedFiles = new();
        private ImageInfo? selectedImage;
        private ImageInfo? imageToDelete;
        private string currentDirectory = "images";
        private string filterText = string.Empty;
        private bool isLoading = true;
        private bool isUploading = false;
        private string alertMessage = string.Empty;
        private string alertClass = "alert-info";
        
        protected override async Task OnInitializedAsync()
        {
            await LoadDirectories();
            await LoadImages();
        }

        private Task LoadDirectories()
        {
            directories.Clear();
            
            // Add default directories
            directories.Add("images");
            
            // Add subdirectories in images
            string wwwrootPath = Environment.WebRootPath;
            string imagesPath = Path.Combine(wwwrootPath, "images");
            
            if (Directory.Exists(imagesPath))
            {
                foreach (var dir in Directory.GetDirectories(imagesPath))
                {
                    string dirName = Path.GetRelativePath(wwwrootPath, dir).Replace("\\", "/");
                    directories.Add(dirName);
                }
            }

            return Task.CompletedTask;
        }

        private async Task LoadImages()
        {
            isLoading = true;
            images.Clear();
            
            try
            {
                string wwwrootPath = Environment.WebRootPath;
                string directoryPath = Path.Combine(wwwrootPath, currentDirectory);
                
                if (Directory.Exists(directoryPath))
                {
                    string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" };
                    var files = Directory.GetFiles(directoryPath)
                        .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                        .ToList();
                    
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            string relativePath = Path.GetRelativePath(wwwrootPath, file).Replace("\\", "/");
                            string url = $"/{relativePath}";
                            
                            // We don't get image dimensions from the server side to avoid external dependencies
                            // Client-side JavaScript can be used to get dimensions if needed
                            int width = 0;
                            int height = 0;
                            
                            images.Add(new ImageInfo
                            {
                                Name = Path.GetFileName(file),
                                FullPath = file,
                                RelativePath = relativePath,
                                Url = url,
                                Size = fileInfo.Length,
                                LastModified = fileInfo.LastWriteTime,
                                Width = width,
                                Height = height
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing file {file}: {ex.Message}");
                        }
                    }
                    
                    // Sort images by name
                    images = images.OrderBy(i => i.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"{Localizer.GetString("ImageManager.Load.Error", "Error loading images")}: {ex.Message}", "alert-danger");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private List<ImageInfo> filteredImages => 
            string.IsNullOrWhiteSpace(filterText) 
                ? images 
                : images.Where(i => i.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase)).ToList();

        private async Task RefreshImages()
        {
            await LoadImages();
        }

        private void OnInputFileChange(InputFileChangeEventArgs e)
        {
            selectedFiles.Clear();
            
            foreach (var file in e.GetMultipleFiles(50)) // Limit to 50 files
            {
                // Validate file type
                string extension = Path.GetExtension(file.Name).ToLowerInvariant();
                if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" }.Contains(extension))
                {
                    selectedFiles.Add(file);
                }
            }
            
            StateHasChanged();
        }

        private async Task UploadFiles()
        {
            if (selectedFiles.Count == 0) return;
            
            isUploading = true;
            
            try
            {
                string wwwrootPath = Environment.WebRootPath;
                string uploadPath = Path.Combine(wwwrootPath, currentDirectory);
                
                // Ensure directory exists
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                int successCount = 0;
                List<string> failedFiles = new();
                
                foreach (var file in selectedFiles)
                {
                    try
                    {
                        string fileName = file.Name;
                        
                        // Clean filename - replace spaces and special characters
                        fileName = Regex.Replace(fileName, @"[^\w\.-]", "-");
                        
                        // Check if file already exists, append number if needed
                        string baseName = Path.GetFileNameWithoutExtension(fileName);
                        string extension = Path.GetExtension(fileName);
                        string fullPath = Path.Combine(uploadPath, fileName);
                        
                        int counter = 1;
                        while (File.Exists(fullPath))
                        {
                            fileName = $"{baseName}-{counter}{extension}";
                            fullPath = Path.Combine(uploadPath, fileName);
                            counter++;
                        }
                        
                        await using var stream = file.OpenReadStream(10 * 1024 * 1024); // Max 10MB
                        await using var fileStream = new FileStream(fullPath, FileMode.Create);
                        await stream.CopyToAsync(fileStream);
                        
                        successCount++;
                    }
                    catch (Exception)
                    {
                        failedFiles.Add(file.Name);
                    }
                }
                
                if (successCount > 0)
                {
                    ShowAlert(Localizer.GetString("ImageManager.Upload.Success", "Images were successfully uploaded."), "alert-success");
                }
                
                if (failedFiles.Count > 0)
                {
                    ShowAlert(Localizer.GetString("ImageManager.Upload.Error", "An error occurred while uploading images."), "alert-danger");
                }
                
                // Refresh the image list
                selectedFiles.Clear();
                await LoadImages();
            }
            catch (Exception ex)
            {
                ShowAlert($"{Localizer.GetString("ImageManager.Upload.Error", "An error occurred while uploading images.")}: {ex.Message}", "alert-danger");
            }
            finally
            {
                isUploading = false;
                StateHasChanged();
            }
        }

        private void ShowImageDetails(ImageInfo image)
        {
            selectedImage = image;
        }

        private void CloseModal()
        {
            selectedImage = null;
        }

        private void ConfirmDeleteImage(ImageInfo image)
        {
            imageToDelete = image;
        }

        private void CancelDelete()
        {
            imageToDelete = null;
        }

        private async Task DeleteImage()
        {
            if (imageToDelete == null) return;
            
            try
            {
                string filePath = imageToDelete.FullPath;
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    ShowAlert(Localizer.GetString("ImageManager.Delete.Success", "Image was successfully deleted."), "alert-success");
                    
                    // If the deleted image was the selected image, close the detail modal
                    if (selectedImage != null && selectedImage.FullPath == imageToDelete.FullPath)
                    {
                        selectedImage = null;
                    }
                    
                    // Remove the image from the list
                    images.RemoveAll(i => i.FullPath == imageToDelete.FullPath);
                }
                else
                {
                    ShowAlert(Localizer.GetString("ImageManager.Delete.Error", "An error occurred while deleting the image."), "alert-danger");
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"{Localizer.GetString("ImageManager.Delete.Error", "An error occurred while deleting the image.")}: {ex.Message}", "alert-danger");
            }
            finally
            {
                imageToDelete = null;
                StateHasChanged();
            }
        }

        private async Task CopyImageUrl(string url)
        {
            try
            {
                await JSRuntime.InvokeAsync<object>("navigator.clipboard.writeText", url);
                ShowAlert(Localizer.GetString("ImageManager.Copy.Success", "URL was copied to clipboard."), "alert-success");
            }
            catch (Exception ex)
            {
                ShowAlert($"{Localizer.GetString("ImageManager.Copy.Error", "An error occurred while copying the URL.")}: {ex.Message}", "alert-danger");
            }
        }

        private void ShowAlert(string message, string alertClass)
        {
            this.alertMessage = message;
            this.alertClass = alertClass;
            StateHasChanged();
        }

        private void ClearAlert()
        {
            alertMessage = string.Empty;
            StateHasChanged();
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }

    public class ImageInfo
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
} 