using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public static class ItemsBuilder
    {
        public static System.Collections.IList SelectItem(System.Collections.IList value, System.Collections.IList outList, Type type, object selected)
        {
            System.Collections.IList result = null;
            if (value?.Count > 0)
            {
                var thisType = value[0].GetType();
                if ((thisType.BaseType == type) || (thisType == type))
                {
                    outList.Clear();
                    result = outList;
                    foreach (var item in value)
                    {
                        result.Add(item);
                    }
                }
            }
            else
            {
                if(selected == null && outList?.Count > 0)
                {
                    outList.Clear();
                    result = outList;
                }
            }
            return result;
        }        
    }
}
