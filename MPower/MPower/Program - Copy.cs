using MPower.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPower
{
    class Program
    {
        private static void Main(string[] args)
        {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            try
            {
                POSSettings pOSSettings = new POSSettings();
                pOSSettings.IntializeStoreSettings();
                foreach (POSSetting current in pOSSettings.PosDetails)
                {
                    if (current.PosName.ToUpper() == "MPOWER")
                    {
                        if (current.StoreSettings.StoreId == 10719)
                        {
                            clsmPower clsmPower = new clsmPower(10719, "https://mpowerapi.azurewebsites.net/api/v1/Items?", "7fe71055-c8e8-44c3-8ec7-233f072e1023", "kishor@techmaticsys.com", 0.07, 1, null, true);
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
