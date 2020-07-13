using System;
using Newtonsoft.Json;
using Smartline.Common.Runtime;

namespace Smartline.Mapping {
    public class TrackerServiceInfo {
        [JsonProperty("id")]
        public int TrackerId { get; set; }
        /// <summary>
        /// Время отправки пакета
        /// </summary>
        [JsonProperty("time")]
        public DateTime SendTime { get; set; }
        /// <summary>
        /// Наличие приема GPS					
        /// </summary>
        [JsonProperty("a")]
        public bool HasGpsSignal { get; set; }
        /// <summary>
        /// Была перезагрузка при регистрации GSM					
        /// </summary>
        [JsonProperty("b")]
        public bool RestartAfterRegisterGsm { get; set; }
        /// <summary>
        /// Была перезагрузка во время установки соединения					
        /// </summary>
        [JsonProperty("c")]
        public bool RestartWhileConnection { get; set; }
        /// <summary>
        /// Была перерегистрация GPS					
        /// </summary>
        [JsonProperty("c1")]
        public bool RegistrationGps { get; set; }
        /// <summary>
        /// сброс при включении питания					
        /// </summary>
        [JsonProperty("d")]
        public bool ResetWhileOnPower { get; set; }
        /// <summary>
        /// сброс по WDT					
        /// </summary>
        [JsonProperty("e")]
        public bool ResetOnWdt { get; set; }
        /// <summary>
        /// сброс из-за несуществующей инструкции					
        /// </summary>
        [JsonProperty("f")]
        public bool ResetOnUndefinedExpection { get; set; }
        /// <summary>
        /// сброс из-за MCLR					
        /// </summary>
        [JsonProperty("g")]
        public bool ResetOnMclr { get; set; }
        /// <summary>
        /// сброс из-за пропадания питания	
        /// </summary>
        [JsonProperty("h")]
        public bool ResetOnPower { get; set; }
        /// <summary>
        /// перезагрузка по причине разрыва соединения	
        /// </summary>
        [JsonProperty("i")]
        public bool ResetOnConnectionLost { get; set; }
        /// <summary>
        /// перезагрузка GSM
        /// </summary>
        [JsonProperty("j")]
        public bool ResetGsm { get; set; }
        /// <summary>
        ///перезагрузка по причине отсутствия ОК					
        /// </summary>
        [JsonProperty("k")]
        public bool ResetAbsentOk { get; set; }
        /// <summary>
        ///перезагрузка по причине команды ERROR
        /// </summary>
        [JsonProperty("k1")]
        public bool ResetOnErrorCommand { get; set; }

        public GpsSignal GetGpsSignal() {
            return new GpsSignal {
                Active = HasGpsSignal,
                Date = SendTime
            };
        }
    }
}