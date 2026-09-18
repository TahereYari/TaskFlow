using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Utilities
{
    using Microsoft.AspNetCore.Http;

    public static class FileUploader
    {
        /// <summary>
        /// فایل را در مسیر wwwroot/Uploads/{folderName} ذخیره می‌کند
        /// و فقط نام فایل ذخیره‌شده را برمی‌گرداند، نه مسیر کامل فایل.
        /// </summary>
        public static async Task<string> SaveFileAsync(
            IFormFile file,
            string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("فایلی برای آپلود ارسال نشده است.");

            if (string.IsNullOrWhiteSpace(folderName))
                throw new ArgumentException("نام پوشه معتبر نیست.");

            // مسیر ذخیره فایل:
            // wwwroot/Uploads/{folderName}
            var basePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Uploads",
                folderName);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            // ایجاد نام یکتا برای جلوگیری از تداخل فایل‌ها
            var originalFileName = Path.GetFileName(file.FileName);

            var uniqueFileName =
                $"{Guid.NewGuid()}_{originalFileName}";

            var fullPath = Path.Combine(
                basePath,
                uniqueFileName);

            using (var stream = new FileStream(
                fullPath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // فقط نام فایل ذخیره‌شده برگردانده می‌شود
            return uniqueFileName;
        }

        /// <summary>
        /// فایل را از مسیر
        /// wwwroot/Uploads/{folderName}/{fileName}
        /// حذف می‌کند.
        /// </summary>
        public static void DeleteFile(string fileName,string folderName)
        {
            if (string.IsNullOrWhiteSpace(fileName) ||
                string.IsNullOrWhiteSpace(folderName))
                return;

            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Uploads",
                folderName,
                fileName);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        /// <summary>
        /// آدرس پایه پوشه Uploads را بر اساس درخواست فعلی
        /// برمی‌گرداند.
        /// </summary>
        public static string GetUploadsBaseUrl(
            HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host}/uploads";
        }
    }
}
