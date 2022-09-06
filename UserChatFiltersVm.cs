using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.MasterModel
{
    public class UserChatFiltersVm
    {
        public UserChatFiltersVm()
        {
            SelectedStatus=new List<string>();
            IVRPhoneIds=new List<string> ();
        }
        public int ChatFilterId { get; set; }

        public int OrganizationId { get; set; }

        public int UserId { get; set; }

        public List<string> SelectedStatus { get; set; }

        public List<string> IVRPhoneIds { get; set; }
    }
}
