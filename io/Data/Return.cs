using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.Data
{
    public class Return<T>
    {
        private ResultEnum _result = ResultEnum.Failure;
        private string _message = string.Empty;
        private string _description = string.Empty;
        private T _value = default(T);

        private string _function = string.Empty;

        public enum ResultEnum
        {
            Success = 1,
            Failure = 2,
            Warning = 3,
            Fatal = 4,
            True = 5,
            False = 6
        }

        public void ChangeT(object newT)
        {
            _value = (T)newT;
        }

        public Return(bool result, string message, string description, T value = default(T))
        {
            if (result) { _result = Return<T>.ResultEnum.Success; } 

            _message = message;
            _description = description;
            _value = value;
        }

        public Return(ResultEnum result, string message, string description = "", T value = default(T))
        {
            _result = result;
            _message = message;
            _description = description;
            _value = value;
        }

        public Return(ResultEnum result, string message, io.AppDomain.App App, string description = "", T value = default(T))
        {
            _result = result;
            _message = message;
            _description = description;
            _value = value;
        }

        public Return(ResultEnum result, string message, Systems.IOSystem ioSystem, string description = "", T value = default(T))
        {
            _result = result;
            _message = message;
            _description = description;
            _value = value;
        }

        public Return<T> LogResult(int systemInstallKey, int systemKey, int appKey, int userSessionKey, int errorCodeKey, string functionName, string exceptionMessage, string sql, string paramsIn, string paramsOut)
        {
            iosystemlog.Modules.Logging.Log.Entry(systemInstallKey, systemKey, appKey, userSessionKey, errorCodeKey, this.Success, _description, functionName, _message, exceptionMessage, sql, paramsIn, paramsOut);
            return this;
        }

        public Return<T> LogResult(int systemInstallKey, int systemKey, int appKey, int userSessionKey, int errorCodeKey, string functionName, string exceptionMessage, string sql, string paramsIn)
        {
            iosystemlog.Modules.Logging.Log.Entry(systemInstallKey, systemKey, appKey, userSessionKey, errorCodeKey, this.Success, _description, functionName, _message, exceptionMessage, sql, paramsIn, "");
            return this;
        }

        public Return<T> LogResult(int systemInstallKey, int systemKey, int appKey, int userSessionKey, int errorCodeKey, string functionName, string exceptionMessage, string sql)
        {
            iosystemlog.Modules.Logging.Log.Entry(systemInstallKey, systemKey, appKey, userSessionKey, errorCodeKey, this.Success, _description, functionName, _message, exceptionMessage, sql, "", "");
            return this;
        }

        public Return<T> LogResult(int systemInstallKey, int systemKey, int appKey, int userSessionKey, int errorCodeKey, string functionName, string exceptionMessage)
        {
            iosystemlog.Modules.Logging.Log.Entry(systemInstallKey, systemKey, appKey, userSessionKey, errorCodeKey, this.Success, _description, functionName, _message, exceptionMessage, "", "", "");
            return this;
        }

        public Return<T> LogResult(int systemInstallKey, int systemKey, int appKey, int userSessionKey, int errorCodeKey, string functionName)
        {
            iosystemlog.Modules.Logging.Log.Entry(systemInstallKey, systemKey, appKey, userSessionKey, errorCodeKey, this.Success, _description, functionName, _message, "", "", "", "");
            return this;
        }

        public bool Success
        {
            get
            {
                switch (_result)
                {
                    case Return<T>.ResultEnum.Success:
                    case Return<T>.ResultEnum.True:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public Return<T> SetMessage(string successMessage, string failureMessage)
        {
            if (this.Success)
                _message = successMessage;
            else
                _message = failureMessage;

            return this;
        }

        public bool Exist
        {
            get { return Success; }
        }

        public bool True
        {
            get { return Success; }
        }

        public bool False
        {
            get { return !Success; }
        }

        public bool Failed
        {
            get { return !Success; }
        }

        public ResultEnum Result
        {
            get { return _result; }
        }

        public string Message
        {
            get { return _message; }
        }

        public string Description
        {
            get { return _description; }
        }

        public T Value
        {
            get { return _value; }
        }

        public T Object
        {
            get { return _value; }
        }

        public string Function
        {
            get { return _function; }
        }

        public Return<T> Me
        {
            get { return this; }
        }

        public Return<T> This
        {
            get { return this; }
        }

        public Return<T> AddTrace(string func)
        {
            if (func.EndsWith("()"))
                _function = func + (_function.Length != 0 ? "->" : "") + _function;
            else if (func.EndsWith("."))
                _function = func + _function;
            else
                _function = func + (_function.Length != 0 ? "." : "") + _function;

            return this;
        }

        public io.Data.UIControllerData.Result<T> ToUIControllerResult()
        {
            return new io.Data.UIControllerData.Result<T>(this.Object, Success, _message);
        }
    }
}
