using AppUpdatedXamarin.Models;
using System.Threading.Tasks;

namespace AppUpdatedXamarin
{
    public interface IPhotoPickerService
    {
        Task<ImageItem> GetImageDataAsync();
    }
}
