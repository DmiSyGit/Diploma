using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using AppUpdatedXamarin.Models;
using AppUpdatedXamarin.Views;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace AppUpdatedXamarin.ViewModels
{
    [QueryProperty(nameof(PasswordKey), nameof(PasswordKey))]
    public class ItemsViewModel : BaseViewModel
    {
        int action = 1;
        int lastAction = 0;
        private string filterParams = null;// Значение фильтров
        private string searchParameter;
        private Item _selectedItem;
        static string folderPath;
        string passwordKey = "NoKey";
        private Page itemsPage;
        static CancellationTokenSource cts = new CancellationTokenSource();
        
        public List<string> FilterList { get; set; }

        public string name = "1";
        public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AdditionalItemsLoad { get; }
        public Command<Item> ItemTapped { get; }

        public string SearchParameter
        {
            get
            {
                return searchParameter;
            }
            set
            {
                searchParameter = value;
            }
        }
        public string PasswordKey
        {
            get
            {
                return passwordKey;
            }
            set
            {
                passwordKey = value;
            }
        }
        public ItemsViewModel(Page page)
        {
            itemsPage = page;
            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            FilterList = new List<string> { "Новинки", "Сначала самые дешевые", "Сначала самые дорогие" };
            Title = "Каталог";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(() =>
            {
                cts.Cancel();
                action = 1;
            });
            AdditionalItemsLoad = new Command(AdditionalLoadingItem);
            ItemTapped = new Command<Item>(OnItemSelected);

            Task task = new Task(async() => {
                while(true)
                {
                    if(action == 1) 
                    {
                        lastAction = action;
                        action = 0;
                        Items.Clear();
                        cts = new CancellationTokenSource();
                        try
                        {
                            if (filterParams != null)
                            {
                                await ClientLaunchAsync("GetProducts%" + Items.Count + filterParams +
                                    "%" + passwordKey +   
                                    "%" + searchParameter, Items);
                            }
                        }
                        catch (Exception ex)
                        {
                             Debug.WriteLine(ex.Message);
                        }
                        finally
                        {
                            IsBusy = false;
                        }
                    }
                    else if(action == 2)
                    {
                        lastAction = action;
                        action = 0;
                        cts = new CancellationTokenSource();
                        try
                        {
                            await ClientLaunchAsync("GetProducts%" + Items.Count + filterParams +
                                    "%" + passwordKey +
                                    "%" + searchParameter, Items);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            });
            task.Start();
        }



        async static Task ClientLaunchAsync(string msg, ObservableCollection<Item> items)
        {
            string[] requestParameters = msg.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);

            ClientWebSocket webSocketClient = new ClientWebSocket();

            await webSocketClient.ConnectAsync(new Uri((string)Application.Current.Resources["ServerIp"]), cts.Token);



            var arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
            await webSocketClient.SendAsync(arraySegment, WebSocketMessageType.Text, true, cts.Token);

            while (webSocketClient.State == WebSocketState.Open)
            {
                WebSocketReceiveResult resp;
                if (requestParameters[0] == "GetProducts")
                {
                    byte[] buffer = new byte[1048576];
                    var segment = new ArraySegment<byte>(buffer);
                    resp = await webSocketClient.ReceiveAsync(segment, cts.Token);
                    string response = Encoding.UTF8.GetString(segment.Array).TrimEnd(' ', '\0');
                    if (response != "EndMessage" && response != "ErrorProducts")
                    {
                        string[] responseParameters = response.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                        items.Add(new Item(
                            responseParameters[0],
                            responseParameters[1],
                            responseParameters[2],
                            Path.Combine(folderPath, responseParameters[3]),
                            responseParameters[4]));
                        ClientLaunchAsync("GetPhoto%" + responseParameters[3], items);
                    }

                }
                else
                {
                    byte[] bufferImage = new byte[5242880];
                    var segmentImage = new ArraySegment<byte>(bufferImage);
                    resp = await webSocketClient.ReceiveAsync(segmentImage, cts.Token);
                    string response = Encoding.UTF8.GetString(segmentImage.Array).TrimEnd(' ', '\0');
                    if (response != "" && response != "PhotoMissing")
                    {
                        string[] photoParameters = msg.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                        File.WriteAllBytes(Path.Combine(folderPath, photoParameters[1]), segmentImage.Array);
                    }
                }
                if (resp.EndOfMessage)
                {
                    break;
                }
            }
        }
        
        public void OnAppearing()
        {
            SelectedItem = null;
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private void AdditionalLoadingItem(object obj)
        {
            action = 2;
        }


        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;
            itemsPage.IsEnabled = false;
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
            itemsPage.IsEnabled = true;
        }

        public void SetRequestStringParams(int paramFilter)
        {
            switch(paramFilter)
            {
                case 0:
                    filterParams = "%NewProd%";
                    break;
                case 1:
                    filterParams = "%Cheapest%";
                    break;
                case 2:
                    filterParams = "%Dearest%";
                    break;
            }
            IsBusy = false;
            IsBusy = true;
        }
    }
}