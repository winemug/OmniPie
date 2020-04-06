using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using OmniPie.Interfaces;

namespace OmniPie.Droid.Platform
{
    public class DroidConfiguration : IConfiguration
    {
        private readonly ISharedPreferences SharedPreferences;
        public DroidConfiguration(ISharedPreferences sharedPreferences)
        {
            SharedPreferences = sharedPreferences;
        }
        public void AddOrUpdate(string key, string data)
        {
            SharedPreferences.Edit().PutString(key, data).Commit();
        }

        public void AddOrUpdate(string key, byte[] data)
        {
            var dataString = Base64.EncodeToString(data, Base64Flags.Default);
            SharedPreferences.Edit().PutString(key, dataString).Commit();
        }

        public void AddOrUpdate(string key, int data)
        {
            SharedPreferences.Edit().PutInt(key, data).Commit();
        }

        public void AddOrUpdate(string key, long data)
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate(string key, bool data)
        {
            throw new NotImplementedException();
        }

        public string GetString(string key)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes(string key)
        {
            throw new NotImplementedException();
        }

        public int? GetInt(string key)
        {
            throw new NotImplementedException();
        }

        public long? GetLong(string key)
        {
            throw new NotImplementedException();
        }

        public bool? GetBool(string key)
        {
            throw new NotImplementedException();
        }
    }
}