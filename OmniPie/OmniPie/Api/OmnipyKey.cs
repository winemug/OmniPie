using System;
using System.Collections.Generic;
using System.Text;
using OmniPie.Definitions;
using OmniPie.Interfaces;
using Xamarin.Forms;

namespace OmniPie.Api
{
    public class OmniPyKey
    {
        private byte[] keyData;
        public OmniPyKey(string passphrase)
        {
            keyData = GetKeyData(passphrase);
        }

        public OmniPyKey(byte[] keyData)
        {
            keyData = this.keyData;
        }

        public static OmniPyKey Load()
        {
            var keyData = DependencyService.Get<IConfiguration>()
                .GetBytes(ConfigurationConstants.OmniPyKey);

            return new OmniPyKey(keyData);
        }

        public void Save()
        {
            DependencyService.Get<IConfiguration>()
                .AddOrUpdate(ConfigurationConstants.OmniPyKey, keyData);
        }

        private byte[] GetKeyData(string passphrase)
        {
            throw new NotImplementedException();
        }

    }
}
