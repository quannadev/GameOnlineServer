using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public static class GameHelper
    {
        public static string ParseString<T>(T data)
        {
           return JsonConvert.SerializeObject(data);
        }
        public static string RandomString(int len)
        {
            var rdn = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid() + $"{DateTime.Now}"));
            return rdn[..len];
        }
        public static T ParseStruct<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static string HashPassword(string txt)
        {
            var crypt = new SHA256Managed();
            var hash = string.Empty;
            var bytes = crypt.ComputeHash(Encoding.UTF8.GetBytes(txt));
            return bytes.Aggregate(hash, (current, theByte) => current + theByte.ToString("x2"));
        }
        public static void Shuffle<T>(this IList<T> list)  
        {  
            var rng = new Random(); 
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
}