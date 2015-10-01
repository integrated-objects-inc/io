using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace io.Data
{
    public interface IUIControllerData
    {
        void Validate();
    }

    public class UIControllerData : IUIControllerData
    {
        protected List<UIData<dynamic>> _items;

        public bool IsValid { get; protected set; }

        public virtual void Validate()
        {
            
        }

        protected void SetIsValid()
        {
            IsValid = true;

            foreach (UIData<dynamic> item in _items)
            {
                if (!item.IsValid)
                    IsValid = false;
            }
        }

        [DataContract()]
        public class Result<T>
        {
            private bool _success = false;
            private T _value = default(T);

            private string _message = "";
            public Result(T value, bool success, string message)
            {
                _success = success;
                _message = message;
                _value = value;
            }

            static public Result<bool> GetDefaultWithSuccess()
            {
                return new Result<bool>(true, true, "");
            }

            public Result()
            {
                _success = true;
                _message = "";
            }

            [DataMember()]
            public bool Success
            {
                get { return _success; }
                set { _success = value; }
            }

            [DataMember()]
            public T Value
            {
                get { return _value; }
                set { _value = value; }
            }

            [DataMember()]
            public string Message
            {
                get { return _message; }
                set { _message = value; }
            }
        }
    }
}
