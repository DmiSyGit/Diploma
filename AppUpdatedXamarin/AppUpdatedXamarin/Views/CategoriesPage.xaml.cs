using System;
using Xamarin.Forms;

namespace AppUpdatedXamarin.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            ItemsPage.productSearch.Query = ((Button)sender).Text;
            await (App.Current.MainPage as Shell).GoToAsync("//tabbar/tab/ItemsPage");
            ItemsPage._viewModel.IsBusy = false;
            ItemsPage._viewModel.IsBusy = true;
        }
    }
}