using Plugin.CloudFirestore;
using RSAMessenger.Entities;
using System;
using System.Collections;
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
    public partial class FindUser : ContentPage
    {
        User user;
        public FindUser()
        {
            InitializeComponent();
            findUserButton.Clicked += FindUserButton_Clicked;
            startchatButton.Clicked += StartchatButton_Clicked;
            nicknameEntry.Text = "";
            statusLabel.Text = "";
        }

        private async void StartchatButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var yourDocument = await CrossCloudFirestore.Current
                                            .Instance
                                            .Collection("CreatedChats")
                                            .Document(Preferences.Get("Nickname", null))
                                            .GetAsync();
                ArrayList yourChatList;
                var yourItems = yourDocument.ToObject<ChatList>().chats;
                if(!(yourItems is null)){
                    yourChatList = new ArrayList(yourItems);
                    foreach(var chat in yourChatList)
                    {
                        if (chat.Equals($"{Preferences.Get("Nickname", null)}-{user.UserName}"))
                            throw new Exception();
                        if (chat.Equals($"{user.UserName}-{Preferences.Get("Nickname", null)}"))
                            throw new Exception();
                    }
                }
                else
                {
                    yourChatList = new ArrayList();
                }
                yourChatList.Add($"{Preferences.Get("Nickname", null)}-{user.UserName}");
                var opponentDocument = await CrossCloudFirestore.Current
                            .Instance
                            .Collection("CreatedChats")
                            .Document(user.UserName)
                            .GetAsync();
                ArrayList opponentChatList;
                var opponentItems = opponentDocument.ToObject<ChatList>().chats;
                if (!(opponentItems is null))
                {
                    opponentChatList = new ArrayList(opponentItems);
                    foreach (var chat in opponentChatList)
                    {
                        if (chat.Equals($"{Preferences.Get("Nickname", null)}-{user.UserName}"))
                            throw new Exception();
                        if (chat.Equals($"{user.UserName}-{Preferences.Get("Nickname", null)}"))
                            throw new Exception();
                    }
                }
                else
                {
                    opponentChatList = new ArrayList();
                }
                opponentChatList.Add($"{Preferences.Get("Nickname", null)}-{user.UserName}");
                await CrossCloudFirestore.Current
                             .Instance
                             .Collection("CreatedChats")
                             .Document(Preferences.Get("Nickname", null))
                             .UpdateAsync(new { chats = yourChatList });

                await CrossCloudFirestore.Current
                             .Instance
                             .Collection("CreatedChats")
                             .Document(user.UserName)
                             .UpdateAsync(new { chats = opponentChatList });
                await CrossCloudFirestore.Current
                         .Instance
                         .Collection("Chats")
                         .Document($"{Preferences.Get("Nickname", null)}-{user.UserName}")
                         .SetAsync(new IChat(){
                             message = "",
                             senderName = "",
                             users = new string[] { Preferences.Get("Nickname", null), user.UserName
                             }
                         });
                await Navigation.PushAsync(new Chats());
            }
            catch
            {
                App.Current.MainPage.DisplayAlert("Error", $"Can't create chat with {user.UserName}", "OK");
            }
        }

        private async void FindUserButton_Clicked(object sender, EventArgs e)
        {
            statusLabel.Text = "";
            try
            {
                var userData = await CrossCloudFirestore.Current
                                         .Instance
                                         .Collection("Users")
                                         .Document(nicknameEntry.Text)
                                         .GetAsync();
                user = userData.ToObject<User>();
                if (!(user is null))
                {
                    statusLabel.TextColor = Color.Green;
                    statusLabel.Text = "FOUND";
                    startchatButton.IsEnabled = true;
                }
                else
                {
                    startchatButton.IsEnabled = false;
                    statusLabel.TextColor = Color.Red;
                    statusLabel.Text = "NOT FOUND";
                    App.Current.MainPage.DisplayAlert("Error", $"Can't find {nicknameEntry.Text}", "OK");
                }
            }
            catch
            {
                startchatButton.IsEnabled = false;
                statusLabel.TextColor = Color.Red;
                statusLabel.Text = "NOT FOUND";
                App.Current.MainPage.DisplayAlert("Error", $"Can't find {nicknameEntry.Text}", "OK");
            }
        }
    }
}