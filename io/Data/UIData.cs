using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace io.Data
{
    public interface IUIData<T>
    {
        T Value { get; set; }
        T SubmittedValue { get; set; }
        bool IsValid { get; set; }
        string DefaultValue { get; set; }
        string Message { get; set; }
    }

    public enum Types
    {
        String = 1,
        Date = 2,
        DateTime = 3,
        Integer = 4,
        Decimal = 5,
        Boolean = 6,
        Double = 7
    }

    [DataContract()]
    public class UIData<T> : IUIData<T>
    {
        private T _submittedValue;
        private T _value;
        private string _defaultValue;
        private string _message;
        private bool _isValid;
        private Types _trueType = Types.String;

        private bool _modified = false;
        public UIData(Types trueType)
        {
            _message = "";
            _isValid = true;
            _trueType = trueType;
        }

        public UIData(T value, Types trueType)
        {
            _value = value;
            _modified = true;
            _submittedValue = value;
            _message = "";
            _isValid = true;
            _trueType = trueType;
        }

        public Types TrueType
        {
            get { return _trueType; }
            set { _trueType = value; }
        }

        public void CopyValuesTo(IUIData<string> item)
        {
            item.Value = _value.ToString();
            item.DefaultValue = _defaultValue;
            item.IsValid = _isValid;
            item.Message = _message;
            _modified = true;
        }

        public bool Modified
        {
            get { return _modified; }
        }

        [DataMember()]
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        [DataMember()]
        public T SubmittedValue
        {
            get { return _submittedValue; }
            set { _submittedValue = value; }
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
            get
            {
                if (_message == null)
                {
                    _message = "";
                }
                return _message;
            }
            set { _message = value; }
        }

        [DataMember()]
        public bool IsValid
        {
            get { return _isValid; }
            set { _isValid = value; }
        }
    }
}
