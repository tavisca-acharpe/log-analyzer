using Amazon;

namespace Log.Analyzer.EmailAdapter
{
    public static class RegionEndpointFactory
    {
        public static RegionEndpoint GetRegion(this string type)
        {
            switch (type.ToLower())
            {
                case KeyStore.RegionEndpoints.USEast1:
                    return RegionEndpoint.USEast1;
                case KeyStore.RegionEndpoints.CNNorth1:
                    return RegionEndpoint.CNNorth1;
                case KeyStore.RegionEndpoints.SAEast1:
                    return RegionEndpoint.SAEast1;
                case KeyStore.RegionEndpoints.APSoutheast2:
                    return RegionEndpoint.APSoutheast2;
                case KeyStore.RegionEndpoints.APSoutheast1:
                    return RegionEndpoint.APSoutheast1;
                case KeyStore.RegionEndpoints.APNortheast2:
                    return RegionEndpoint.APNortheast2;
                case KeyStore.RegionEndpoints.USGovCloudWest1:
                    return RegionEndpoint.USGovCloudWest1;
                case KeyStore.RegionEndpoints.EUCentral1:
                    return RegionEndpoint.EUCentral1;
                case KeyStore.RegionEndpoints.EUWest1:
                    return RegionEndpoint.EUWest1;
                case KeyStore.RegionEndpoints.USWest2:
                    return RegionEndpoint.USWest2;
                case KeyStore.RegionEndpoints.USWest1:
                    return RegionEndpoint.USWest1;
                case KeyStore.RegionEndpoints.APNortheast1:
                    return RegionEndpoint.APNortheast1;
                default:
                    return RegionEndpoint.USEast1;
            }
        }
    }
}
