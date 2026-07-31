using Krepim.Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Krepim.Catalog.Api.Endpoints.Products
{
    internal static class ImageEndpoints
    {
        public static void MapImageEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapPost("/manager/images", async (
                HttpRequest request,
                [FromServices] IFileStorageService storageService,
                CancellationToken ct) =>
            {
                if (!request.HasFormContentType || !request.Form.Files.Any())
                    return Results.BadRequest("Файлы не выбраны");

                IFormFileCollection files = request.Form.Files;
                string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                List<string> uploadedUrls = new List<string>();

                foreach (IFormFile file in files)
                {
                    string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                        return Results.BadRequest($"Файл {file.FileName} имеет недопустимый формат.");
                }

                foreach (IFormFile file in files)
                {
                    using Stream stream = file.OpenReadStream();
                    string url = await storageService.UploadFileAsync(stream, file.FileName, file.ContentType, ct);
                    uploadedUrls.Add(url);
                }

                return Results.Ok(new { Urls = uploadedUrls });
            })
            .WithName("UploadProductImages")
            .WithSummary("Загрузить галерею изображений товара в MinIO")
            .RequireAuthorization(policy => policy.RequireRole("Manager"))
            .DisableAntiforgery();
        }
    }
}
