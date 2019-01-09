﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;

namespace EventApp.Services
{
    public class MockDataStore : IDataStore<Holiday>
    {

        List<Holiday> items;
        string ec2Instance = "http://ec2-54-156-187-51.compute-1.amazonaws.com";
        HttpClient client = new HttpClient();

        public MockDataStore()
        {
            

        }

        public async Task<bool> AddItemAsync(Holiday item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Holiday item)
        {
            var oldItem = items.Where((Holiday arg) => arg.Description == item.Description).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }


        public async Task<Holiday> GetItemAsync(string description)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Description == description));
        }

        public async Task<IEnumerable<Holiday>> GetItemsAsync(bool forceRefresh = false)
        {
            items = new List<Holiday>();

            DateTime currentDate = DateTime.Today;
            string dateString = currentDate.ToString("dd-MM-yyyy");
            string dayNumber = dateString.Split('-')[0].TrimStart('0');
            int monthNumber = Int32.Parse(dateString.Split('-')[1]);

            List<string> months = new List<string>() {
                "January","February","March","April","May","June","July",
                "August", "September", "October", "November", "December"
            };

            string monthString = months[monthNumber - 1];

            var values = new Dictionary<string, string>{
                   { "month", monthString },
                   { "day", dayNumber }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(ec2Instance + "/portal/get_holidays/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            dynamic holidayList = responseJSON.HolidayList;

            foreach (var holiday in holidayList)
            {
                items.Insert(0, new Holiday() { Id = holiday.id, Name = holiday.name, Description = holiday.description });
            }

            return await Task.FromResult(items);
        }
    }
}