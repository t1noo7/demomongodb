using DiChoSaiGon.Models;
using System.Collections.Generic;

namespace DiChoSaiGon.Areas.Admin.ViewModel
{
    public class EditViewModel
    {
        public Role singleRole { get; set; }
        public List<RoleViewModel> RelatedRoles { get; set; }
    }

    public class RoleViewModel
    {
        public Role Role { get; set; }
        public List<Manage> relatedManage { get; set; }
        public List<AccessPermission> relatedAccessPermission { get; set; }
    }
}
