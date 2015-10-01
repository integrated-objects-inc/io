using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.AppDomain
{
    public class User
    {
        private int _id;
        private string _firstname = "";
        private string _lastname = "";
        private string _email = "";

        private string _windowsId = "";
        public User(int id, string firstname, string lastname, string email)
        {
            _id = id;
            _firstname = firstname;
            _lastname = lastname;
            _email = email;
        }

        public User(int id, string firstname, string lastname, string email, string windowsId)
        {
            _id = id;
            _firstname = firstname;
            _lastname = lastname;
            _email = email;
            _windowsId = windowsId;
        }

        public int ID
        {
            get { return _id; }
        }

        public string FirstName
        {
            get { return _firstname; }
        }

        public string LastName
        {
            get { return _lastname; }
        }

        public string Email
        {
            get { return _email; }
        }

        public string WindowsId
        {
            get { return _windowsId; }
            set { _windowsId = value; }
        }
    }
}