using Plugin.CloudFirestore;
using RSAMessenger.Core;
using RSAMessenger.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RSAMessenger
{
    public partial class MainPage : ContentPage
    {
        string nickname;
        RSAKeys keys;
        public MainPage()
        {
            InitializeComponent();
            findUserButton.Clicked += FindUserButton_Clicked;
            chatsButton.Clicked += ChatsButton_Clicked;
            saveButton.Clicked += SaveButton_Clicked;
            resetButton.Clicked += ResetButton_Clicked;
            

        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                Task.Run(async () =>
                {
                    await CrossCloudFirestore.Current
                             .Instance
                             .Collection("Users")
                             .Document(nickname)
                             .DeleteAsync();
                    var chatListDictionary = await CrossCloudFirestore.Current
                        .Instance
                        .Collection("CreatedChats")
                        .Document(Preferences.Get("Nickname", null))
                        .GetAsync();
                    var chatList = chatListDictionary.ToObject<ChatList>().chats;
                    try
                    {
                        foreach (var item in chatList)
                        {
                            await CrossCloudFirestore.Current
                                 .Instance
                                 .Collection("Chats")
                                 .Document(item)
                                 .DeleteAsync();
                        }
                    }
                    catch
                    {

                    }
                    await CrossCloudFirestore.Current
                             .Instance
                             .Collection("CreatedChats")
                             .Document(nickname)
                             .DeleteAsync();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        nickname = null;
                        keys = null;
                        Preferences.Remove("PrivateKey");
                        Preferences.Remove("Nickname");
                        nicknameEntry.IsEnabled = true;
                        findUserButton.IsEnabled = false;
                        chatsButton.IsEnabled = false;
                        saveButton.IsEnabled = true;
                        nicknameEntry.IsEnabled = true;
                        statusLabel.TextColor = Color.Red;
                        statusLabel.Text = "OFFLINE";
                        App.Current.MainPage.DisplayAlert("Successfully", "Deleated", "OK");
                    });
                });
                statusLabel.TextColor = Color.DarkGoldenrod;
                statusLabel.Text = "DELEATING...";
                resetButton.IsEnabled = false;
            }
            catch
            {
                App.Current.MainPage.DisplayAlert("Error", "Can't deleate", "OK");
            }
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!(nicknameEntry.Text is null))
                {
                    Task.Run(async () =>
                    {
                        var userData = await CrossCloudFirestore.Current
                                                     .Instance
                                                     .Collection("Users")
                                                     .Document(nicknameEntry.Text)
                                                     .GetAsync();
                        if (!userData.Exists)
                        {
                            nickname = nicknameEntry.Text;
                            keys = new RSALibrary().createRSAKeys();
                            await CrossCloudFirestore.Current
                                         .Instance
                                         .Collection("Users")
                                         .Document(nickname)
                                         .SetAsync(new User
                                         {
                                             UserName = nickname,
                                             PublicKey = keys.Public
                                         });
                            await CrossCloudFirestore.Current
                                        .Instance
                                        .Collection("CreatedChats")
                                        .Document(nickname)
                                        .SetAsync(new {});
                            Preferences.Set("PrivateKey", keys.Private);
                            Preferences.Set("Nickname", nickname);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                nicknameEntry.IsEnabled = false;
                                findUserButton.IsEnabled = true;
                                chatsButton.IsEnabled = true;
                                resetButton.IsEnabled = true;
                                statusLabel.TextColor = Color.Green;
                                statusLabel.Text = "ONLINE";
                                App.Current.MainPage.DisplayAlert("Successfully", "Saved\n\n!!!Importantly!!!\nOnce you're done, you'll need to delete your account to remain anonymous.", "OK");
                            });
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                saveButton.IsEnabled = true;
                                nicknameEntry.IsEnabled = true;
                                statusLabel.TextColor = Color.Red;
                                statusLabel.Text = "OFFLINE";
                                App.Current.MainPage.DisplayAlert("Error", $"{nicknameEntry.Text} is already exists", "OK");
                            });
                        }
                    });
                    statusLabel.TextColor = Color.DarkGoldenrod;
                    statusLabel.Text = "SAVING...";
                    saveButton.IsEnabled = false;
                    nicknameEntry.IsEnabled = false;
                }
                else
                {
                    App.Current.MainPage.DisplayAlert("Error", "Enter nickname", "OK");
                }
            }
            catch
            {
                App.Current.MainPage.DisplayAlert("Error", "Can't register", "OK");
            }
        }

        private async void ChatsButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Chats());
        }

        private async void FindUserButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FindUser());
        }
    }
}
