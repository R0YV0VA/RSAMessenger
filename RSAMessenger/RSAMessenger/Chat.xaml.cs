using Plugin.CloudFirestore;
using RSAMessenger.Core;
using RSAMessenger.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RSAMessenger
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Chat : ContentPage
    {
        private string opponentPublicKey;
        private string nickname;
        private string opponentNickname;
        private string PrivateKey;
        private bool isReady = false;
        private string lastMessage;
        private ObservableCollection<string> itemList = new ObservableCollection<string>();
        private RSALibrary rsalibrary = new RSALibrary();
        private string chatName;
        public Chat(string chat)
        {
            InitializeComponent();
            nickname = Preferences.Get("Nickname", null);
            PrivateKey = Preferences.Get("PrivateKey", null);
            chatName = chat;
            chatNameLabel.Text = chat;
            messageEntry.WidthRequest = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            Task.Factory.StartNew(async () => 
            {
                var chatPropertysDictionary = await CrossCloudFirestore.Current
                            .Instance
                            .Collection("Chats")
                            .Document(chat)
                            .GetAsync();
                var chatPropertys = chatPropertysDictionary.ToObject<IChat>();
                foreach(var user in chatPropertys.users)
                {
                    if (!user.Equals(nickname))
                        opponentNickname = user;
                }
                var opponentPropertysDictionary = await CrossCloudFirestore.Current
                            .Instance
                            .Collection("Users")
                            .Document(opponentNickname)
                            .GetAsync();
                var opponentPropertys = opponentPropertysDictionary.ToObject<User>();
                opponentPublicKey = opponentPropertys.PublicKey;
                isReady = true;
            });
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    Thread.Sleep(new Random().Next(100, 501));
                    if (isReady)
                    {
                        var chatPropertysDictionary = await CrossCloudFirestore.Current
                            .Instance
                            .Collection("Chats")
                            .Document(chatName)
                            .GetAsync();
                        var messageEncrypted = chatPropertysDictionary.ToObject<IChat>().message;
                        if (chatPropertysDictionary.ToObject<IChat>().senderName.Equals(opponentNickname))
                        {
                            if (!messageEncrypted.Equals(lastMessage))
                            {
                                //itemList.Add(messageEncrypted);
                                itemList.Add(rsalibrary.Decrypt(messageEncrypted, PrivateKey));
                                lastMessage = messageEncrypted;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    Device.BeginInvokeOnMainThread(() => updateUI());
                }
            });
            sendButton.Clicked += SendButton_Clicked;
        }
        private void updateUI()
        {
            chatListView.ItemsSource = itemList;
        }
        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            string messageEncrypted = rsalibrary.Encrypt($"{nickname}==> {messageEntry.Text}", opponentPublicKey);
            itemList.Add($"{nickname} ==> {messageEntry.Text}");
            //string messageEncrypted = $"{nickname} ==> {messageEntry.Text}";
            messageEntry.Text = null;
            await CrossCloudFirestore.Current
                         .Instance
                         .Collection("Chats")
                         .Document(chatName)
                         .SetAsync(new IChat()
                         {
                             message = messageEncrypted,
                             senderName = nickname,
                             users = new string[] { nickname, opponentNickname }
                         });
            Device.BeginInvokeOnMainThread(() => updateUI());
        }
    }
}