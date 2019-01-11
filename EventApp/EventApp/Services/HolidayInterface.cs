﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventApp.Services
{
    public interface HolidayInterface<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
    }

}