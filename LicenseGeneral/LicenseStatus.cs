namespace Smartline.License.Common
{
    public enum LicenseStatus
    {
        [UserDescription("Цифровая подпись соответствует")] OK,
        [UserDescription("Ошибка в цифровой подписи")] ErrSign,
        [UserDescription("Нет доступа или ошибка в файле-ключе лицензии")] ErrFile,
        [UserDescription("Файл-ключ лицензии поврежден")] CorruptData,
        [UserDescription("Срок лицензионного сопровождения истек")] LicenseExpired,
        [UserDescription("Файл-ключ лицензии предназначен для другого компьютера")] AnotherPC,
        [UserDescription("Файл-ключ лицензии предназначен для другой программы")] AnotherProgram,
        [UserDescription("Файл-ключ лицензии предназначен для другой версии программы")] AnotherProgramVersion,
        ErrOther
    }
}