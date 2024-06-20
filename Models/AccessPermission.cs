using System;
using System.Collections.Generic;

namespace DiChoSaiGon.Models;

public partial class AccessPermission
{
    public int AccessPermissionId { get; set; }

    public int ManageId { get; set; }

    public bool CanCreate { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool CanRead { get; set; }

    public int RoleId { get; set; }

    public bool CanAccessPermission { get; set; }

    public virtual Manage Manage { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
