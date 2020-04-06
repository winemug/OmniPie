using System;
using System.Collections.Generic;
using System.Text;

namespace OmniPie.Interfaces
{
    public interface IConfiguration
    {
        void AddOrUpdate(string key, string data);
        void AddOrUpdate(string key, byte[] data);
        void AddOrUpdate(string key, int data);
        void AddOrUpdate(string key, long data);
        void AddOrUpdate(string key, bool data);

        string GetString(string key);
        byte[] GetBytes(string key);
        int? GetInt(string key);
        long? GetLong(string key);
        bool? GetBool(string key);
    }
}
