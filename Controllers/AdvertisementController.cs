using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Lab4.Data;
using Lab4.Models;
using Azure.Storage.Blobs;
using Azure;
using Lab4.Models.ViewModels;
using System.Linq;

namespace Lab4.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly SchoolCommunityContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string containerName = "advertisement";

        public AdvertisementController(SchoolCommunityContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        public async Task<IActionResult> Index(string ID)
        {
            if (ID is null)
            {
                throw new System.ArgumentNullException(nameof(ID));
            }

            var viewModel = new AdvertisementViewModel();
            viewModel.Community = _context.Communities.Where(a => a.ID == ID).Single();
            System.Collections.Generic.List<Advertisement> Adlist = await _context.Advertisements
                              .Where(c => c.CommunityID == ID).OrderBy(c => c.AdvertisementId).AsNoTracking().ToListAsync();
            viewModel.Advertisements = Adlist;
            return View(viewModel);


        }
        [HttpGet]
        public IActionResult Upload(string ID)
        {
            if (ID is null)
            {
                throw new System.ArgumentNullException(nameof(ID));
            }

            ViewData["CommunityID"] = ID;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string ID)
        {
            if (file != null)
            {
                BlobContainerClient containerClient;
                // Create the container and return a container client object
                try
                {
                    containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
                    containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
                }
                catch (RequestFailedException)
                {
                    containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                }


                try
                {
                    // create the blob to hold the data
                    var blockBlob = containerClient.GetBlobClient(file.FileName);
                    if (await blockBlob.ExistsAsync())
                    {
                        await blockBlob.DeleteAsync();
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        // copy the file data into memory
                        await file.CopyToAsync(memoryStream);

                        // navigate back to the beginning of the memory stream
                        memoryStream.Position = 0;

                        // send the file to the cloud
                        await blockBlob.UploadAsync(memoryStream);
                        memoryStream.Close();
                    }

                    // add the photo to the database if it uploaded successfully
                    var image = new Advertisement();
                    image.CommunityID = ID;
                    image.Url = blockBlob.Uri.AbsoluteUri;
                    image.FileName = file.FileName;

                    _context.Advertisements.Add(image);
                    _context.SaveChanges();
                }
                catch (RequestFailedException)
                {
                    View("Error");
                }

                var imageroute = new { id = ID };
                return base.RedirectToAction("Index", imageroute);
            }
            var route = new { id = ID };
            return base.RedirectToAction("Index", route);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                //  return NotFound();
                throw new System.ArgumentNullException(nameof(id));
            }

            var image = await _context.Advertisements
                .FirstOrDefaultAsync(m => m.AdvertisementId == id);
            return image is null ? NotFound() : (IActionResult)View(image);

        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string CommunityID)
        {
            var image = await _context.Advertisements.FindAsync(id);


            BlobContainerClient containerClient;
            // Get the container and return a container client object
            try
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            try
            {
                int numofAds = _context.Advertisements.Where(a => a.Url == image.Url).Count();


                // Get the blob that holds the data
                var blockBlob = containerClient.GetBlobClient(image.FileName);
                if (await blockBlob.ExistsAsync())
                {
                    if (numofAds == 1)
                        await blockBlob.DeleteAsync();
                }

                _context.Advertisements.Remove(image);
                await _context.SaveChangesAsync();
            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            var deleteroute = new { id = CommunityID };
            return base.RedirectToAction("Index", deleteroute);
        }

    }
}


