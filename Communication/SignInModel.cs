using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    [DataContract]
    public class SignInModel
    {
        public SignInModel()
        { }

        public SignInModel(string email, string password)
        {
            Email = email;
            Password = password;
        }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }
    }
}
