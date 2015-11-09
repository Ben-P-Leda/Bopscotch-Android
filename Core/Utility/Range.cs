using System;
using System.Text;
using System.Globalization;

namespace Leda.Core
{
    public struct Range : IEquatable<Range>
    {
        private static Range emptyRange = new Range(0f, 0f);
        private static Range oneRange = new Range(1.0f, 1.0f);

        public float Minimum;
        public float Maximum;

        public float Interval { get { return Maximum - Minimum; } }

        public static Range Empty { get { return emptyRange; } }
        public static Range One { get { return oneRange; } }

        public Range(float minimum, float maximum)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        public Range(float initialValue)
        {
            this.Minimum = initialValue;
            this.Maximum = initialValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is Range) { return Equals((Range)obj); }
            return false;
        }

        public bool Equals(Range other)
        {
            return (Minimum == other.Minimum) && (Maximum == other.Maximum);
        }

        public void Expand(float newLimit)
        {
            this.Minimum = Math.Min(this.Minimum, newLimit);
            this.Maximum = Math.Max(this.Maximum, newLimit);
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return string.Format(currentCulture, "{{Minimum:{0} Maximum:{1}}}", new object[] { 
				this.Minimum.ToString(currentCulture), this.Maximum.ToString(currentCulture) });
        }

        public float RandomValue { get { return Random.Generator.Next(this.Minimum, this.Maximum); } }
        public float MiddleValue { get { return this.Minimum + ((this.Maximum - this.Minimum) / 2.0f); } }

        public static bool operator ==(Range value1, Range value2)
        {
            return value1.Minimum == value2.Minimum && value1.Maximum == value2.Maximum;
        }

        public static bool operator !=(Range value1, Range value2)
        {
            return value1.Minimum != value2.Minimum || value1.Maximum != value2.Maximum;
        }

        public bool Contains(float value)
        {
            return ((value >= this.Minimum) && (value <= this.Maximum));
        }

        public bool Overlaps(Range target)
        {
            return ((this.Contains(target.Minimum)) || (this.Contains(target.Maximum)) || 
                (target.Contains(this.Minimum)) || (target.Contains(this.Maximum)));
        }
    }
}
