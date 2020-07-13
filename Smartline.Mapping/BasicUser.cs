using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Couchbase.Extensions;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;


namespace Smartline.Mapping {
    /// <summary>
    /// for users uses increment key with name 'I_User'
    /// </summary>
    public class BasicUser<T> : IUser where T : Tracker {
        public BasicUser() {
            Trackers = new List<T>();
            Operators = new List<InternalUser>();
        }

        [JsonProperty("id")]
        public ulong Id { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("secret")]
        public string Secret { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("isblocked")]
        public bool IsBlocked { get; set; }
        [JsonProperty("reason")]
        public string Reason { get; set; }
        [JsonProperty("owner")]
        public string Owner { get; set; }
        [JsonProperty("isadmin")]
        public bool IsAdmin { get; set; }
        [JsonProperty("jsontype")]
        public string JsonType {
            get { return "user"; }
            internal set { }
        }
        [JsonProperty("meta")]
        public UserMeta Meta { get; set; }
        [JsonProperty("ei")]
        public EvosIntegration EvosIntegration { get; set; }

        [JsonProperty("trackers", IsReference = true)]
        public List<T> Trackers { get; set; }

        [JsonProperty("operators", IsReference = true)]
        public List<InternalUser> Operators { get; set; }

        public static string ComputeSecret(string login, string secret) {
            return BitConverter.ToString(HashAlgorithm.Create("SHA1").ComputeHash(Encoding.UTF8.GetBytes(login + secret))).Replace("-", "");
        }

        public static string ComputeSecretNew(string secret) {
            return BitConverter.ToString(HashAlgorithm.Create("SHA1").ComputeHash(Encoding.UTF8.GetBytes(secret + secret))).Replace("-", "");
        }

        public bool Update() {
            return (CouchbaseManager.Main.StoreJson(StoreMode.Set, Id + "", this));
        }
    }
}