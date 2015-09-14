using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronozoom.UI.Utils;
using System.Data.SqlClient;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net;
using System.IO;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;

namespace OneDriveThumbnailsRepairer
{
    class Program
    {
        private static void SaveUploadThumbnail(CloudBlobContainer imagesContainer, string filename, Bitmap bitmap, int dimension)
        {
            if (imagesContainer == null)
                return;

            using (Bitmap thumbnail = new Bitmap(dimension, dimension))
            {

                Graphics graphics = Graphics.FromImage(thumbnail);
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.DrawImage(bitmap, 0, 0, dimension, dimension);

                using (MemoryStream thumbnailStream = new MemoryStream())
                {
                    thumbnail.Save(thumbnailStream, System.Drawing.Imaging.ImageFormat.Png);

                    // Upload thumbnail
                    CloudBlockBlob blockBlob = imagesContainer.GetBlockBlobReference(@"x" + dimension + @"\" + filename + ".png");

                    thumbnailStream.Seek(0, SeekOrigin.Begin);
                    blockBlob.UploadFromStream(thumbnailStream);
                }
            }
        }
   

        static void GenerateThumbnails(CloudBlobContainer imagesContainer, Guid id, string uri)
        {
            // Generate thumbnails
            WebRequest request = WebRequest.Create(uri);
            using (WebResponse response = request.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                try
                {
                    using (Bitmap bitmap = new Bitmap(responseStream))
                    {
                        if (response.ContentLength > 10000000)//Max source content length. Is necessary?
                        {
                            throw new InvalidDataException("Source image is too big.");
                        }

                        SaveUploadThumbnail(imagesContainer, id.ToString(), bitmap, 8);
                        SaveUploadThumbnail(imagesContainer, id.ToString(), bitmap, 16);
                        SaveUploadThumbnail(imagesContainer, id.ToString(), bitmap, 32);
                        SaveUploadThumbnail(imagesContainer, id.ToString(), bitmap, 64);
                        SaveUploadThumbnail(imagesContainer, id.ToString(), bitmap, 128);
                        Console.WriteLine("OK!");
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); };
            }
        }
        static void Main(string[] args)
        {
            string _thumbnailStorage = "DefaultEndpointsProtocol=https;AccountName=cznodeletetest;AccountKey=8oasSZEXHy7kyRA1Xe+HoloCbUnH+hx/Y049WXmkNWcS3lZ4V4OgGm73Shnwvxo9diOP3oCU45dBGAcpNOmRSg==;BlobEndpoint=https://cznodeletetest.blob.core.windows.net";

            // Retrieve storage account information
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_thumbnailStorage);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to images container. 
            CloudBlobContainer imagesContainer = blobClient.GetContainerReference("images");

            if (!imagesContainer.Exists())
            {
                imagesContainer.CreateIfNotExists();
                imagesContainer.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });
            }

            using (SqlConnection cn = new SqlConnection("Data Source=tcp:jvlwd0c6a4.database.windows.net,1433;Initial Catalog=cz-nodelete-chronozoom-test;User ID=cznodeletechronozoomtest@jvlwd0c6a4;Password=ChronoChrono!"))
            {

                // at least an empty db needs to pre-exist but a schema is not required
                cn.Open();
                var _sql =
                    @"
                    SELECT Id,Uri FROM ContentItems where MediaType = 'skydrive-image'
                    ";
                using (SqlCommand cmd = new SqlCommand(_sql, cn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Call Read before accessing data.
                        while (reader.Read())
                        {
                            IDataRecord record = (IDataRecord)reader;
                            GenerateThumbnails(imagesContainer, (Guid)record[0], (string)record[1]);
                        }
                    }
                }
                
            }



        }
    }
}
