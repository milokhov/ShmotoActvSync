using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShmotoActvSync.ViewModels
{
    public class UserViewModel
    {
        public string UserName { get; set; }
        public bool AssociatedToMoto { get; set; }
        public string MotoUserName { get; set; }
    }
}
