namespace Glass.Imaging.ZoneConfigurations.Alphanumeric
{
    using System.Linq;

    public class AlphanumericEvaluator : Evaluator, IEvaluator
    {
        public AlphanumericEvaluator(StringFilter stringFilter) : base(stringFilter)
        {
        }

        protected override int GetValidChars(string s)
        {
            return s.Count(char.IsLetterOrDigit);
        }
    }
}