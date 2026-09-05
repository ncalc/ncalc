using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NCalc.Playground.Extensions;

public static class EnumExtensions
{
    extension(Enum value)
    {
        public string GetDisplayName()
        {
            var member = value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault();

            return member?
                       .GetCustomAttribute<DisplayAttribute>()?
                       .GetName()
                   ?? value.ToString();
        }
    }
}