using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Utils.Standard.Helpers
{
    public static class EnumHelper
    {
        public static ObservableCollection<TEnum> GetEnumList<TEnum>() where TEnum : Enum
        {
            var cols = new ObservableCollection<TEnum>();
            var items = Enum.GetValues(typeof(TEnum));
            foreach (var item in items)
            {
                cols.Add((TEnum)item);
            }
            return cols;
        }
    }
}
