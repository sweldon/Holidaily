﻿using System;
using Xamarin.Forms;
using EventApp.Models;
using EventApp.ViewModels;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
#if __IOS__
using UIKit;
#endif
namespace EventApp.Views
{
    public partial class HolidayDetailPage : ContentPage
    {

        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
            set
            {
                if (Settings.IsLoggedIn == value)
                    return;
                Settings.IsLoggedIn = value;
                OnPropertyChanged();
            }
        }

        public bool isPremium
        {
            get { return Settings.IsPremium; }
            set
            {
                if (Settings.IsPremium == value)
                    return;
                Settings.IsPremium = value;
                OnPropertyChanged();
            }
        }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
            set
            {
                if (Settings.CurrentUser == value)
                    return;
                Settings.CurrentUser = value;
                OnPropertyChanged();
            }
        }


        public string devicePushId
        {
            get { return Settings.DevicePushId; }
            set
            {
                if (Settings.DevicePushId == value)
                    return;
                Settings.DevicePushId = value;
                OnPropertyChanged();
            }
        }

        HolidayDetailViewModel viewModel;

        public Comment Comment { get; set; }


        public HolidayDetailPage(HolidayDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = this.viewModel = viewModel;
            // Remove when reply button added
            HolidayDetailList.ItemSelected += OnCommentSelected;

            // Infinite scrolling for comments
            HolidayDetailList.ItemAppearing += (sender, e) =>
            {
                
                if (viewModel.IsBusy || viewModel.GroupedCommentList.Count == 0)
                {
                    return;
                }
                LoadingCommentsDialog.IsVisible = true;
                var group = e.Item as CommentList;
                if (viewModel.GroupedCommentList.Last() == group)
                {
                    viewModel.GetMoreComments.Execute(null);
                }
                LoadingCommentsDialog.IsVisible = false;
            };
            

            // TODO: just add comment to sublist dont refresh the whole thing
            //MessagingCenter.Subscribe<HolidayDetailPage, Object[]>(this,
            //"UpdateCelebrateStatus", (sender, data) => {
            //    UpdateCelebrateStatus((string)data[0], (bool)data[1], (string)data[2]);
            //});

        }



        async void OnDeleteTapped(object sender, EventArgs args)
        {

            if (!isLoggedIn)
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                try
                {
                    var item = (sender as Label).BindingContext as Comment;
                    if (String.Equals(item.UserName, currentUser,
                           StringComparison.OrdinalIgnoreCase))
                    {
                        var deleteComment = await DisplayAlert("Delete Forever",
                        "Are you sure you want to delete this comment?", "Yes", "No");
                        if (deleteComment)
                        {

                            var values = new Dictionary<string, string>{
                           { "username", currentUser },
                           { "delete", item.Id },
                           { "device_id", devicePushId }
                            };

                            var content = new FormUrlEncodedContent(values);
                            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/", content);

                            var responseString = await response.Content.ReadAsStringAsync();
                            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                            int status = responseJSON.status;
                            string message = responseJSON.message;
                            if (status == 200)
                            {
                                item.UserName = "[deleted]";
                                item.Content = "[deleted]";
                                item.ShowReply = "False";
                                item.ShowDelete = "False";

                                // Disable voting
                                item.Enabled = false;
                                item.ElementOpacity = .2;
                                //MessagingCenter.Send(this, "UpdateComments");
                                //await Navigation.PopAsync();
                            }
                            else
                            {
                                await DisplayAlert("Error", message, "OK");
                            }

                        }

                    }
                }
                catch
                {
                    await DisplayAlert("Error", "Please try that again", "OK");
                }
            }
        }

        async void OnReplyTapped(object sender, EventArgs args)
        {

            if (!isLoggedIn)
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                var item = (sender as Label).BindingContext as Comment;
                await Navigation.PushAsync(new CommentPage(new CommentViewModel(item.Id, viewModel.Holiday.Id)));
            }
        }

        //private void OnItemTapped(object sender, ItemTappedEventArgs e)
        //{
        //    var comment = e.Item as Comment;
        //    var commentGroup = e.Group as CommentList;
        //    var commentIndexInGroup = commentGroup.IndexOf(comment);
        //    var entireList = (HolidayDetailList.ItemsSource as List<CommentList>).IndexOf(commentGroup);
        //    var commentId = comment.Id;
        //    Debug.WriteLine("Tapped comment id: " + commentId);
        //    // if entireList entireList == page, load more

        //}

        public void OnCommentSelected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            //var item = args.SelectedItem as Comment;
        }

        public HolidayDetailPage()
        {
            InitializeComponent();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.GroupedCommentList.Count == 0)
                viewModel.LoadHolidayComments.Execute(null);
            AdBanner.IsVisible = !isPremium;
            try
            {
                viewModel.Holiday = await viewModel.HolidayStore.GetHolidayById(viewModel.HolidayId);
                HolidayImageSource.Source = viewModel.Holiday.HolidayImage;
                if (!string.IsNullOrEmpty(viewModel.Holiday.Description))
                    Description.Text = viewModel.Holiday.Description;
                else
                    Description.Text = "This holiday has no information yet!";
                this.Title = viewModel.Holiday.Name;
                CurrentVotes.Text = viewModel.Holiday.Votes.ToString() + " Celebrating!";

                if (isLoggedIn)
                {
                    UpVoteImage.Source = viewModel.Holiday.CelebrateStatus;
                }

                HolidayDetailList.IsVisible = true;
            }
            catch
            {
                await DisplayAlert("Error", "We couldn't fetch the data for this holiday", "OK");
                await Navigation.PopAsync();
            }
           
        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            if (!isLoggedIn)
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new NewCommentPage(viewModel.Holiday)));
                this.IsEnabled = true;
            }
            this.IsEnabled = true;


        }

        public void Share(object sender, EventArgs args)
        {
            this.IsEnabled = false;

            var holiday = viewModel.Holiday;

            var holidayName = holiday.Name;
            var timeSince = holiday.Date;
            var holidayLink = App.HolidailyHost + "/holiday?id=" + holiday.Id;
            string preface = "It's " + holidayName + "! ";
            string HolidayDescriptionShort = holiday.Description.Length <= 90 ? preface + holiday.Description +
                "\nSee more! " : preface + holiday.Description.Substring(0, 90) + "...\nSee more! ";

            if (!CrossShare.IsSupported)
                return;

            CrossShare.Current.Share(new ShareMessage
            {
                Title = holidayName,
                Text = HolidayDescriptionShort,
                Url = holidayLink
            });

            this.IsEnabled = true;

        }

        async void AddComment(object sender, EventArgs args)
        {
            
            this.IsEnabled = false;
            bool allowed = Time.ActiveHoliday(viewModel.Holiday.TimeSince);
            if (allowed)
            {
                if (!isLoggedIn)
                {
                    await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                }
                else
                {
                    this.IsEnabled = false;
                    await Navigation.PushModalAsync(new NavigationPage(new NewCommentPage(viewModel.Holiday)));
                    this.IsEnabled = true;
                }
            }
            else
            {
                await DisplayAlert("Sorry!", "We currently restrict new comments to holidays in the past week.", "OK");
            }
            this.IsEnabled = true;

        }


        async void UpVote(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            #if __IOS__
                            var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                            haptic.Prepare();
                            haptic.ImpactOccurred();
                            haptic.Dispose();
            #endif

            #if __ANDROID__
                                    var duration = TimeSpan.FromSeconds(.025);
                                    Xamarin.Essentials.Vibration.Vibrate(duration);
            #endif

            string VotesString = CurrentVotes.Text;
            int votesInt = Int32.Parse(VotesString.Split(null)[0]);
            int newVotesInt = Int32.Parse(VotesString.Split(null)[0]);
            var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;


            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                if (UpVoteIcon == "celebrate_active.png")
                {
                    // Undo

                    newVotesInt -= 1;
                    CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                    UpVoteImage.Source = "celebrate.png";
                    await UpVoteImage.ScaleTo(2, 50);
                    await UpVoteImage.ScaleTo(1, 50);
                    await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "3");


                    Object[] values = { viewModel.Holiday.Name, false, newVotesInt.ToString() };
                    MessagingCenter.Send(this, "UpdateCelebrateStatus", values);
                }
                else
                {
                    // Only allow if user hasnt already downvoted
                    newVotesInt += 1;
                    if (newVotesInt <= votesInt + 1 && newVotesInt >= votesInt - 1)
                    {
                        CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                        UpVoteImage.Source = "celebrate_active.png";
                        await UpVoteImage.ScaleTo(2, 50);
                        await UpVoteImage.ScaleTo(1, 50);
                        await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "1");


                    Object[] values = { viewModel.Holiday.Name, true, newVotesInt.ToString() };
                        MessagingCenter.Send(this, "UpdateCelebrateStatus", values);
                }
                else
                {
                    newVotesInt -= 2;
                    CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                    UpVoteImage.Source = "celebrate.png";
                    await UpVoteImage.ScaleTo(2, 50);
                    await UpVoteImage.ScaleTo(1, 50);
                    await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "5");

    
                    Object[] values = { viewModel.Holiday.Name, false, newVotesInt.ToString() };
                    MessagingCenter.Send(this, "UpdateCelebrateStatus", values);

                }

            }

            }
            this.IsEnabled = true;

        }


        async void DownVoteComment(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            //var voteStatus = (sender as Image).Source;
            var item = (sender as Image).BindingContext as Comment;
            string commentId = item.Id;

            #if __IOS__
                        var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                        haptic.Prepare();
                        haptic.ImpactOccurred();
                        haptic.Dispose();
            #endif

            #if __ANDROID__
                        var duration = TimeSpan.FromSeconds(.025);
                        Xamarin.Essentials.Vibration.Vibrate(duration);
            #endif

            int CurrentVotes = item.Votes;

            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                if (item.UpVoteStatus == "up_active.png")
                {
                    item.Votes -= 2;
                    item.UpVoteStatus = "up.png";
                    item.DownVoteStatus = "down_active.png";
                    await viewModel.CommentStore.VoteComment(commentId, "5");
                }
                else
                {
                    if (item.DownVoteStatus == "down_active.png")
                    {
                        // Undo
                        item.Votes += 1;
                        item.DownVoteStatus = "down.png";
                        await viewModel.CommentStore.VoteComment(commentId, "2");
                    }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        int newVotes = item.Votes - 1;
                        if (newVotes <= item.Votes + 1 && newVotes >= item.Votes - 1)
                        {
                            item.Votes -= 1;
                            item.DownVoteStatus = "down_active.png";
                            await viewModel.CommentStore.VoteComment(commentId, "0");
                        }
                        else
                        {
                            // Undo
                            item.Votes += 2;
                            item.DownVoteStatus = "down.png";
                            await viewModel.CommentStore.VoteComment(commentId,"4");
                        }
                    }
                }

            }
            this.IsEnabled = true;


        }

        async void UpVoteComment(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            var item = (sender as Image).BindingContext as Comment;
            string commentId = item.Id;

            #if __IOS__
                            var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                            haptic.Prepare();
                            haptic.ImpactOccurred();
                            haptic.Dispose();
            #endif

            #if __ANDROID__
                        var duration = TimeSpan.FromSeconds(.025);
                        Xamarin.Essentials.Vibration.Vibrate(duration);
            #endif

            int CurrentVotes = item.Votes;

            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                if (item.DownVoteStatus == "down_active.png")
                {
                    Debug.WriteLine("Down was active upvoting twice");
                    item.Votes += 2;
                    item.DownVoteStatus = "down.png";
                    item.UpVoteStatus = "up_active.png";
                    await viewModel.CommentStore.VoteComment(commentId, "4");
                }
                else
                {
                    if (item.UpVoteStatus == "up_active.png")
                    {
                        // Undo

                        item.Votes -= 1;
                        item.UpVoteStatus = "up.png";
                        await viewModel.CommentStore.VoteComment(commentId, "3");
                    }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        int newVotes = item.Votes + 1;
                        if (newVotes <= item.Votes + 1 && newVotes >= item.Votes - 1)
                        {
                            item.Votes += 1;
                            item.UpVoteStatus = "up_active.png";
                            await viewModel.CommentStore.VoteComment(commentId, "1");
                        }
                        else
                        {

                            item.Votes -= 2;
                            item.UpVoteStatus = "up.png";
                            await viewModel.CommentStore.VoteComment(commentId, "5");
                        }

                    }


                }
            }
            this.IsEnabled = true;

        }


    }
}
