using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace GrznarAi.Web.Components.Shared
{
    public partial class ImageSelector
    {
        [Inject] private IWebHostEnvironment Environment { get; set; } = default!;
        
        [Parameter] public EventCallback<string> OnImageSelected { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public bool IsVisible { get; set; }

        private List<DirectoryItem> directoryTree = new();
        private List<ImageInfo> images = new();
        private string selectedDirectoryPath = "images";
        private string? selectedImageUrl;
        private bool isLoading = true;

        protected override async Task OnParametersSetAsync()
        {
            if (IsVisible)
            {
                await LoadDirectoriesAsync();
                await LoadImagesAsync(selectedDirectoryPath);
            }
        }

        private Task LoadDirectoriesAsync()
        {
            var wwwRootPath = Environment.WebRootPath;
            var imagesPath = Path.Combine(wwwRootPath, "images");
            if (Directory.Exists(imagesPath))
            {
                var root = new System.IO.DirectoryInfo(imagesPath);
                directoryTree = root.GetDirectories()
                    .Select(d => new DirectoryItem
                    {
                        Name = d.Name,
                        RelativePath = Path.GetRelativePath(wwwRootPath, d.FullName).Replace('\\', '/')
                    })
                    .ToList();
                
                // Add root directory
                directoryTree.Insert(0, new DirectoryItem { Name = "images", RelativePath = "images" });
            }
            return Task.CompletedTask;
        }

        private Task LoadImagesAsync(string directoryPath)
        {
            isLoading = true;
            StateHasChanged();

            var wwwRootPath = Environment.WebRootPath;
            var fullPath = Path.Combine(wwwRootPath, directoryPath);

            if (Directory.Exists(fullPath))
            {
                var directoryInfo = new System.IO.DirectoryInfo(fullPath);
            
                images = directoryInfo.GetFiles()
                    .Where(f => IsImageFile(f.Name))
                    .Select(f => new ImageInfo
                    {
                        Name = f.Name,
                        Url = $"/{directoryPath.Replace('\\', '/')}/{f.Name}",
                        RelativePath = Path.GetRelativePath(wwwRootPath, f.FullName).Replace('\\', '/')
                    }).ToList();
            }
            
            isLoading = false;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private async Task SelectDirectory(string path)
        {
            selectedDirectoryPath = path;
            selectedImageUrl = null;
            await LoadImagesAsync(path);
        }

        private void SelectImage(string imageUrl)
        {
            selectedImageUrl = imageUrl;
        }

        private async Task ConfirmSelection()
        {
            if (!string.IsNullOrEmpty(selectedImageUrl))
            {
                await OnImageSelected.InvokeAsync(selectedImageUrl);
            }
        }

        private async Task Close()
        {
            IsVisible = false;
            await OnClose.InvokeAsync();
        }

        private static bool IsImageFile(string fileName)
        {
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" };
            return extensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
        }

        public class ImageInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string RelativePath { get; set; } = string.Empty;
        }

        public class DirectoryItem
        {
            public string Name { get; set; } = string.Empty;
            public string RelativePath { get; set; } = string.Empty;
        }
    }
} 