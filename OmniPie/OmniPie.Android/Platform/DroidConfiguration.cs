using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniPie.Interfaces;

namespace OmniPie.Droid.Platform
{
    public class DroidConfiguration : IConfiguration
    {
        public void AddOrUpdate(string key, string data)
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate(string key, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate(string key, int data)
        {
            throw new NotImplementedException();
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