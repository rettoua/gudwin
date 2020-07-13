using System;
using System.Collections;
using Smartline.Mapping;
using Smartline.Common.Runtime;
using Smartline.Server.Runtime.Monitoring;
using Smartline.Server.Runtime.TrackerEngine;
using Smartline.Server.Runtime.TransportLayout;

namespace Smartline.Server.Runtime.Package {
    public class ProtocolEngine {
        private readonly ProtocolSpecification _protocolSpecific = new ProtocolSpecification();

        internal void Parse(TemporaryForIncomingPackages item, GpHandler store) {
            if (store.Tracker.Id == 6000) {
                Logger.WriteDemo(item.Buffer);
            } else {
                Logger.Write(item.Buffer);
            }
            int typeCommand = item.Buffer[_protocolSpecific.TYPE_COMMAND_OFFSET.NoByteStart];

            switch (typeCommand) {
                case 48: {
                        GpsSignal gpsSignal = null;
                        if (item.Buffer[10] == 52 && item.Buffer[11] == 49) {
                            gpsSignal = new GpsSignal { Active = true };
                        } else if (item.Buffer[10] == 52 && item.Buffer[11] == 50) {
                            gpsSignal = new GpsSignal { Active = false };
                        }
                        if (gpsSignal != null) {
                            gpsSignal.Date = DateTime.Now;
                            store.SetGpsSignal(gpsSignal);
                            //StatisticController.Instance.AddNewPackage(item.TrackerId, 48);
                        }
                    }
                    break;
                //команда передачи обычных gps координат
                case 51: {
                        if (IsNormal51Package(item.Buffer)) {
                            Gp gp = ParseGpsPackage(item.Buffer, store.Tracker); if (gp != null) {
                                gp.TrackerId = store.Tracker.Id;
                                store.SetNewPoint(gp);
                            }
                        }
                        //StatisticController.Instance.AddNewPackage(item.TrackerId, 51);
                    }
                    break;
                //команда выполнения
                case 53: {
                        //TrackerServiceInfo serviceInfo = ParseTrackerServiceInfoPackage(item.Buffer);
                        //store.SetServiceInfo(serviceInfo);
                        //item.UserToken.NewPackage(53);
                        //StatisticController.Instance.AddNewPackage(item.TrackerId, 53);
                    }
                    break;
            }
        }

        private static int GetTimeZoneOffset() {
            try {
                return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
            } catch {
                return 2;
            }
        }

        public static Gp ParseGpsPackage(byte[] package, Tracker tracker) {
            try {
                var newGps = new Gp {
                    SendTime = package.ParseSendTime().AddHours(GetTimeZoneOffset()),
                    Latitude = package.ParseLatitude(),
                    Longitude = package.ParseLongitude(),
                    Speed = Math.Round(package.ParseSpeed(), 2),
                    Distance = package.ParseDistance(),
                    Sensors = package.ParseSensor(tracker),
                    Battery = package.ParseBatteryState(),
                    SOS1 = package.ParseSos1(),
                    SOS2 = package.ParseSos2()
                };
                return newGps;
            } catch (Exception exception) {
                Logger.Write(exception, package);
                return null;
            }
        }

        private static bool IsNormal51Package(byte[] package) {
            decimal latitude = package.ParseLatitude();
            if (latitude == 0m) {
                return false;
            }
            return true;
        }

        // Формат пакета перехода с режима "Свободно" в режим "Занято":											
        //[$],[$],[$],[0],[4 байта идентификационного кода устройства],[1],[1],[18 байт заполненные 0-ми],[&],[&],[&]

        private RelayAction ParseRelayActionPackage(byte[] package) {
            var relayAction = new RelayAction {
                Date = package.ParseSendTime(),
                RelayIndex = package[10],
                IsOn = package[11] == 1,
                Executing = false,
                Executed = true
            };
            return relayAction;
        }

        public static TrackerServiceInfo ParseTrackerServiceInfoPackage(byte[] package) {
            var serviceInfo = new TrackerServiceInfo {
                SendTime = package.ParseSendTime().AddHours(GetTimeZoneOffset()),
                HasGpsSignal = ByteToBoolean(package[10]),
                RestartAfterRegisterGsm = ByteToBoolean(package[11]),
                RestartWhileConnection = ByteToBoolean(package[12]),
                RegistrationGps = ByteToBoolean(package[13]),
                ResetWhileOnPower = ByteToBoolean(package[14]),
                ResetOnWdt = ByteToBoolean(package[16]),
                ResetOnUndefinedExpection = ByteToBoolean(package[17]),
                ResetOnMclr = ByteToBoolean(package[18]),
                ResetOnPower = ByteToBoolean(package[19]),
                ResetOnConnectionLost = ByteToBoolean(package[20]),
                ResetGsm = ByteToBoolean(package[21]),
                ResetAbsentOk = ByteToBoolean(package[22]),
                ResetOnErrorCommand = ByteToBoolean(package[23])
            };
            return serviceInfo;
        }

        private static bool ByteToBoolean(byte b) {
            return b == 1;
        }


    }
}