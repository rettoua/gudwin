using System;
using System.Globalization;
using System.IO;
using System.Net;
using Ext.Net;
using Newtonsoft.Json.Linq;

namespace Smartline.Common.Runtime {
    public class RevertGeoCoding {
        public static string Decode(decimal latitude, decimal longitude) {
            string url = string.Format("http://geocode-maps.yandex.ru/1.x/?geocode={1},{0}&results=1&format=json",
                                       latitude.ToString("00.00000", CultureInfo.InvariantCulture), longitude.ToString("00.00000", CultureInfo.InvariantCulture));
            var request = (HttpWebRequest)WebRequest.Create(url);
            try {
                request.Timeout = 2000;
                var streamReader = new StreamReader(request.GetResponse().GetResponseStream());
                return Parse(streamReader.ReadToEnd());
            } catch (Exception exception) {
                Logger.Write(exception);
            }
            return string.Empty;
        }

        private static string Parse(string responseStr) {
            try {
                var jsonObject = (JObject)JSON.Deserialize(responseStr);
                var value = jsonObject.SelectToken("response.GeoObjectCollection.featureMember[0].GeoObject.metaDataProperty.GeocoderMetaData.AddressDetails.Country.AddressLine") as JValue;
                if (value != null)
                    return value.Value.ToString();
                return string.Empty;
            } catch (Exception exception) {
                Logger.Write(exception);
            }
            return string.Empty;
        }
    }
}
