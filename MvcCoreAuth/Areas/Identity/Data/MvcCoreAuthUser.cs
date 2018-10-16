using Microsoft.AspNetCore.Identity;
using System;

namespace MvcCoreAuth.Areas.Identity.Data
{
    public class MvcCoreAuthUser : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; }
        [PersonalData]
        public DateTime DOB { get; set; }
    }
}
