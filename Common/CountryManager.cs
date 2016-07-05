using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class CountryManager
    {

        public static IDictionary<string, string> CisCountry()
        {
            return new Dictionary<string, string>
            {

                {"RUS", "RUS"},
                {"ARM", "ARM"},
                {"BLR", "BLR"},
                {"KAZ", "KAZ"},
                {"KGZ", "KGZ"},
                {"MDA", "MDA"},
                {"TJK", "TJK"},
                {"TKM", "TKM"},
                {"UZB", "UZB"},
                {"UKR", "UKR"},
                {"LTU", "LTU"},
                {"LVA", "LVA"},
                {"EST", "EST"},
                {"GEO", "GEO"},

            };
            
        }

        public static IDictionary<string, string> ChineseCountry()
        {
            return new Dictionary<string, string>
            {

                {"CHN", "CHN"},


            };
        } 

        public static readonly Dictionary<string, string> CountryIso3ToIso2Links = new Dictionary<string, string>
        {

            #region ISO Codes3To2
            {"ABW", "AB"},
            {"AFG", "AF"},
            {"AGO", "AO"},
            {"AIA", "AI"},
            {"ALA", "AX"},
            {"ALB", "AL"},
            {"AND", "AD"},
            {"ARE", "AE"},
            {"ARG", "AR"},
            {"ARM", "AM"},
            {"ASM", "AS"},
            {"ATA", "AQ"},
            {"ATF", "TF"},
            {"ATG", "AG"},
            {"AUS", "AU"},
            {"AUT", "AT"},
            {"AZE", "AZ"},
            {"BDI", "BI"},
            {"BEL", "BE"},
            {"BEN", "BJ"},
            {"BES", "BQ"},
            {"BFA", "BF"},
            {"BGD", "BD"},
            {"BGR", "BG"},
            {"BHR", "BH"},
            {"BHS", "BS"},
            {"BIH", "BA"},
            {"BLM", "BL"},
            {"BLR", "BY"},
            {"BLZ", "BZ"},
            {"BMU", "BM"},
            {"BOL", "BO"},
            {"BRA", "BR"},
            {"BRB", "BB"},
            {"BRN", "BN"},
            {"BTN", "BT"},
            {"BVT", "BV"},
            {"BWA", "BW"},
            {"CAF", "CF"},
            {"CAN", "CA"},
            {"CCK", "CC"},
            {"CHE", "CH"},
            {"CHL", "CL"},
            {"CHN", "CN"},
            {"CIV", "CI"},
            {"CMR", "CM"},
            {"COD", "CD"},
            {"COG", "CG"},
            {"COK", "CK"},
            {"COL", "CO"},
            {"COM", "KM"},
            {"CPV", "CV"},
            {"CRI", "CR"},
            {"CUB", "CU"},
            {"CUW", "CW"},
            {"CXR", "CX"},
            {"CYM", "KY"},
            {"CYP", "CY"},
            {"CZE", "CZ"},
            {"DEU", "DE"},
            {"DJI", "DJ"},
            {"DMA", "DM"},
            {"DNK", "DK"},
            {"DOM", "DO"},
            {"DZA", "DZ"},
            {"ECU", "EC"},
            {"EGY", "EG"},
            {"ERI", "ER"},
            {"ESH", "EH"},
            {"ESP", "ES"},
            {"EST", "EE"},
            {"ETH", "ET"},
            {"FIN", "FI"},
            {"FJI", "FJ"},
            {"FLK", "FK"},
            {"FRA", "FR"},
            {"FRO", "FO"},
            {"FSM", "FM"},
            {"GAB", "GA"},
            {"GBR", "GB"},
            {"GEO", "GE"},
            {"GGY", "GG"},
            {"GHA", "GH"},
            {"GIB", "GI"},
            {"GIN", "GN"},
            {"GLP", "GP"},
            {"GMB", "GM"},
            {"GNB", "GW"},
            {"GNQ", "GQ"},
            {"GRC", "GR"},
            {"GRD", "GD"},
            {"GRL", "GL"},
            {"GTM", "GT"},
            {"GUF", "GF"},
            {"GUM", "GU"},
            {"GUY", "GY"},
            {"HKG", "HK"},
            {"HMD", "HM"},
            {"HND", "HN"},
            {"HRV", "HR"},
            {"HTI", "HT"},
            {"HUN", "HU"},
            {"IDN", "ID"},
            {"IMN", "IM"},
            {"IND", "IN"},
            {"IOT", "IO"},
            {"IRL", "IE"},
            {"IRN", "IR"},
            {"IRQ", "IQ"},
            {"ISL", "IS"},
            {"ISR", "IL"},
            {"ITA", "IT"},
            {"JAM", "JM"},
            {"JEY", "JE"},
            {"JOR", "JO"},
            {"JPN", "JP"},
            {"KAZ", "KZ"},
            {"KEN", "KE"},
            {"KGZ", "KG"},
            {"KHM", "KH"},
            {"KIR", "KI"},
            {"KNA", "KN"},
            {"KOR", "KR"},
            {"KWT", "KW"},
            {"LAO", "LA"},
            {"LBN", "LB"},
            {"LBR", "LR"},
            {"LBY", "LY"},
            {"LCA", "LC"},
            {"LIE", "LI"},
            {"LKA", "LK"},
            {"LSO", "LS"},
            {"LTU", "LT"},
            {"LUX", "LU"},
            {"LVA", "LV"},
            {"MAC", "MO"},
            {"MAF", "MF"},
            {"MAR", "MA"},
            {"MCO", "MC"},
            {"MDA", "MD"},
            {"MDG", "MG"},
            {"MDV", "MV"},
            {"MEX", "MX"},
            {"MHL", "MH"},
            {"MKD", "MK"},
            {"MLI", "ML"},
            {"MLT", "MT"},
            {"MMR", "MM"},
            {"MNE", "ME"},
            {"MNG", "MN"},
            {"MNP", "MP"},
            {"MOZ", "MZ"},
            {"MRT", "MR"},
            {"MSR", "MS"},
            {"MTQ", "MQ"},
            {"MUS", "MU"},
            {"MWI", "MW"},
            {"MYS", "MY"},
            {"MYT", "YT"},
            {"NAM", "NA"},
            {"NCL", "NC"},
            {"NER", "NE"},
            {"NFK", "NF"},
            {"NGA", "NG"},
            {"NIC", "NI"},
            {"NIU", "NU"},
            {"NLD", "NL"},
            {"NOR", "NO"},
            {"NPL", "NP"},
            {"NRU", "NR"},
            {"NZL", "NZ"},
            {"OMN", "OM"},
            {"PAK", "PK"},
            {"PAN", "PA"},
            {"PCN", "PN"},
            {"PER", "PE"},
            {"PHL", "PH"},
            {"PLW", "PW"},
            {"PNG", "PG"},
            {"POL", "PL"},
            {"PRI", "PR"},
            {"PRK", "KP"},
            {"PRT", "PT"},
            {"PRY", "PY"},
            {"PSE", "PS"},
            {"PYF", "PF"},
            {"QAT", "QA"},
            {"REU", "RE"},
            {"ROU", "RO"},
            {"RUS", "RU"},
            {"RWA", "RW"},
            {"SAU", "SA"},
            {"SDN", "SD"},
            {"SEN", "SN"},
            {"SGP", "SG"},
            {"SGS", "GS"},
            {"SHN", "SH"},
            {"SJM", "SJ"},
            {"SLB", "SB"},
            {"SLE", "SL"},
            {"SLV", "SV"},
            {"SMR", "SM"},
            {"SOM", "SO"},
            {"SPM", "PM"},
            {"SRB", "RS"},
            {"SSD", "SS"},
            {"STP", "ST"},
            {"SUR", "SR"},
            {"SVK", "SK"},
            {"SVN", "SI"},
            {"SWE", "SE"},
            {"SWZ", "SZ"},
            {"SXM", "SX"},
            {"SYC", "SC"},
            {"SYR", "SY"},
            {"TCA", "TC"},
            {"TCD", "TD"},
            {"TGO", "TG"},
            {"THA", "TH"},
            {"TJK", "TJ"},
            {"TKL", "TK"},
            {"TKM", "TM"},
            {"TLS", "TL"},
            {"TON", "TO"},
            {"TTO", "TT"},
            {"TUN", "TN"},
            {"TUR", "TR"},
            {"TUV", "TV"},
            {"TWN", "TW"},
            {"TZA", "TZ"},
            {"UGA", "UG"},
            {"UKR", "UA"},
            {"UMI", "UM"},
            {"URY", "UY"},
            {"USA", "US"},
            {"UZB", "UZ"},
            {"VAT", "VA"},
            {"VCT", "VC"},
            {"VEN", "VE"},
            {"VGB", "VG"},
            {"VIR", "VI"},
            {"VNM", "VN"},
            {"VUT", "VU"},
            {"WLF", "WF"},
            {"WSM", "WS"},
            {"YEM", "YE"},
            {"ZAF", "ZA"},
            {"ZMB", "ZM"},
            {"ZWE", "ZW"},

            #endregion

        };


        private static readonly Lazy<Dictionary<string, string>> DictCountryIso2ToIso3Links
            = new Lazy<Dictionary<string, string>>(() => CountryIso3ToIso2Links.ToDictionary(kpv => kpv.Value, kpv => kpv.Key));

        public static Dictionary<string, string> CountryIso2ToIso3Links => DictCountryIso2ToIso3Links.Value;

        public static bool HasIso3(string iso3Id)
        {
            return CountryIso3ToIso2Links.ContainsKey(iso3Id);
        }

        public static bool HasIso2(string iso2Id)
        {
            return CountryIso2ToIso3Links.ContainsKey(iso2Id);
        }

        public static IEnumerable<string> AllAlpha3Codes()
        {
            return CountryIso3ToIso2Links.Keys;
        }

        public static IEnumerable<string> AllAlpha2Codes()
        {
            return CountryIso3ToIso2Links.Values;
        }

        public static string Iso3ToIso2(string iso3)
        {
            return CountryIso3ToIso2Links[iso3];
        }

        public static string Iso2ToIso3(string iso2)
        {
            return CountryIso2ToIso3Links.ContainsKey(iso2) ? CountryIso2ToIso3Links[iso2] : iso2;
        }

    }
}
