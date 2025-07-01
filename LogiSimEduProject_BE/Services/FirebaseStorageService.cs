using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Services
{
    public class FirebaseStorageService
    {
        private readonly string _bucketName = "your-bucket.appspot.com";
        private readonly StorageClient _client;

        public FirebaseStorageService(IHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "Credential/logisimedu-firebase-adminsdk-fbsvc-cea79f44be.json");
            var credential = GoogleCredential.FromFile(path);
            _client = StorageClient.Create(credential);
        }

        public async Task<string> UploadZipAsync(IFormFile file)
        {
            var objectName = $"scene-zips/{Guid.NewGuid()}-{file.FileName}";
            using var stream = file.OpenReadStream();
            await _client.UploadObjectAsync(_bucketName, objectName, file.ContentType, stream);
            return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
        }
    }
}
