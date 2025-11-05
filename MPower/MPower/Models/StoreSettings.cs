    using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPower.Models
{
    public class POSSettings
    {
        public List<POSSetting> PosDetails { get; set; }
        public void IntializeStoreSettings()
        {
            DataSet dsResult = new DataSet();
            List<POSSetting> posdetails = new List<POSSetting>();
            try
            {
                string constr = ConfigurationManager.AppSettings.Get("LiquorAppsConnectionString");
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "usp_ts_GetStorePosSetting";
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            da.Fill(dsResult);
                        }
                    }
                }
                if (dsResult != null || dsResult.Tables.Count > 0)
                {
                    foreach (DataRow dr in dsResult.Tables[0].Rows)
                    {
                        if (dr["PosName"].ToString().ToUpper() == "MPOWER")
                        {
                            POSSetting pobj = new POSSetting();
                            pobj.Setting = dr["Settings"].ToString();
                            StoreSetting obj = new StoreSetting();
                            obj.StoreId = Convert.ToInt16(dr["StoreId"] == DBNull.Value ? 0 : dr["StoreId"]);
                            obj.POSSettings = JsonConvert.DeserializeObject<Setting>(pobj.Setting);
                            pobj.PosName = dr["PosName"].ToString();
                            pobj.PosId = Convert.ToInt32(dr["PosId"]);
                            pobj.StoreSettings = obj;
                            posdetails.Add(pobj);
                        }
                        else { continue; }
                    }
                }
                PosDetails = posdetails;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
    public class POSSetting
    {
        public int PosId { get; set; }
        public string PosName { get; set; }
        public StoreSetting StoreSettings { get; set; }
        public string Setting { get; set; }
    }
    public class Setting
    {
        public string ClientId { get; set; }
        public string merchantId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
        public string tokenid { get; set; }
        public string instock { get; set; }
        public string category { get; set; }
        public string BaseUrl { get; set; }
        public decimal tax { get; set; }
        public decimal LiquorWineTax { get; set; }
        public decimal BeerTax { get; set; }
        public decimal MiscNonAlcoholTax { get; set; }
        public decimal GroceryTax { get; set; }
        public string PosFileName { get; set; }
        public string APIKey { get; set; }
        public int StoreMapId { get; set; }
        public decimal liquortax { get; set; }
        public decimal liquortaxrateperlitre { get; set; }
        public List<categories> categoriess { set; get; }
        public int LocationId { get; set; }
        public bool IsSalePrice { get; set; }
        public int Thread { get; set; }
    }
    public class categories
    {
        public string id { get; set; }
        public string name { get; set; }
        public decimal taxrate { get; set; }
        public Boolean selected { get; set; }
    }
    public class StoreSetting
    {
        public int StoreId { get; set; }
        public Setting POSSettings { get; set; }
    }
    public class storecat
    {
        public string catid { get; set; }
        public string catname { get; set; }
    }
}
