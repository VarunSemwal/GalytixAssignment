namespace GalytixAssignment.API.Models
{
    public sealed class CountryGwpData
    {
        public CountryGwpData(string country, string lineOfBusiness, decimal?[] yearValues)
        {
            Country = country;
            LineOfBusiness = lineOfBusiness;
            YearValues = yearValues;
        }

        public string Country { get; }

        public string LineOfBusiness { get; }

        //Will only contain year array from 2008 to 2015
        public decimal?[] YearValues { get; }
    }
}
