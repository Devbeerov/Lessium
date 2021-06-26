using System.Collections.Generic;

namespace Lessium.Services
{
    public static class SectionTestsInfoManager
    {
        private static List<SectionTestsInfoService> services = new List<SectionTestsInfoService>();

        public static void AddService(SectionTestsInfoService service)
        {
            services.Add(service);
        }
    }
}
