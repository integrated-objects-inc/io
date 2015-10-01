using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using io.Data;

namespace io.Database
{
    public class Command : IDisposable
    {
        private string _commandText = string.Empty;
        private string _connectionString = string.Empty;
        private List<KeyValuePair<string, object>> _parametersList = new List<KeyValuePair<string,object>>();

        private bool _isStoredProcedure = false;
        private bool _disposedValue = false; 

        private AppDomain.App _app;
        private Systems.IOSystem _ioSystem;
        private System.Data.SqlClient.SqlParameterCollection _parameters;

        private bool _noTimeOut = false;

        public Command(string connectionString, string commandText, AppDomain.App app, bool noTimeout)
        {
            _connectionString = connectionString;
            _commandText = commandText;
            _app = app;
            _noTimeOut = noTimeout;
        }

        public Command(int connectionIndex, string commandText, AppDomain.App app, bool noTimeout)
        {
            _connectionString = app.Connections[connectionIndex].ToString();
            _commandText = commandText;
            _app = app;
            _noTimeOut = noTimeout;
        }

        public Command(string server, string database, string userId, string password, string commandText, bool noTimeout)
        {
            _connectionString = "Server=" + server + ";Database=" + database + ";UID=" + userId + ";Password=" + password + ";";
            _commandText = commandText;
            _noTimeOut = noTimeout;
        }

        public Command(string connectionString, string commandText, Systems.IOSystem ioSystem, bool noTimeout)
        {
            _connectionString = connectionString;
            _commandText = commandText;
            _ioSystem = ioSystem;
            _noTimeOut = noTimeout;
        }

        public Command(int connectionIndex, string commandText, Systems.IOSystem ioSystem, bool noTimeout)
        {
            try
            {
                _connectionString = ioSystem.Connection(connectionIndex).ConnectionString;
                _commandText = commandText;
                _ioSystem = ioSystem;
                _noTimeOut = noTimeout;
            }
            catch
            { 
            
            }
        }

        public Command(int connectionIndex, string commandText, List<KeyValuePair<string, object>> parametersList, Systems.IOSystem ioSystem, bool noTimeout)
        {
            _connectionString = ioSystem.Connection(connectionIndex).ConnectionString;
            _commandText = commandText;
            _ioSystem = ioSystem;
            _isStoredProcedure = true;
            _noTimeOut = noTimeout;

            if (parametersList != null)
                _parametersList = parametersList;
        }

        public Command(int connectionIndex, string commandText, List<KeyValuePair<string, object>> parametersList, AppDomain.App app, bool noTimeout)
        {
            _connectionString = app.Connections[connectionIndex].ToString();
            _commandText = commandText;
            _app = app;
            _isStoredProcedure = true;
            _noTimeOut = noTimeout;

            if (parametersList != null)
                _parametersList = parametersList;
        }

        public string CommandText
        {
            get { return _commandText; }
            set { _commandText = value; }
        }

        public System.Data.SqlClient.SqlParameterCollection Parameters
        {
            get { return _parameters; }
        }

        public Return<bool> ExecuteNonQuery()
        {
            using (System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(_commandText, cn);
                int rowsAffected = 0;
                try
                {
                    cn.Open();

                    if (_isStoredProcedure)
                        cmd.CommandType = CommandType.StoredProcedure;

                    if (_parametersList.Count != 0)
                    {
                        foreach (KeyValuePair<string, object> parameter in _parametersList)
                        {
                            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                    }

                    _parameters = cmd.Parameters;

                    if (_noTimeOut)
                        cmd.CommandTimeout = 0;

                    rowsAffected = cmd.ExecuteNonQuery();

                    cn.Close();

                    if (_ioSystem == null)
                        return new Return<bool>(Return<bool>.ResultEnum.Success, rowsAffected.ToString(), _app, "", true);
                    else
                        return new Return<bool>(Return<bool>.ResultEnum.Success, rowsAffected.ToString(), _ioSystem, "", true);
                }
                catch (Exception ex)
                {
                    if (_ioSystem == null)
                        return new Return<bool>(Return<bool>.ResultEnum.Failure, ex.Message, _app, "", false);
                    else
                        return new Return<bool>(Return<bool>.ResultEnum.Failure, ex.Message, _ioSystem, "", false);
                }
            }
        }

        public Return<object> ExecuteScalar()
        {
            if (_commandText.Contains(Constants.SEPARATOR)) { return ExecuteMultipleScalar(); }

            object cmdResult = null;

            using (System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(_commandText, cn);

                try
                {
                    cn.Open();

                    if (_isStoredProcedure)
                        cmd.CommandType = CommandType.StoredProcedure;

                    if (_parametersList.Count != 0)
                    {
                        foreach (KeyValuePair<string, object> parameter in _parametersList)
                        {
                            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                    }

                    _parameters = cmd.Parameters;

                    if (_noTimeOut)
                        cmd.CommandTimeout = 0;

                    cmdResult = cmd.ExecuteScalar();

                    cn.Close();

                    if (_ioSystem == null)
                        return new Return<object>(Return<object>.ResultEnum.Success, "", _app, "", cmdResult);
                    else
                        return new Return<object>(Return<object>.ResultEnum.Success, "", _ioSystem, "", cmdResult);
                }
                catch (Exception ex)
                {
                    if (_ioSystem == null)
                        return new Return<object>(Return<object>.ResultEnum.Failure, ex.Message, _app, "", cmdResult);
                    else
                        return new Return<object>(Return<object>.ResultEnum.Failure, ex.Message, _ioSystem, "", cmdResult);
                }
            }
        }

        private Return<object> ExecuteMultipleScalar()
        {
            object cmdResult = null;
            string sql = "";

            using (System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                try
                {
                    cn.Open();
                    
                    for (int i = 0; i <= _commandText.Split(Constants.SEPARATOR).Length - 1; i++)
                    {
                        sql = _commandText.Split(Constants.SEPARATOR)[i];

                        System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql, cn);

                        if (_isStoredProcedure)
                            cmd.CommandType = CommandType.StoredProcedure;

                        if (_noTimeOut)
                            cmd.CommandTimeout = 0;

                        cmdResult = cmd.ExecuteScalar();
                    }
                    
                    cn.Close();
                    
                    if (_ioSystem == null)
                        return new Return<object>(Return<object>.ResultEnum.Success, "", _app, "", cmdResult);
                    else
                        return new Return<object>(Return<object>.ResultEnum.Success, "", _ioSystem, "", cmdResult);
                }
                catch (Exception ex)
                {
                    
                    if (_ioSystem == null)
                        return new Return<object>(Return<object>.ResultEnum.Failure, ex.Message, _app, "", cmdResult);
                    else
                        return new Return<object>(Return<object>.ResultEnum.Failure, ex.Message, _ioSystem, "", cmdResult);
                }
            }
        }

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {

                if (disposing)
                { }

                _commandText = "";
                _connectionString = "";
                _parametersList = null;
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