using Plugin.CloudFirestore;
using RSAMessenger.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RSAMessenger
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Chats : ContentPage
    {
        public Chats()
        {
            InitializeComponent();
            refreshButton.IsEnabled = false;
            refreshList();
            refreshButton.Clicked += RefreshButton_Clicked;
            chatItemList.ItemTapped += ChatItemList_ItemTapped;
        }

        private async void ChatItemList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new Chat(chatItemList.SelectedItem.ToString()));
        }

        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            refreshButton.IsEnabled = false;
            refreshList();
        }

        private async Task refreshList()
        {
            var chatListDictionary = await CrossCloudFirestore.Current
                                            .Instance
                                            .Collection("CreatedChats")
                                            .Document(Preferences.Get("Nickname", null))
                                            .GetAsync();
            var chatList = chatListDictionary.ToObject<ChatList>().chats;
            Device.BeginInvokeOnMainThread(() =>
            {
                chatItemList.ItemsSource = chatList;
                refreshButton.IsEnabled = true;
            });
        }
    }
}