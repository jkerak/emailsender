using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace jkerak.emailsender.Utilities
{
    public static class StringExtensions
    {
        [Deterministic]
        public static string FormatCurrency(this string value)
        {
            var amount = Convert.ToDecimal(value);
            var currencyFormat = amount.ToString("C");
            return currencyFormat.Substring(1);
        }
        
        [Deterministic]
        public static byte[] HashString(this string dataToHash)
        {
            var byteData = Encoding.Unicode.GetBytes(dataToHash);
            var hasher = new SHA256CryptoServiceProvider();
            var digest = hasher.ComputeHash(byteData);
            return digest;
        }
    }
}