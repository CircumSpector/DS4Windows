using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Common.Core
{
    public class PropertyChangedBase<T> : INotifyPropertyChanged
        where T : PropertyChangedBase<T>
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }
    }
}
