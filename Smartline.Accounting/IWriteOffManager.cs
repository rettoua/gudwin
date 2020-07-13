using System.Collections.Generic;
using Smartline.Mapping;

namespace Smartline.Accounting {
    internal interface IWriteOffManager {
        bool WriteOffRequired(Tracker tracker, Account account);
        List<WriteOff> MakeWriteOffs(Tracker tracker, Account account, GlobalAccountingSettings accountingSettings);        
    }
}