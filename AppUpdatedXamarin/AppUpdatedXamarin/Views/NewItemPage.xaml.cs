using System;
using Xamarin.Forms;
using AppUpdatedXamarin.Models;
using AppUpdatedXamarin.ViewModels;

namespace AppUpdatedXamarin.Views
{
    public partial class NewItemPage : ContentPage
    {
        string folderPath;
        NewItemViewModel _viewModel;
        //private List<string> imagesString = new List<string>(5);
        public NewItemPage()
        {
            InitializeComponent();
            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            BindingContext = _viewModel = new NewItemViewModel(this);
        }
        async void OnPickPhotoButtonClicked(object sender, EventArgs e)
        {
            AddMainImageBtn.IsEnabled = false;
            AddImageBtn.IsEnabled = false;
            try
            {
                ImageItem imageData = await DependencyService.Get<IPhotoPickerService>().GetImageDataAsync();
                if (imageData != null && imageData.ImagePath != "BigImage")
                {
                    if ((sender as Button) == AddImageBtn)
                    {
                        if (_viewModel.Images.Count < 5)
                        {
                            _viewModel.Images.Add(imageData);
                        }
                        else
                        {
                            await DisplayAlert("Внимание!", "Для одного товара нельзя добавлять более 5 фотографий!\n" +
                                "Вы можете удалить фотографии, выделив их нажатием.", "OK");
                        }
                    }
                    else
                    {
                        if (_viewModel.Images.Count > 0 && _viewModel.Images.Count == 5)
                        {
                            if (await DisplayAlert("Внимание!", "Произойдет замена главной фотографии!", "Да", "Нет"))
                            {
                                _viewModel.Images.RemoveAt(0);
                                _viewModel.Images.Insert(0, imageData);
                            }
                        }
                        else
                        {
                            _viewModel.Images.Insert(0, imageData);
                        }
                    }
                }
                else if (imageData.ImagePath == "BigImage")
                {
                    await DisplayAlert("Внимание!", "Изображение не должно привышать 5МБ!", "Ок");
                }
            }
            catch
            {
                await DisplayAlert("Ошибка", "Произошла непредвиденная ошибка!", "Ок");
            }   
            AddMainImageBtn.IsEnabled = true;
            AddImageBtn.IsEnabled = true;
        }
       

        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            foreach(var item in imagesCollectionView.SelectedItems)
            {
                _viewModel.Images.Remove(item as ImageItem);
            }
            imagesCollectionView.SelectedItems.Clear();
            DeleteImageBtn.IsEnabled = false;
        }

        private void ImagesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(imagesCollectionView.SelectedItems.Count > 0)
            {
                DeleteImageBtn.IsEnabled = true;
            }
            else
            {
                DeleteImageBtn.IsEnabled = false;
            }
        }
    }
}