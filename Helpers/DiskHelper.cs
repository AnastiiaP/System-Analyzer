using SystemAnalyzer.Configurations;
using SystemAnalyzer.Converters;

namespace SystemAnalyzer.Helpers
{
    public class DiskHelper
    {
        private AppSetting _appSetting;
        private ILogger<Worker> _logger;

        public DiskHelper(ILogger<Worker> logger, AppSetting appSetting)
        {
            _logger = logger;
            _appSetting = appSetting;
        }

        public long AnalyzeDiskSpace()
        {
            string driveName = _appSetting.Disk;
            DriveInfo d = new DriveInfo(driveName);
            if (d.IsReady)
            {
                _logger.LogInformation("{time} диск: {1}", DateTimeOffset.Now, d.Name);
                _logger.LogInformation("{time} файлова система: {1}", DateTimeOffset.Now, d.VolumeLabel);
                _logger.LogInformation("{time} доступний простір для поточного користувача: {1} мегабайт", DateTimeOffset.Now, ByteConverter.BytesToMegabytes(d.AvailableFreeSpace));
                _logger.LogInformation("{time} загальний доступний простір: {1} мегабайт", DateTimeOffset.Now, ByteConverter.BytesToMegabytes(d.TotalFreeSpace));
                _logger.LogInformation("{time} загальний розмір диска: {1} мегабайт", DateTimeOffset.Now, ByteConverter.BytesToMegabytes(d.TotalSize));

                return d.AvailableFreeSpace;
            }

            return 0;
        }

        public long AnalyzeChangingDiskMemory(long lastAvailableDiskSpace)
        {
            long triggerSpaceBytes;
            bool success = long.TryParse(_appSetting.TriggerSpaceBytes, out triggerSpaceBytes);
            string driveName = _appSetting.Disk;

            DriveInfo d = new DriveInfo(driveName);

            long freeVolumeDifference = Math.Abs(d.AvailableFreeSpace - lastAvailableDiskSpace);
            if (d.IsReady)
            {
                if (freeVolumeDifference > triggerSpaceBytes)
                    _logger.LogInformation("{time}, обєм вільного місця змінився з {lastAvailableDiskSpace} МБ до {currentAvailableDiskSpace} МБ", DateTimeOffset.Now, ByteConverter.BytesToMegabytes(lastAvailableDiskSpace), ByteConverter.BytesToMegabytes(d.AvailableFreeSpace));

                return d.AvailableFreeSpace;
            }
            return lastAvailableDiskSpace;
        }

    }
}
