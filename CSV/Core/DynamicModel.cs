using System.Collections.Generic;
using System.Dynamic;

namespace MatthiWare.Csv.Core
{
    internal class DynamicModel : DynamicObject
    {
        private readonly IDictionary<string, object> m_properties = new Dictionary<string, object>();

        public void AddProperty(string name, object value)
            => m_properties.Add(name, value);

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            m_properties[binder.Name] = value;

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return m_properties.TryGetValue(binder.Name, out result);
        }

    }
}
