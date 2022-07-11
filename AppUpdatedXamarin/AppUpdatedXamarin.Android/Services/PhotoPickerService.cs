using System.Threading.Tasks;
using Android.Content;
using AppUpdatedXamarin.Droid;
using AppUpdatedXamarin.Models;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhotoPickerService))]
namespace AppUpdatedXamarin.Droid
{
    public class PhotoPickerService : IPhotoPickerService
    {
        public Task<ImageItem> GetImageDataAsync()
        {
            // Define the Intent for getting images
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            // Start the picture-picker activity (resumes in MainActivity.cs)
            MainActivity.Instance.StartActivityForResult(
                Intent.CreateChooser(intent, "Select Photo"),
                MainActivity.PickImageId);

            // Save the TaskCompletionSource object as a MainActivity property
            MainActivity.Instance.PickImageTaskCompletionSource = new TaskCompletionSource<ImageItem>();

            // Return Task object
            return MainActivity.Instance.PickImageTaskCompletionSource.Task;
        }
    }
}