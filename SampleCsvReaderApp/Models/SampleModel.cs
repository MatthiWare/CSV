using MatthiWare.Csv.Attributes;

namespace SampleCsvReaderApp.Models
{
    public class SampleModel
    {
        [CsvColumn("policyID")]
        public int ID { get; set; }

        //policyID,statecode,county,eq_site_limit,hu_site_limit,
        //fl_site_limit,fr_site_limit,tiv_2011,tiv_2012,eq_site_deductible,
        //hu_site_deductible,fl_site_deductible,fr_site_deductible,point_latitude,
        //point_longitude,line,construction,point_granularity

        [CsvColumn("county")]
        public string County { get; set; }

        [CsvColumn("eq_site_limit")]
        public double Limit { get; set; }

        [CsvColumn("point_granularity")]
        public int Granularity { get; set; }
    }
}
