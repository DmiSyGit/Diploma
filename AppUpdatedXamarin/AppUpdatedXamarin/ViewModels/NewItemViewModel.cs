using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using AppUpdatedXamarin.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AppUpdatedXamarin.ViewModels
{
    [QueryProperty(nameof(IsUpdate), "isUpdate")]
    [QueryProperty(nameof(NameProduct), "nameProduct")]
    [QueryProperty(nameof(DescriptionProduct), "descriptionProduct")]
    [QueryProperty(nameof(ImagesPath), "imagesPath")]
    [QueryProperty(nameof(Price), "price")]
    [QueryProperty(nameof(Sizes), "sizes")]
    [QueryProperty(nameof(CategoryId), "categoryId")]
    [QueryProperty(nameof(ItemId), "itemId")]
    public class NewItemViewModel : BaseViewModel
    {
        private string itemId;
        private string nameProduct;
        private string descriptionProduct;
        private string sizes;
        private int categoryId;
        private string price;
        public List<string> CategoryList { get; set; }
        string folderPath;
        bool isUpdate = false;
        Page newItemPage;
        public ObservableCollection<ImageItem> Images { get; set; }


        public NewItemViewModel(Page page)
        {
            CategoryList = new List<string> {"Брюки", "Рубашки", "Верхняя одежда", "Джинсы",
                "Костюмы", "Пиджаки", "Толстовки", "Футболки", "Платья", "Блузки", "Туники",
                "Юбки", "Шорты", "Кросовки", "Ботинки", "Детская обувь" , "Другое"};
            newItemPage = page;
            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            SaveCommand = new Command(OnSave, ValidateSave);
            this.PropertyChanged +=
                (_, __) => SaveCommand.ChangeCanExecute();
            Images = new ObservableCollection<ImageItem>();
        }

        private bool ValidateSave()
        {
            float priceNum;
            return !String.IsNullOrWhiteSpace(nameProduct)
                && !String.IsNullOrWhiteSpace(descriptionProduct)
                && !String.IsNullOrWhiteSpace(price) && float.TryParse(price, out priceNum) && priceNum > 0
                && !String.IsNullOrWhiteSpace(sizes);
        }

       
        private async void OnSave()
        {
            newItemPage.IsEnabled = false;
            if (Images.Count > 0)
            {
                string message;
                if (isUpdate)
                {
                    message = $"AddUpdateProduct%1%{Preferences.Get("userPassword_key", "NoKey")}%{itemId}%{nameProduct}" +
                        $"%{descriptionProduct}" +
                        $"%{sizes}" +
                        $"%{categoryId + 1}" +
                        $"%{price}" +
                        $"%{Images.Count.ToString()}";
                }
                else
                {
                    message = $"AddUpdateProduct%0%{Preferences.Get("userPassword_key", "NoKey")}%newId%{nameProduct}" +
                        $"%{descriptionProduct}" +
                        $"%{sizes}" +
                        $"%{categoryId + 1}" +
                        $"%{price}" +
                        $"%{Images.Count.ToString()}";
                }

                try
                {
                    ClientWebSocket webSocketClient = new ClientWebSocket();
                    await webSocketClient.ConnectAsync(new Uri((string)Application.Current.Resources["ServerIp"]), CancellationToken.None);

                    var arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    await webSocketClient.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

                    foreach (ImageItem item in Images)
                    {
                        ArraySegment<byte> arrray = new ArraySegment<byte>(item.ImageBytes);
                        await webSocketClient.SendAsync(arrray, WebSocketMessageType.Binary, true, CancellationToken.None);
                    }

                    byte[] buffer = new byte[10000];
                    var segment = new ArraySegment<byte>(buffer);
                    await webSocketClient.ReceiveAsync(segment, CancellationToken.None);
                    string response = Encoding.UTF8.GetString(segment.Array).TrimEnd(' ', '\0');
                    if (response == "ProductAdditionCompleted")
                    {
                        await newItemPage.DisplayAlert("Завершено", "Операция успешно выполнена!", "OK");
                        await Shell.Current.GoToAsync("..");
                        return;
                    }
                    else if (response == "ProductAdditionError")
                    {
                        await newItemPage.DisplayAlert("Ошибка", "Произошла ошибка при выполнении операции, обратитесь в тех.поддержку(vk.com/m_a__z__a_y)!", "OK");
                    }
                }
                catch (Exception messa)
                {
                    await newItemPage.DisplayAlert("Ошибка", "Неудалось выполнить операцию!", "OK");
                }
            }
            else
            {
                await newItemPage.DisplayAlert("Ошибка", "Должна быть добавлена хотя бы одна фотография!", "OK");
            }
            newItemPage.IsEnabled = true;
        }




        public Command SaveCommand { get; }
        public string NameProduct
        {
            get => nameProduct;
            set => SetProperty(ref nameProduct, value);
        }
        public string IsUpdate
        {
            set
            {
                if (value == "1")
                {
                    isUpdate = true;
                }
                else if (value == "0")
                {
                    isUpdate = false;
                }
            }
        }

        public string DescriptionProduct
        {
            get => descriptionProduct;
            set => SetProperty(ref descriptionProduct, value);
        }
        public string ImagesPath
        {
            set
            {
                string[] imagesIds = value.Split(new char[] { '!' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string imageId in imagesIds)
                {
                    string path = Path.Combine(folderPath, imageId);
                    Images.Add(new ImageItem(path, File.ReadAllBytes(path)));
                }
            }
        }
        public string ItemId
        {
            get
            {
                return itemId;
            }
            set
            {
                itemId = value;
            }
        }
        public string Price
        {
            get => price;
            set => SetProperty(ref price, value);
        }
        public string Sizes
        {
            get => sizes;
            set => SetProperty(ref sizes, value);
        }
        public string CategoryId
        {
            get => categoryId.ToString();
            set
            {
                if (int.TryParse(value, out int result))
                {
                     SetProperty(ref categoryId, result);
                }
                else
                {
                    SetProperty(ref categoryId, 0);
                }
            }
        }
    }
}
