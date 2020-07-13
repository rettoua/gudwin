namespace Smartline.Server.Runtime.Package {
    public class ProtocolSpecification {
        private static ProtocolSpecification _instance;

        public static ProtocolSpecification Instance {
            get { return _instance ?? (_instance = new ProtocolSpecification()); }
            set { _instance = value; }
        }

        /// <summary>
        /// код команды передачи координат
        /// </summary>
        public ProtocolSpecification_Data TYPE_COMMAND_OFFSET = new ProtocolSpecification_Data(0, 1, 0, 8);

        #region Tracker
        /// <summary>
        /// номер идентификации устройства
        /// </summary>
        public ProtocolSpecification_Data GPS_TRACKERID = new ProtocolSpecification_Data(1, 4, 0, 8);
        /// <summary>
        /// Время. год (последняя цифра) 
        /// </summary>
        public ProtocolSpecification_Data GPS_TIME_YEAR = new ProtocolSpecification_Data(5, 1, 4, 4);
        /// <summary>
        /// Время. месяц
        /// </summary>
        public ProtocolSpecification_Data GPS_TIME_MOUNTH = new ProtocolSpecification_Data(5, 1, 0, 4);
        /// <summary>
        /// Время. Час
        /// </summary>
        public ProtocolSpecification_Data GPS_TIME_HOUR = new ProtocolSpecification_Data(7, 1, 3, 5);
        /// <summary>
        /// Время. День
        /// </summary>
        public ProtocolSpecification_Data GPS_TIME_DAY = new ProtocolSpecification_Data(6, 1, 3, 5);
        /// <summary>
        /// Время. Минуты
        /// </summary>
        public ProtocolSpecification_Data GPS_TIME_MINUTE = new ProtocolSpecification_Data(8, 1, 2, 6);
        /// <summary>
        /// Время. Секунды
        /// </summary>
        public ProtocolSpecification_Data GPS_TIME_SECOND = new ProtocolSpecification_Data(9, 1, 2, 6);
        /// <summary>
        /// Координаты. Фиксация
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_FIX = new ProtocolSpecification_Data(10, 1, 7, 1);
        /// <summary>
        /// Координаты. Полушарие 0-N/1-S
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_HEMISPHERE_NS = new ProtocolSpecification_Data(13, 1, 7, 1);
        /// <summary>
        /// Координаты. полушарие (0-E, 1-E)
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_HEMISPHERE_EW = new ProtocolSpecification_Data(17, 1, 7, 1);
        /// <summary>
        /// Координаты. Широта 
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_LATITUDE_1 = new ProtocolSpecification_Data(10, 1, 1, 7);
        /// <summary>
        /// Координаты. Широта
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_LATITUDE_2 = new ProtocolSpecification_Data(11, 1, 1, 7);
        /// <summary>
        /// Координаты. Широта
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_LATITUDE_3 = new ProtocolSpecification_Data(12, 1, 1, 7);
        /// <summary>
        /// Координаты. Широта
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_LATITUDE_4 = new ProtocolSpecification_Data(13, 1, 1, 7);
        /// <summary>
        /// Координаты. Долгота
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_LONGITUDE_1 = new ProtocolSpecification_Data(14, 1, 1, 7);
        /// <summary>
        /// Координаты. Долгота
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_LONGITUDE_2 = new ProtocolSpecification_Data(15, 1, 1, 7);
        /// <summary>
        /// Координаты. Долгота
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_LONGITUDE_3 = new ProtocolSpecification_Data(16, 1, 1, 7);
        /// <summary>
        /// Координаты. Долгота 
        /// </summary>
        public ProtocolSpecification_Data GPS_COORDINATES_LONGITUDE_4 = new ProtocolSpecification_Data(17, 1, 1, 7);
        /// <summary>
        /// Скорость до запятой
        /// </summary>
        public ProtocolSpecification_Data GPS_SPEED_HIGH = new ProtocolSpecification_Data(18, 1, 0, 8);
        /// <summary>
        /// Скорость после запятой
        /// </summary>
        public ProtocolSpecification_Data GPS_SPEED_LOW = new ProtocolSpecification_Data(19, 1, 0, 7);
        /// <summary>
        /// Направление / 2
        /// </summary>
        public ProtocolSpecification_Data GPS_DIRECTION = new ProtocolSpecification_Data(20, 1, 0, 8);
        /// <summary>
        /// <para>Высота над уровнем моря 2 старших знака</para>
        /// </summary>
        public ProtocolSpecification_Data GPS_HEIGHT_HIGH = new ProtocolSpecification_Data(21, 1, 0, 7);
        /// <summary>
        /// Высота над уровнем моря 2 младших знака
        /// </summary>
        //public ProtocolSpecification_Data GPS_HEIGHT_LOW = new ProtocolSpecification_Data(22, 1, 0, 8);
        /// <summary>
        /// <para>Флаг нахождения приёмника над/ниже уровня моря</para>
        /// </summary>
        //public ProtocolSpecification_Data GPS_HEIGHT_OVER = new ProtocolSpecification_Data(21, 1, 7, 1);
        /// <summary>
        /// Уровень бензина (Аналог)
        /// </summary>
        //public ProtocolSpecification_Data GPS_ANALOG = new ProtocolSpecification_Data(23, 1, 0, 8);
        /// <summary>
        /// Уровень бензина (Аналог)
        /// </summary>
        public ProtocolSpecification_Data GPS_DISTANCE = new ProtocolSpecification_Data(21, 2, 0, 8);
        

        #endregion

        public int AUTH_TRACKER_NUMBER_OFFSET = 1;
        public int AUTH_LOGIN_OFFSET = 5;
        public int AUTH_PASSWORD_OFFSET = 13;
    }

    public class ProtocolSpecification_Data {
        /// <summary>
        /// Часть информации в пакете
        /// </summary>
        /// <param name="NoByteStart">Номер байта начало</param>
        /// <param name="ByteCnt">байт количество</param>
        /// <param name="NoBitStart">Номер бита начало</param>
        /// <param name="BitCnt">бит количество</param>
        public ProtocolSpecification_Data(int NoByteStart, int ByteCnt, int NoBitStart, int BitCnt) {
            this.BitCnt = BitCnt;
            this.NoBitStart = NoBitStart;
            this.ByteCnt = ByteCnt;
            this.NoByteStart = NoByteStart;
        }
        /// <summary>
        /// Номер байта начало
        /// </summary>
        public int NoByteStart { get; private set; }
        /// <summary>
        /// байт количество 
        /// </summary>
        public int ByteCnt { get; private set; }
        /// <summary>
        /// Номер бита начало
        /// </summary>
        public int NoBitStart { get; private set; }
        /// <summary>
        /// бит количество
        /// </summary>
        public int BitCnt { get; private set; }
    }
}