using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace TaskProgressAndCancel
{
    public class WorkTypeWrapper
    {
        public WorkTypeWrapper(PropertyInfo property, object instance)
        {
            Command = (ICommand)property.GetValue(instance);
            Title = property.Name.TrimEnd("Command".ToArray());
        }
        public string Title { get; set; }
        public ICommand Command { get; set; }
    }
}
