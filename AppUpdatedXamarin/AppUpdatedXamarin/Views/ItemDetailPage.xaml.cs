using Xamarin.Forms;
using AppUpdatedXamarin.ViewModels;

namespace AppUpdatedXamarin.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        ItemDetailViewModel _itemDetailViewModel;
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = _itemDetailViewModel = new ItemDetailViewModel(this);
            carouselViewPhoto.ItemsSource = _itemDetailViewModel.images;
        }
    }
}