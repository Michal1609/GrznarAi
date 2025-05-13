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
        private List<DirectoryInfo> directoryTree = new();
        private List<IBrowserFile> selectedFiles = new();
        private ImageInfo? selectedImage;
        private ImageInfo? imageToDelete;
        private DirectoryInfo? directoryToDelete;
        private string currentDirectory = "images";
        private string filterText = string.Empty;
        private bool isLoading = true;
        private bool isUploading = false;
        private string alertMessage = string.Empty;
        private string alertClass = "alert-info";
        private string newFolderName = string.Empty;
        private bool showNewFolderModal = false;
        private string selectedDirectoryPath = string.Empty;
        
        protected override async Task OnInitializedAsync()
        {
            await LoadDirectoryTree();
            await LoadImages();
            selectedDirectoryPath = currentDirectory;
        }

        private async Task LoadDirectoryTree()
        {
            directoryTree.Clear();
            
            // Get base wwwroot/images directory
            string wwwrootPath = Environment.WebRootPath;
            string imagesBasePath = Path.Combine(wwwrootPath, "images");
            
            // Create the root directories if they don't exist
            if (!Directory.Exists(imagesBasePath))
            {
                Directory.CreateDirectory(imagesBasePath);
            }

            // Add root directory
            var rootDir = new DirectoryInfo
            {
                Name = "images",
                FullPath = imagesBasePath,
                RelativePath = "images",
                Level = 0,
                Children = new List<DirectoryInfo>()
            };
            
            directoryTree.Add(rootDir);
            
            // Process subdirectories recursively
            await LoadSubdirectories(rootDir);
            
            await Task.CompletedTask;
        }
        
        private async Task LoadSubdirectories(DirectoryInfo parentDir)
        {
            try
            {
                string[] subdirectories = Directory.GetDirectories(parentDir.FullPath);
                
                foreach (var dir in subdirectories)
                {
                    string dirName = Path.GetFileName(dir);
                    var subDir = new DirectoryInfo
                    {
                        Name = dirName,
                        FullPath = dir,
                        RelativePath = Path.Combine(parentDir.RelativePath, dirName).Replace("\\", "/"),
                        Level = parentDir.Level + 1,
                        Parent = parentDir,
                        Children = new List<DirectoryInfo>()
                    };
                    
                    parentDir.Children.Add(subDir);
                    
                    // Recursively process subdirectories
                    await LoadSubdirectories(subDir);
                }
            }
            catch (Exception ex)
            {
                // Log error or handle exception
                ShowAlert($"Error loading directories: {ex.Message}", "alert-danger");
            }
            
            await Task.CompletedTask;
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
                            
                            var imageInfo = new ImageInfo
                            {
                                Name = Path.GetFileName(file),
                                FullPath = file,
                                RelativePath = relativePath,
                                Url = url,
                                Size = fileInfo.Length,
                                LastModified = fileInfo.LastWriteTime
                            };
                            
                            // Try to get image dimensions if possible
                            // Note: In a real app, you might want to use a library that can read image dimensions without loading the whole image
                            try
                            {
                                // Code to get image dimensions could be added here
                                // For simplicity, we're not implementing it in this example
                            }
                            catch { /* Ignore errors when getting dimensions */ }
                            
                            images.Add(imageInfo);
                        }
                        catch (Exception)
                        {
                            // Skip files that can't be processed
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Error loading images: {ex.Message}", "alert-danger");
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
            await LoadDirectoryTree();
            await LoadImages();
        }
        
        private async Task SelectDirectory(string directoryPath)
        {
            currentDirectory = directoryPath;
            selectedDirectoryPath = directoryPath;
            await LoadImages();
        }
        
        private void ShowNewFolderModal()
        {
            newFolderName = string.Empty;
            showNewFolderModal = true;
        }
        
        private void HideNewFolderModal()
        {
            showNewFolderModal = false;
        }
        
        private async Task CreateNewFolder()
        {
            if (string.IsNullOrWhiteSpace(newFolderName))
            {
                ShowAlert(Localizer.GetString("ImageManager.NewFolder.EmptyName", "Folder name cannot be empty."), "alert-warning");
                return;
            }
            
            try
            {
                // Clean folder name to remove invalid characters
                string cleanName = Regex.Replace(newFolderName, @"[^\w\.-]", "-");
                
                string wwwrootPath = Environment.WebRootPath;
                string newFolderPath = Path.Combine(wwwrootPath, currentDirectory, cleanName);
                
                if (Directory.Exists(newFolderPath))
                {
                    ShowAlert(Localizer.GetString("ImageManager.NewFolder.AlreadyExists", "A folder with this name already exists."), "alert-warning");
                    return;
                }
                
                Directory.CreateDirectory(newFolderPath);
                ShowAlert(Localizer.GetString("ImageManager.NewFolder.Success", "Folder created successfully."), "alert-success");
                
                // Refresh directory tree
                await LoadDirectoryTree();
                HideNewFolderModal();
            }
            catch (Exception ex)
            {
                ShowAlert($"{Localizer.GetString("ImageManager.NewFolder.Error", "An error occurred while creating the folder.")}: {ex.Message}", "alert-danger");
            }
        }
        
        private void ConfirmDeleteDirectory(DirectoryInfo directory)
        {
            directoryToDelete = directory;
        }
        
        private void CancelDeleteDirectory()
        {
            directoryToDelete = null;
        }
        
        private async Task DeleteDirectory()
        {
            if (directoryToDelete == null) return;
            
            try
            {
                string directoryPath = directoryToDelete.FullPath;
                
                if (Directory.Exists(directoryPath))
                {
                    // Check if the directory is empty
                    bool isEmpty = !Directory.EnumerateFileSystemEntries(directoryPath).Any();
                    
                    if (!isEmpty)
                    {
                        ShowAlert(Localizer.GetString("ImageManager.DeleteDirectory.NotEmpty", "The directory is not empty. Delete all files and subdirectories first."), "alert-warning");
                        return;
                    }
                    
                    Directory.Delete(directoryPath);
                    ShowAlert(Localizer.GetString("ImageManager.DeleteDirectory.Success", "Directory was successfully deleted."), "alert-success");
                    
                    // If the deleted directory was the current directory, go back to the parent
                    if (currentDirectory == directoryToDelete.RelativePath)
                    {
                        if (directoryToDelete.Parent != null)
                        {
                            currentDirectory = directoryToDelete.Parent.RelativePath;
                            selectedDirectoryPath = currentDirectory;
                            await LoadImages();
                        }
                        else
                        {
                            currentDirectory = "images";
                            selectedDirectoryPath = "images";
                            await LoadImages();
                        }
                    }
                    
                    // Refresh directory tree
                    await LoadDirectoryTree();
                }
                else
                {
                    ShowAlert(Localizer.GetString("ImageManager.DeleteDirectory.NotFound", "The directory does not exist."), "alert-warning");
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"{Localizer.GetString("ImageManager.DeleteDirectory.Error", "An error occurred while deleting the directory.")}: {ex.Message}", "alert-danger");
            }
            finally
            {
                directoryToDelete = null;
                StateHasChanged();
            }
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
    
    public class DirectoryInfo
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public int Level { get; set; }
        public DirectoryInfo? Parent { get; set; }
        public List<DirectoryInfo> Children { get; set; } = new();
        public bool IsExpanded { get; set; } = true;
    }
} 