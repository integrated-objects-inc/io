using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.AppDomain
{
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

    public struct App
    {
        public string AppName;
        public AppType AppType;
        public string AppPath;
        public LogType LogType;
        public User CurrentUser;

        public List<string> Connections;

        public static App NewInstance(string appname, AppType apptype, string apppath, User user, LogType logtype, List<string> connections)
        {
            App app = new App();

            app.AppName = appname;
            app.AppType = apptype;
            app.AppPath = apppath;
            app.LogType = logtype;
            app.CurrentUser = user;
            app.Connections = connections;

            return app;
        }
    }
}