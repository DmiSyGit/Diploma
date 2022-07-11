namespace AppUpdatedXamarin.Models
{
    public class ImageItem
    {
        public string ImagePath { get; set; }
        public byte[] ImageBytes { get; set; }
        public ImageItem(string imagePath, byte[] imageBytes)
        {
            ImagePath = imagePath;
            ImageBytes = imageBytes;
        }
        public ImageItem(string imagePath)
        {
            ImagePath = imagePath;
        }

        public ImageItem()
        {
        }
    }
}
