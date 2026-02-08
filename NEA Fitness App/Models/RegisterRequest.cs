using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAFitnessApp.Models
{
    public class RegisterRequest
    {
        public int UserID { get; set; } 
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
    }
}
