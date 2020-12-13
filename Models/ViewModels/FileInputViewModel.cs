using Microsoft.AspNetCore.Http;

namespace Lab4.Models.ViewModels
{
    public class FileInputViewModel
    {
        public string CommunityId { get; set; }
        public string CommunityTitle { get; set; }
        public IFormFile File { get; set; }
    }

}

