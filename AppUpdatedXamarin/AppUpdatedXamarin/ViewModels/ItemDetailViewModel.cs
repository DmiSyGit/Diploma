using System;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Xamarin.Forms;
using System.IO;
using Xamarin.Essentials;
using AppUpdatedXamarin.Views;

namespace AppUpdatedXamarin.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private Page ItemDetailPage;
        private string itemId;
        private string nameProduct;
        private string description;
        private string sizes;
        private string addressMagazin;
        private string price;
        private string imagesIds;
        private int categoryId;
        private string feedback;
        private bool isCanEdit;
        private static string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public ObservableCollection<string> images = new ObservableCollection<string>();
        public Command RedactCommand { get; }
        public Command DeleteCommand { get; }
        public Command FeedbackCommand { get; }

        public ItemDetailViewModel(Page itemDetailPage)
        {
            imagesIds = "";
            ItemDetailPage = itemDetailPage;
            RedactCommand = new Command(OnRedactItemClicked);
            DeleteCommand = new Command(OnDeleteItemClicked);
            FeedbackCommand = new Command(OnFeedbackClicked);
        }

        public string Id { get; set; }
        public bool IsCanEdit
        {
            get => isCanEdit;
            set => SetProperty(ref isCanEdit, value);
        }
        public string NameProduct
        {
            get => nameProduct;
            set => SetProperty(ref nameProduct, value);
        }
        public string Price
        {
            get => price;
            set => SetProperty(ref price, value);
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }
        public string Sizes
        {
            get => sizes;
            set => SetProperty(ref sizes, value);
        }
        public string AddressMagazin
        {
            get => addressMagazin;
            set => SetProperty(ref addressMagazin, value);
        }
        public string Feedback
        {
            get => feedback;
            set => SetProperty(ref feedback, value);
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
                LoadItemId("GetMoreInfoProduct%" + value + "%" + Preferences.Get("userPassword_key", "NoKey"));
            }
        }

        public async void LoadItemId(string message)
        {
            
            try
            {
                string[] requestParameters = message.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                ClientWebSocket webSocketClient = new ClientWebSocket();
                await webSocketClient.ConnectAsync(new Uri((string)Application.Current.Resources["ServerIp"]), CancellationToken.None);

                var arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                await webSocketClient.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

                while (webSocketClient.State == WebSocketState.Open)
                {
                    byte[] buffer = new byte[5242880];
                    var segment = new ArraySegment<byte>(buffer);

                    WebSocketReceiveResult resp;
                    if (requestParameters[0] == "GetMoreInfoProduct")
                    {
                        resp = await webSocketClient.ReceiveAsync(segment, CancellationToken.None);
                        string response = Encoding.UTF8.GetString(segment.Array).TrimEnd(' ', '\0');
                        if (response != "EndMessage")
                        {
                            string[] responseParameters = response.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                            NameProduct = responseParameters[0];
                            Description = responseParameters[1];
                            Sizes = responseParameters[2];
                            AddressMagazin = responseParameters[3];
                            Price = responseParameters[4].Replace('.',',');
                            Feedback = responseParameters[5];
                            categoryId = int.Parse(responseParameters[6])-1;
                            if (responseParameters[7] == "LockEdit")
                            {
                                IsCanEdit = false;
                            }
                            else if(responseParameters[7] == "AllowedEdit")
                            {
                                IsCanEdit = true;
                            }

                            images.Clear();
                            imagesIds = "";
                            if (responseParameters.Length > 8)
                            {
                                for(int i = 8; i < responseParameters.Length; i++)
                                {
                                    imagesIds += responseParameters[i] + "!";
                                    images.Add(Path.Combine(folderPath, responseParameters[i]));
                                    LoadItemId("GetPhoto%" + responseParameters[i]);
                                }
                            }
                        }
                        else if(response == "EndMessage")
                        {
                            break;
                        }

                    }
                    else if(requestParameters[0] == "GetPhoto")
                    {
                        resp = await webSocketClient.ReceiveAsync(segment, CancellationToken.None);
                        string response = Encoding.UTF8.GetString(segment.Array).TrimEnd(' ', '\0');
                        if (response != "" && response != "PhotoMissing")
                        {
                            string[] photoParameters = message.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                            File.WriteAllBytes(Path.Combine(folderPath, photoParameters[1]), segment.Array);
                        }
                    }
                    else if(requestParameters[0] == "DeleteProduct")
                    {
                        byte[] bufferDelete = new byte[1000];
                        ArraySegment<byte> arraySegmentDelete = new ArraySegment<byte>(bufferDelete);
                        await webSocketClient.ReceiveAsync(arraySegmentDelete, CancellationToken.None);
                        string response = Encoding.UTF8.GetString(arraySegmentDelete.Array).TrimEnd(' ', '\0');
                        if (response == "ProductRemoved")
                        {
                            await ItemDetailPage.DisplayAlert("Удаление", "Операция выполнена успешно!", "OK");
                            await Shell.Current.GoToAsync("..");
                        }
                        else if(response == "FailedRemoved")
                        {
                            await ItemDetailPage.DisplayAlert("Удаление", "Во время выполнения произошла ошибка!", "OK");
                        }
                    }
                }
            }
            catch (Exception)
            {
                await ItemDetailPage.DisplayAlert("Ошибка", "Во время выполнения операции произошла ошибка, повторите операцию позже!", "OK");
            }
        }
        private async void OnRedactItemClicked(object obj)
        {
            ItemDetailPage.IsEnabled = false;
            try
            {
                await Shell.Current.GoToAsync($"{nameof(NewItemPage)}?" +
                    $"isUpdate=1&" +
                    $"nameProduct={nameProduct}&" +
                    $"descriptionProduct={description}&" +
                    $"imagesPath={imagesIds}&" +
                    $"itemId={itemId}&" +
                    $"price={price}&" +
                    $"sizes={sizes}&" +
                    $"categoryId={categoryId.ToString()}&", true);
            }
            catch
            {
                await ItemDetailPage.DisplayAlert("Ошибка", "Во время выполнения операции произошла ошибка, повторите операцию позже!", "OK");
            }
            ItemDetailPage.IsEnabled = true;
        }
        private async void OnDeleteItemClicked(object obj)
        {
            if (await ItemDetailPage.DisplayAlert("Внимание!", "Произойдет удаление текущего товара!", "Да", "Нет"))
            {
                LoadItemId("DeleteProduct%" + ItemId + "%" + Preferences.Get("userPassword_key", "NoKey"));
            }
        }

        private async void OnFeedbackClicked(object obj)
        {
            try
            {
                await Browser.OpenAsync(new Uri(feedback), BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                // Произошла непредвиденная ошибка. На устройстве не может быть установлен браузер.
                await ItemDetailPage.DisplayAlert("Ошибка", "Произошла непредвиденная ошибка. На устройстве не может быть установлен браузер!", "OK");
            }
        }
    }
}
