using System;
using System.Collections.Generic;

namespace DiChoSaiGon.Models;

public partial class Manage
{
    public int ManageId { get; set; }

    public string? ManageName { get; set; }

    public virtual ICollection<AccessPermission> AccessPermissions { get; set; } = new List<AccessPermission>();
}
