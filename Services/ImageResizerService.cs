﻿using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using RainstormTech.Storm.ImageProxy.Options;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace RainstormTech.Storm.ImageProxy
{
    public class ImageResizerService : IImageResizerService
    {
        private readonly HttpClient httpClient;
        private readonly IOptions<ImageResizerOptions> settings;
        private readonly ILogger logger;
        private readonly IConfiguration config;

        public ImageResizerService(HttpClient httpClient,
            IOptions<ImageResizerOptions> settings,
            IConfiguration configuration,
            ILogger<ImageResizerService> logger)
        {
            this.httpClient = httpClient;
            this.settings = settings;
            this.logger = logger;
            config = configuration;
        }

        /// <summary>
        /// Resizes an image to specified size and format
        /// </summary>
        /// <param name="url"></param>
        /// <param name="size"></param>
        /// <param name="output"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> ResizeAsync(ResizeImagePayload resizeParams, string size, string output, string mode)
        {
            Stream resizedImageStream;
            ImageSize imageSize = StringToImageSize(size);

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(resizeParams.connectionString);

            try
            {
                // Create the container and return a container client object
                var container = blobServiceClient.GetBlobContainerClient(resizeParams.containerIn);
                await container.CreateIfNotExistsAsync();

                // Get a reference to a blob
                BlobClient inputBlobClient = container.GetBlobClient(resizeParams.nameIn);

                // Download the blob's contents and save it to a stream
                using (var imageStream = new MemoryStream())
                {
                    var res = await inputBlobClient.DownloadToAsync(imageStream);
                    imageStream.Position = 0;

                    // Note that it was this stream that used to be returned
                    resizedImageStream = GetResizedImage(imageStream, imageSize, output, mode);

                    // Save to output blob container
                    var outputContainer = blobServiceClient.GetBlobContainerClient(resizeParams.containerOut);
                    await outputContainer.CreateIfNotExistsAsync();
                    // Use the same file name as the larger input image file
                    BlobClient outputBlobClient = outputContainer.GetBlobClient(resizeParams.nameIn);
                    BlobContentInfo resBlobUpload = await outputBlobClient.UploadAsync(resizedImageStream, new BlobHttpHeaders { ContentType = res.Headers.ContentType });

                    return true;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Exception manipulating blob container images - " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Resizes/Crops image
        /// </summary>
        /// <param name="originalStream"></param>
        /// <param name="size"></param>
        /// <param name="output"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        private Stream GetResizedImage(Stream stream, ImageSize size, string output, string mode)
        {
            var resultStream = new MemoryStream();

            // if we don't need to resize it, then just copy to resultStream and leave
            if (size.Name == ImageSize.OriginalImageSize)
            {
                stream.CopyTo(resultStream);
                resultStream.Position = 0;
                return resultStream;
            }

            // handle the resize
            using (var image = Image.Load(stream))
            {
                var seletedMode = mode.ToLower() switch
                {
                    "boxpad" => ResizeMode.BoxPad,
                    "pad" => ResizeMode.Pad,
                    "max" => ResizeMode.Max,
                    "min" => ResizeMode.Min,
                    "stretch" => ResizeMode.Stretch,
                    _ => ResizeMode.Crop
                };

                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(size.Width, size.Height),
                    Mode = seletedMode
                };

                image.Mutate(x => x
                    .AutoOrient()
                    .Resize(resizeOptions));

                // output defaults to the current filetype, but can be overridden
                switch (output)
                {
                    case "jpg":
                        image.SaveAsJpeg(resultStream);
                        break;
                    case "gif":
                        image.SaveAsGif(resultStream);
                        break;
                    // case "webp":         
                    // hopefully SixLabors adds their own WebP save feature soon
                    // break;
                    default: // png
                        image.SaveAsPng(resultStream);
                        break;
                }

                resultStream.Position = 0;
            }

            return resultStream;
        }

        /// <summary>
        /// Figure out if the user passed a standard size or custom size
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private ImageSize StringToImageSize(string value)
        {
            ImageSize imageSize;
            if (this.settings.Value.PredefinedImageSizes.TryGetValue(value.ToLowerInvariant(), out imageSize))
            {
                return imageSize;
            }

            return ImageSize.Parse(value);
        }

        /// <summary>
        /// If the image is base64 encoded, convert that to a string.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static Uri NormalizeUrl(string url)
        {
            var targetUrl = url;
            if (url.StartsWith("base64:", StringComparison.InvariantCultureIgnoreCase))
            {
                targetUrl = Encoding.UTF8.GetString(Convert.FromBase64String(url.Substring(7)));
            }

            return new Uri(targetUrl.StartsWith(Uri.UriSchemeHttp) ? targetUrl : (Uri.UriSchemeHttp + Uri.SchemeDelimiter + targetUrl));
        }
    }
}
