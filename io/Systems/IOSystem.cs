using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.Systems
{
    public class IOSystem
    {
        private List<ConnectionSetting> _connections;
        private Mode _currentMode;

        public enum AppType
        {
            DesktopApp = 1,
            WindowsService = 2,
            Website = 3,
            Library = 4
        }

        public enum LogType
        {
            File = 1,
            WindowsLog = 2
        }

        public enum Mode
        {
            None = 0,
            Development = 1,
            Testing = 2,
            Production = 3
        }
        
        public struct ConnectionSetting
        {
            public int Index;
            public string Name;
            public string ConnectionString;
            public bool Valid;
        }

        public IOSystem()
        {
            _connections = new List<ConnectionSetting>();
            LoadConnections();
        }

        public Mode CurrentMode
        { get { return _currentMode; } }

        public List<ConnectionSetting> Connections()
        {
            { return _connections; }
        }

        public ConnectionSetting Connection(int index)
        {
            foreach (ConnectionSetting value in _connections)
            {
                if (value.Index == index)
                    return value;
            }

            return new ConnectionSetting();
        }

        private void LoadConnections()
        {
            var mode = GetMode();

            if (mode.Key != Mode.None)
            { 
                using (var file = new System.IO.StreamReader(mode.Value))
                {
                    while (!file.EndOfStream)
                    {
                        var setting = ParseLine(file.ReadLine());
                        if (setting.Valid)
                            _connections.Add(setting);
                    }
                }
            }
        }
        
        private KeyValuePair<Mode, string> GetMode()
        {
            var _devConfig = @"\ioconfig-dev.txt";
            var _testConfig = @"\ioconfig-test.txt";
            var _prodConfig = @"\ioconfig-prod.txt";
            var path = BaseDirectory();

            var mode = new KeyValuePair<Mode, string>(Mode.None, "");

            var devInfo = new System.IO.FileInfo(path + _devConfig);
            var testInfo = new System.IO.FileInfo(path + _testConfig);
            var prodInfo = new System.IO.FileInfo(path + _prodConfig);

            if (devInfo.Exists)
                mode = new KeyValuePair<Mode, string>(Mode.Development, path + _devConfig);
            else if (testInfo.Exists)
                mode = new KeyValuePair<Mode, string>(Mode.Testing, path + _testConfig);
            else if (prodInfo.Exists)
                mode = new KeyValuePair<Mode, string>(Mode.Production, path + _prodConfig);

            _currentMode = mode.Key;

            return mode;
        }

        private string BaseDirectory()
        {
            var path = System.AppDomain.CurrentDomain.BaseDirectory;
            var devInfo = new System.IO.FileInfo(path + @"\iosystemlog.dll");
            if (devInfo.Exists)
                return path;

            path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin");
            devInfo = new System.IO.FileInfo(path + @"\iosystemlog.dll");
            if (devInfo.Exists)
                return path;

            return "";
        }

        private ConnectionSetting ParseLine(string line)
        {
            var setting = new ConnectionSetting();

            setting.Index = -1;
            setting.Name = String.Empty;
            setting.ConnectionString = String.Empty;
            setting.Valid = false;

            try
            {
                var data = line.Split('~');
                setting.Index = Convert.ToInt32(data[0].ToString());
                setting.Name = data[1].ToString();
                setting.ConnectionString = data[2].ToString();
                setting.Valid = true;
            }
            catch
            {
                setting.Valid = false;
            }

            return setting;
        }
    }
}
