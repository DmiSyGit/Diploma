using System;
using Xamarin.Forms;
using AppUpdatedXamarin.Models;
using AppUpdatedXamarin.ViewModels;

namespace AppUpdatedXamarin.Views
{
    public partial class ItemsPage : ContentPage
    {
        public static ItemsViewModel _viewModel;
        public static ProductSearchHandler productSearch;

        public ItemsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ItemsViewModel(this);
            productSearch = ProdSearchHandler;
            productSearch.viewModel = _viewModel;
        }

        protected override void OnAppearing()
        {
            FilterPickerItems.SelectedIndex = 0;
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        private void FilterPickerItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            _viewModel.SetRequestStringParams(((Picker)sender).SelectedIndex);
        }
    }
}