using PointBlank.Battle.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointBlank.Battle.Data.Models.Event
{
    public class ActionStateInfo
    {
        public ACTION_STATE Action;
        public byte Value;
        public WEAPON_SYNC_TYPE Flag;
    }
}
