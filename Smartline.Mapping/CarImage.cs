using System.Collections.Generic;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public class CarImage {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("isrecalc")]
        public bool IsDirectionCalculationRequired { get; set; }

        public static CarImage Default { get { return new CarImage { IsDirectionCalculationRequired = true, Name = "default", Title = "Автомобиль" }; } }

        public static List<CarImage> Images {
            get {
                return _carImages ??
                    (_carImages = new List<CarImage> {
                                                                            Default,
                                                                            new CarImage {IsDirectionCalculationRequired = false, Name = "circle", Title = "Окружность"}
                                                                        });
            }
        }
        private static List<CarImage> _carImages;
    }
}