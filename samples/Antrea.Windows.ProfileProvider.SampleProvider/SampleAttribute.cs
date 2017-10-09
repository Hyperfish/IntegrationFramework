using Antrea.Windows.ProfileProvider;

namespace SampleConnector
{
    public class SampleAttribute : ProfileAttribute
    {
        public SampleAttribute(string name) 
            : base(name)
        {
        }

        public SampleAttribute()
            : base(string.Empty)
        {
        }

        public override IAttribute DeepClone()
        {
            var a = base.Clone<SampleAttribute>();
            return a;
        }
    }
}
