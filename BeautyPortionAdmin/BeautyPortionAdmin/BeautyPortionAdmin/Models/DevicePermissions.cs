using System;
using System.Collections.Generic;
using System.Text;

namespace BeautyPortionAdmin.Models
{
    public class DevicePermissions
    {
        public Dictionary<Type, bool> PermissionsRequested { get; } = new Dictionary<Type, bool>();
    }
}
