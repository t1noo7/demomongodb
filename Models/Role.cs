using System;
using System.Collections.Generic;

namespace DiChoSaiGon.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<AccessPermission> AccessPermissions { get; set; } = new List<AccessPermission>();

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
