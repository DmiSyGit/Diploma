using AppUpdatedXamarin.Views;
using System;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AppUpdatedXamarin.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string password;
        private bool isLogged;
        private bool isNeedHelp;
        private Page LogInPage;
        public Command LoginCommand { get; }
        public Command LogoutCommand { get; }
        public Command AddNewItemCommand { get; }
        public Command RedactItemsCommand { get; }
        public Command GoСontactCommand { get; }

        public LoginViewModel(Page pageLogin)
        {
            LogInPage = pageLogin;
            LoginCommand = new Command(OnLoginClicked);
            LogoutCommand = new Command(OnLogoutClicked);
            AddNewItemCommand = new Command(OnAddNewItemClicked);
            RedactItemsCommand = new Command(OnRedactItemsClicked);
            GoСontactCommand = new Command(OnGoСontactClicked);

            string passwordValue = Preferences.Get("userPassword_key", "");
            password = passwordValue;
            isLogged = Preferences.Get("userLogged_key", false);
            IsNeedHelp = Preferences.Get("userLogged_key", false);
        }
        public bool IsLogged
        {
            get => isLogged;
            set
            {
                SetProperty(ref isLogged, value);
                IsNeedHelp = value;
            }
        }
        public bool IsNeedHelp
        {
            get => isNeedHelp;
            set => SetProperty(ref isNeedHelp, !value);
        }
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }
        private async void OnLoginClicked(object obj)
        {
            if (password != "")
            {
                try
                {
                    if (await TryLogInAsync("LogIn%" + password))
                    {
                        Preferences.Set("userPassword_key", password);
                        IsLogged = true;
                        Preferences.Set("userLogged_key", true);
                    }
                    else
                    {
                        await LogInPage.DisplayAlert("Ошибка", "Такой ключ не существует!", "OK");
                    }
                }
                catch(Exception ex)
                {
                    await LogInPage.DisplayAlert("Ошибка", "Не удалось выполнить запрос", "OK");
                }
            }
            else
            {
                await LogInPage.DisplayAlert("Ошибка", "Требуется ввести ключ", "OK");
            }
        }
        private void OnLogoutClicked(object obj)
        {
            Preferences.Remove("userPassword_key");
            Preferences.Remove("userLogged_key");
            IsLogged = false;
            Password = "";
        }
        private async void OnAddNewItemClicked(object obj)
        {
            LogInPage.IsEnabled = false;
            await Shell.Current.GoToAsync(nameof(NewItemPage), true);
            LogInPage.IsEnabled = true;
        }
        private async void OnRedactItemsClicked(object obj)
        {
            LogInPage.IsEnabled = false;
            await Shell.Current.GoToAsync("ItemsPageShop?PasswordKey=" + Preferences.Get("userPassword_key", "no"), true);
            LogInPage.IsEnabled = true;
        }
        private async void OnGoСontactClicked(object obj)
        {
            try
            {
                await Browser.OpenAsync(new Uri("https://vk.com/m_a__z__a_y"), BrowserLaunchMode.SystemPreferred); 
            }
            catch (Exception ex)
            {
                // Произошла непредвиденная ошибка. На устройстве не может быть установлен браузер.
                await LogInPage.DisplayAlert("Ошибка", "Произошла непредвиденная ошибка. На устройстве не может быть установлен браузер!", "OK");
            }
        }
        async static Task<bool> TryLogInAsync(string request)
        {
            ClientWebSocket webSocketClient = new ClientWebSocket();
            await webSocketClient.ConnectAsync(new Uri((string)Application.Current.Resources["ServerIp"]), CancellationToken.None);

            ArraySegment<byte> arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(request));
            await webSocketClient.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            byte[] buffer = new byte[1000];
            arraySegment = new ArraySegment<byte>(buffer);
            await webSocketClient.ReceiveAsync(arraySegment, CancellationToken.None);
            string response = Encoding.UTF8.GetString(arraySegment.Array).TrimEnd(' ', '\0');
            if(response == "Correctly")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
