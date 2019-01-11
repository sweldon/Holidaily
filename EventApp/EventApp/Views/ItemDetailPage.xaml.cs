﻿using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EventApp.Models;
using EventApp.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemDetailPage : ContentPage
    {
        ItemDetailViewModel viewModel;
        public Comment Comment { get; set; }

    public ItemDetailPage(ItemDetailViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
        }

        public ItemDetailPage()
        {
            InitializeComponent();

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Comments.Count == 0)
                viewModel.LoadHolidayComments.Execute(null);
        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            var labelSender = (Label)sender;
            this.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new NewItemPage(viewModel.Holiday)));
            // await Navigation.PushModalAsync(new NewItemPage(new NewItemPageViewModel(viewModel.Item));
            this.IsEnabled = true;
        }


    }
}
