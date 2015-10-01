using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.Database
{
    public class Commands
    {
        public class Command
        {
            private string _commandText = string.Empty;
            private List<KeyValuePair<string, object>> _parametersList = new List<KeyValuePair<string, object>>();

            public Command(string commandText, List<KeyValuePair<string, object>> parametersList)
            {
                _commandText = commandText;

                if (parametersList != null)
                    _parametersList = parametersList;
            }

            public Command(string commandText)
            {
                _commandText = commandText;
            }

            public string CommandText
            {
                get { return _commandText; }
            }

            public List<KeyValuePair<string, object>> ParametersList
            {
                get { return _parametersList; }
            }
        }
        
        private string _connectionString = string.Empty;
        private List<Command> _commands = new List<Command>();

        private bool _isStoredProcedure = false;
        private bool _disposedValue = false; 

        private AppDomain.App _app;
        private Systems.IOSystem _ioSystem;

        private bool _noTimeOut = false;

        public Commands(int connectionIndex, Systems.IOSystem ioSystem, bool noTimeout)
        {
            _connectionString = ioSystem.Connection(connectionIndex).ConnectionString;
            _ioSystem = ioSystem;
            _noTimeOut = noTimeout;
        }

        public Commands(List<Command> commands)
        { 
        
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {

                if (disposing)
                { }

                _connectionString = "";
            }
            _disposedValue = true;
        }

        #region " IDisposable Support "
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
