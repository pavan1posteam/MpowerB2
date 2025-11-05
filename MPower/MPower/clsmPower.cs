
using MPower.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace MPower
{
    class clsmPower
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string frequentstores = ConfigurationManager.AppSettings["frequent_store"];
        string differentmapping = ConfigurationManager.AppSettings["different_mapping"];
        string irrespectiveofqty = ConfigurationManager.AppSettings["irrespective_of_qty"];
        string missionstores = ConfigurationManager.AppSettings["mission_stores"];
        string pricing = ConfigurationManager.AppSettings["pricing_format"];
        string liquorworldstores = ConfigurationManager.AppSettings["liquor_world_stores"];
        string cattax = ConfigurationManager.AppSettings["cat_tax"];
        string s_price = ConfigurationManager.AppSettings["s_price"];
        string s_price0 = ConfigurationManager.AppSettings["s_price0"];
        string cat_price = ConfigurationManager.AppSettings["cat_price"];
        string wedsaleprice = ConfigurationManager.AppSettings["wed_sprice"];
        string trsdysprice = ConfigurationManager.AppSettings["thur_sprice"];
        string uomassize = ConfigurationManager.AppSettings["uom_size"];
        string specialprice = ConfigurationManager.AppSettings["special_price"];
        string catdeposit = ConfigurationManager.AppSettings["cat_deposit"];
        string catqty = ConfigurationManager.AppSettings["cat_qty"];
        string staticqty = ConfigurationManager.AppSettings["static_qty"];
        public int curentpage = 1;
        public int totalpages = 1;
        public int pageSize = 1000;
        string pathProduct = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"DepositFile.json");
        string depositPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Deposit.json");
        public clsmPower(int StoreId, string BaseUrl, string accessToken,  decimal tax, int LocationId, string Code, bool IsSalePrice,decimal LiquorWineTax,decimal BeerTax,decimal MiscNonAlcoholTax,decimal GroceryTax)
        {
            string val = mPowerSetting(StoreId, BaseUrl, accessToken, tax, LocationId, Code, IsSalePrice, LiquorWineTax, BeerTax, MiscNonAlcoholTax, GroceryTax);
        }
        public string mPowerSetting(int StoreId, string BaseUrl, string ApiKey, decimal tax, int LocationId, string Code, bool IsSalePrice, decimal LiquorWineTax, decimal BeerTax, decimal MiscNonAlcoholTax, decimal GroceryTax)
        {
            string accessToken = ApiKey;
            List<Results> prd = new List<Results>();
            List<Results> prd1 = new List<Results>();
            List<Result> promotionList = new List<Result>();
            List<PromotionItem> promoItemList = new List<PromotionItem>();
            Console.WriteLine("Generating mPower " + StoreId + " Product File....");
            Console.WriteLine("Generating mPower " + StoreId + " Fullname File....");
            if (frequentstores.Contains(StoreId.ToString()))
            {
                Console.WriteLine("Generating mPower " + StoreId + " Frequent Shopper File....");
            }
            try
            {
                for (int i = 1; i <= totalpages; i++)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(Code))
                        {
                            prd.AddRange(getProductDetails(BaseUrl, accessToken, i, pageSize));
                        }
                        else
                        {
                            var response = getProductDetails(BaseUrl, accessToken, i, LocationId);
                            if (response != null)
                            {
                                prd.AddRange(response);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        new clsEmail().sendEmail(DeveloperId, "", "", "Error in MPOWER_POS@" + DateTime.UtcNow + " GMT", ex.Message + StoreId + "<br/>" + ex.StackTrace);
                        Console.WriteLine(ex.Message);
                    }
                    finally { }
                }
                if (IsSalePrice)
                {
                    try
                    {
                        totalpages = 1;
                        for (int i = 1; i <= totalpages; i++)
                        {
                            var promoList = getPromotions(accessToken, i, LocationId);
                            if (promoList != null || promoList.FirstOrDefault().EndDate > DateTime.Now.Date || promoList.FirstOrDefault().Description != "Club Pricing" || promoList.Count() > 0)
                            {
                                promotionList.AddRange(promoList);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //new clsEmail().sendEmail(DeveloperId, "", "", "Error in MPOWER_POS@" + DateTime.UtcNow + " GMT", ex.Message + StoreId + "<br/>" + ex.StackTrace);
                        //Console.WriteLine(ex.Message);
                        //Console.ReadLine();
                    }
                    finally { }
                }
                totalpages = 1;
                prd1 = new List<Results>();
                for (int i = 1; i <= totalpages; i++)
                {
                    if (frequentstores.Contains(StoreId.ToString()))
                    {
                        prd1.AddRange(getCustomerDetails(accessToken, i));
                    }
                }
                int storeid = StoreId;
                GenerateCsvFile(storeid, prd, prd1, promotionList, tax, LocationId, IsSalePrice,LiquorWineTax,BeerTax,MiscNonAlcoholTax,GroceryTax);
                Console.WriteLine("Product File Generated For mPower " + StoreId);
                Console.WriteLine("Fullname File Generated For mPower " + StoreId);
                if (frequentstores.Contains(StoreId.ToString()))
                {
                    Console.WriteLine("Frequent Shopper File Generated For mPower " + StoreId);
                }
                return "success";
            }
            catch (Exception e)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in MPOWER_POS@" + DateTime.UtcNow + " GMT", e.Message + StoreId + "<br/>" + e.StackTrace);
                Console.WriteLine(e.Message);
                return e.Message;
            }
            finally { }
        }
        private List<Results> getProductDetails(string BaseUrl, string accessToken, int pageno, int LocationId)
        {
            clsProduct fresult = new clsProduct();
            try
            {
                Thread.Sleep(1000);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                var client = new RestClient(BaseUrl + "PageNumber=" + pageno);
                var request = new RestRequest(Method.POST);

                request.AddHeader("Authorization", "Bearer " + accessToken);
                string json = null;
                if (LocationId > 0)
                {
                    //json = "{\"IncludeDeleted\":true," + "\"WebActive\":true," + "\"UpdatedSince\":\"2021-05-20T07:52:00\"," + "\"LocationId\":" + LocationId + "}";
                    json = "{\"WebActive\":true," + "\"LocationId\":" + LocationId + "}";
                }
                else
                {
                    //json = "{\"IncludeDeleted\":true," + "\"WebActive\":true," + "\"UpdatedSince\":\"2021-05-20T07:52:00\"}";
                    json = "{\"WebActive\":true }";
                }
                request.AddParameter("application/json", json, ParameterType.RequestBody);

                IRestResponse response = client.Execute(request);
                if (response == null)
                {

                }
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fresult = (clsProduct)JsonConvert.DeserializeObject(response.Content, typeof(clsProduct));
                    totalpages = fresult.TotalPages;
                }

            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in MPOWER_POS@" + DateTime.UtcNow + " GMT", ex.Message + accessToken + LocationId + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            return fresult.results;
        }
        private List<Results> getProductDetails(string BaseUrl, string accessToken, int pageno, int pageSize, int LocationId)
        {
            clsProduct fresult = new clsProduct();
            try
            {
                var client = new RestClient(BaseUrl + "PageNumber=" + pageno + "&pageSize=" + pageSize);
                var request = new RestRequest(Method.POST);

                request.AddHeader("Authorization", "Bearer " + accessToken);
                string json = "{\"IncludeDeleted\":true," + "\"WebActive\":true," + "\"UpdatedSince\":\"2021-05-20T07:52:00.000Z," + "\"LocationId\":\"" + LocationId + "}";
                request.AddParameter("application/json", json, ParameterType.RequestBody);

                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fresult = (clsProduct)JsonConvert.DeserializeObject(response.Content, typeof(clsProduct));
                    totalpages = fresult.TotalPages;
                }

            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in MPOWER_POS@" + DateTime.UtcNow + " GMT", ex.Message + accessToken + LocationId + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            return fresult.results;
        }
        private List<Result> getPromotions(string accessToken, int pageno, int LocationId)
        {
            Promotion promotion = new Promotion();
            try
            {
                Thread.Sleep(1000);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                var client = new RestClient("https://mpowerapi.azurewebsites.net/api/v1/Promotions/search?pageNumber=" + pageno);
                var request = new RestRequest(Method.POST);

                request.AddHeader("Authorization", "Bearer " + accessToken);
                string json = null;
                if (LocationId > 0)
                {
                    json = "{\"LocationId\":" + LocationId + "}";
                }
                else
                {
                    //json = "{\"UpdatedSince\":\"2021-05-20T07:52:00.000Z\"}";
                }

                request.AddParameter("application/json", json, ParameterType.RequestBody);

                IRestResponse response = client.Execute(request);
                File.AppendAllText("10394Promotionresponse.txt",response.Content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    promotion = (Promotion)JsonConvert.DeserializeObject(response.Content, typeof(Promotion));
                    totalpages = promotion.TotalPages;
                }
            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in MPOWER_POS@" + DateTime.UtcNow + " GMT", ex.Message + accessToken + LocationId + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            return promotion.Results;
        }
        private List<Results> getCustomerDetails(string accessToken, int pageno)
        {
          
            Thread.Sleep(1000);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            string BaseUrl = "https://mpowerapi.azurewebsites.net/api/v1/customers?";
            var client = new RestClient(BaseUrl + "PageNumber=" + pageno);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + accessToken);
            request.AddParameter("page", pageno);
            request.AddParameter("per_page", 1000);
            IRestResponse response = client.Execute(request);
            clsProduct fresult = (clsProduct)JsonConvert.DeserializeObject(response.Content, typeof(clsProduct));
            totalpages = fresult.TotalPages;
            return fresult.results;
        }
        public static void CreateCSVFromGenericList<T>(List<T> list, string csvNameWithExt)
        {
            if (list == null || list.Count == 0) return;

            //get type from 0th member
            Type t = list[0].GetType();
            string newLine = Environment.NewLine;

            using (var sw = new StreamWriter(csvNameWithExt))
            {
                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();

                //foreach of the properties in class above, write out properties
                //this is the header row
                foreach (PropertyInfo pi in props)
                {
                    sw.Write(pi.Name + ",");
                }
                sw.Write(newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    //this acts as datacolumn
                    foreach (PropertyInfo pi in props)
                    {
                        //this is the row+col intersection (the value)
                        string whatToWrite =
                            Convert.ToString(item.GetType()
                                                 .GetProperty(pi.Name)
                                                 .GetValue(item, null))
                                .Replace("\n", string.Empty)
                                .Replace("\r\n", string.Empty)
                                .Replace("\r", string.Empty)
                                .Replace(',', ' ') + ',';

                        sw.Write(whatToWrite);
                    }
                    sw.Write(newLine);
                }
            }
        }
        public void GenerateCsvFile(int StoreId, List<Results> results, List<Results> result1, List<Result> promo, decimal tax, int LocationId, bool IsSalePrice, decimal LiquorWineTax, decimal BeerTax, decimal MiscNonAlcoholTax, decimal GroceryTax)
        {
            try
            {
                List<ExportProduct> exProd = new List<ExportProduct>();
                List<clsFullnameModel> fullname = new List<clsFullnameModel>();
                List<ExportProductFor10380> exprod10380 = new List<ExportProductFor10380>();
                List<FrequentShopper> fshopper = new List<FrequentShopper>();
                List<Cus> customer = new List<Cus>();

                ExportProduct prod = new ExportProduct();
                clsFullnameModel fname = new clsFullnameModel();
                ExportProductFor10380 prod10380 = new ExportProductFor10380();
                FrequentShopper frequentshopper = new FrequentShopper();
                Cus onetimefile = new Cus();

                StreamReader r = new StreamReader(pathProduct);
                string json = r.ReadToEnd();
                List<RootObject> Depsitsitems = JsonConvert.DeserializeObject<List<RootObject>>(json);


                StreamReader sRead = new StreamReader(depositPath);
                string jsonDeposit = sRead.ReadToEnd();
                List<DepositRoot> Deposits = JsonConvert.DeserializeObject<List<DepositRoot>>(jsonDeposit);

                foreach (Results item in results)
                {
                    prod = new ExportProduct();
                    fname = new clsFullnameModel();
                    prod10380 = new ExportProductFor10380();
                    bool WebActive = item.WebActive;

                    if (differentmapping.Contains(StoreId.ToString()))
                    {
                        var Depositsize = item.Size.ToString().Trim();
                        prod10380.storeid = StoreId;
                        prod10380.StoreProductName = item.Name;
                        prod10380.Storedescription = Regex.Replace(item.Description, @"^""|""$|\\n?", "").Replace("'", string.Empty);
                        var pack = Depsitsitems.Where(a => a.Name == Depositsize).FirstOrDefault();
                        if (pack != null)
                        {
                            prod10380.pack = pack.PackColumnValue;
                        }
                        prod10380.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                        prod10380.price = item.Retail;
                        if (prod10380.price <= 0)
                        {
                            continue;
                        }
                        if (item.QuantityOnHand > 0)
                        {
                            prod10380.qty = Convert.ToInt32(item.QuantityOnHand);
                        }
                        else { continue; }
                        prod10380.upc = "#" + item.upc.ToString().Trim();
                        prod10380.sprice = 0;
                        prod10380.start = "";
                        prod10380.end = "";
                        prod10380.tax = tax;
                        prod10380.altupc1 = "";
                        prod10380.altupc2 = "";
                        prod10380.altupc3 = "";
                        prod10380.altupc4 = "";
                        prod10380.altupc5 = "";

                        decimal BottleDeposit = 0;
                        var SizeExists = Depsitsitems.Where(a => a.Name == Depositsize).FirstOrDefault();
                        if (SizeExists != null)
                        {
                            decimal.TryParse(SizeExists.BottleDeposit, out BottleDeposit);
                            prod10380.deposit = BottleDeposit.ToString();
                        }
                        prod10380.vintage = item.Vintage.ToString();
                        prod10380.cost = "";
                        fname.pname = item.Name;
                        fname.pdesc = item.Description;
                        fname.pcat = item.Department;
                        fname.pcat1 = item.Category;
                        fname.pcat2 = "";
                        fname.region = "";
                        fname.country = "";
                        fname.upc = "#" + item.upc.ToString().Trim();
                        fname.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                        fname.uom = item.Size;
                        fname.Price = item.Retail;
                        if (WebActive)
                        {
                            exprod10380.Add(prod10380);
                        }
                        fullname.Add(fname);
                    }
                    if (LocationId != 0)
                    {
                        if (item.LocationId == LocationId)
                        {
                            
                            if (missionstores.Contains(StoreId.ToString()))
                            {                             
                                if (item.Size != null)
                                {
                                    var Depositsize = item.Size.ToString().Trim();
                                    prod.storeid = StoreId;
                                    prod.StoreProductName = item.Name;
                                    prod.Storedescription = item.Description;
                                    prod.pack = 1;
                                    prod.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                                    fname.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                                    if (item.Suggested > 0)
                                    {
                                        prod.price = item.Suggested;
                                        fname.Price = item.Suggested;
                                    }
                                    else { continue; }
                                    if (prod.price <= 0 || fname.Price<= 0)
                                    {
                                        continue;
                                    }
                                    prod.qty = Convert.ToInt32(item.QuantityOnHand);
                                    if (!string.IsNullOrEmpty(item.upc))
                                    {
                                        prod.upc = "#" + item.upc.ToString().Trim();
                                        fname.upc = "#" + item.upc.ToString().Trim();
                                    }
                                    else
                                    { continue; }
                                    prod.sprice = 0;
                                    prod.start = "";
                                    prod.end = "";
                                    prod.tax = tax;
                                    prod.altupc1 = "";
                                    prod.altupc2 = "";
                                    prod.altupc3 = "";
                                    prod.altupc4 = "";
                                    prod.altupc5 = "";
                                    var SizeExists = Deposits.Where(a => a.Size == Depositsize).FirstOrDefault();
                                    if (SizeExists != null)
                                    {
                                        var deposit = SizeExists.Deposit;
                                        prod.deposit = deposit.ToString();
                                    }
                                    fname.pname = item.Name;
                                    fname.pdesc = item.Description;
                                    fname.pcat = item.Department;
                                    fname.pcat1 = item.Category;
                                    fname.pcat2 = "";
                                    fname.region = "";
                                    fname.country = "";
                                    fname.uom = item.Size;
                                    if (WebActive)
                                    {
                                        exProd.Add(prod);
                                        fullname.Add(fname);
                                    }
                                }
                            }
                            else if (irrespectiveofqty.Contains(StoreId.ToString()))
                            {                                
                                prod.storeid = StoreId;
                                prod.StoreProductName = item.Name;
                                prod.Storedescription = item.Description;
                                fname.pname = item.Name;
                                fname.pdesc = item.Description;
                                prod.pack = 1;
                                prod.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                                fname.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                                if (item.Department == "CIGARETTES & CIGARS")
                                {
                                    continue;
                                }
                                fname.pcat = item.Department;
                                fname.pcat1 = item.Category;
                                fname.pcat2 = "";
                                fname.region = "";
                                fname.country = "";
                                fname.uom = item.Size;
                                if (item.Retail > 0)
                                {
                                    prod.price = item.Retail;
                                    fname.Price = item.Retail;
                                }else
                                { continue; }

                                
                                if (pricing.Contains(StoreId.ToString()) && item.Retail > 0)
                                {
                                    prod.price = (Math.Round(item.Retail, 2));
                                    fname.Price = (Math.Round(item.Retail, 2));
                                }
                                if (prod.price <= 0 || fname.Price <= 0)
                                {
                                    continue;
                                }
                                prod.tax = tax;
                                prod.qty = Convert.ToInt64(item.QuantityOnHand);
                                if(staticqty.Contains(StoreId.ToString()))
                                {
                                    prod.qty = 999;
                                }
                                if (!string.IsNullOrEmpty(item.upc))
                                {
                                    prod.upc = "#" + item.upc.ToString().Trim();
                                    fname.upc = "#" + item.upc.ToString().Trim();
                                }
                                else
                                {
                                    continue;
                                }
                                if (IsSalePrice)
                                {
                                    DateTime StartDate = DateTime.Now.Date;
                                    DateTime EndDate = DateTime.Now.Date;
                                    double DiscountAmount = 0;
                                    double DiscountPercent = 0;

                                    bool ExitLoop = false;

                                    foreach (var xItem in promo)
                                    {
                                        if (xItem.EndDate > DateTime.Now.Date && xItem.Description != "Club Pricing")
                                        {
                                            foreach (var yItem in xItem.PromotionItems)
                                            {
                                                if (yItem.SkuNumber == item.SkuNumber)
                                                {
                                                    StartDate = xItem.StartDate;
                                                    EndDate = xItem.EndDate;
                                                    DiscountAmount = yItem.DiscountAmount;
                                                    DiscountPercent = yItem.DiscountPercent;

                                                    ExitLoop = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (ExitLoop)
                                        {
                                            break;
                                        }
                                    }
                                    DateTime today = DateTime.Now.Date;
                                    if (EndDate > today)
                                    {
                                        if (DiscountAmount > 0)
                                        {
                                            prod.sprice = item.Retail - Convert.ToDecimal(DiscountAmount);
                                        }

                                        else
                                        {
                                            prod.sprice = item.Retail - (item.Retail * Convert.ToDecimal(DiscountPercent) / 100);
                                        }
                                    }
                                    else
                                    {
                                        prod.sprice = 0;
                                    }

                                    if (prod.sprice > 0)
                                    {
                                        prod.start = StartDate.ToString("MM/dd/yyyy");
                                        prod.end = EndDate.ToString("MM/dd/yyyy");
                                    }
                                    else
                                    {
                                        prod.start = "";
                                        prod.end = "";
                                    }
                                }
                                else
                                {
                                    prod.sprice = 0;
                                    prod.start = "";
                                    prod.end = "";
                                }
                                string day = DateTime.Now.DayOfWeek.ToString().ToUpper();
                                prod.altupc1 = "";
                                prod.altupc2 = "";
                                prod.altupc3 = "";
                                prod.altupc4 = "";
                                prod.altupc5 = "";

                                if (staticqty.Contains(StoreId.ToString()) && WebActive)
                                {
                                    if(fname.pcat == "WINE" || fname.pcat == "BEER")
                                    {
                                        continue;
                                    }                                    
                                }                               
                                if (WebActive)
                                {
                                    exProd.Add(prod);
                                    fullname.Add(fname);
                                }
                            }
                            else
                            {                               
                                prod.storeid = StoreId;
                                if (liquorworldstores.Contains(StoreId.ToString()))
                                {                                    
                                    prod.StoreProductName = item.Name + Regex.Replace(item.Description, @"^""|""$|\\n?", "").Replace("'", string.Empty);
                                    prod.Storedescription = item.Name + Regex.Replace(item.Description, @"^""|""$|\\n?", "").Replace("'", string.Empty);
                                    if (item.Size.ToUpper().ToString().Trim() == "KEG")
                                    {
                                        prod.deposit = "30";
                                    }
                                }
                                else
                                {
                                    prod.StoreProductName = item.Name.Replace("*", string.Empty);
                                    prod.Storedescription = Regex.Replace(item.Description, @"^""|""$|\\n?", "").Replace("'", string.Empty);
                                }
                                fname.pname = item.Name;
                                fname.pdesc = Regex.Replace(item.Description, @"^""|""$|\\n?", "").Replace("'", string.Empty);
                                prod.pack = 1;
                                prod.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                                fname.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                                if (item.Department == "CIGARETTES & CIGARS")
                                {
                                    continue;
                                }
                                fname.pcat = item.Department.ToString().ToUpper();
                                fname.pcat1 = item.Category;
                                fname.pcat2 = "";
                                fname.region = "";
                                fname.country = "";
                                fname.uom = item.Size;

                                if (item.Retail > 0)
                                {
                                    if (liquorworldstores.Contains(StoreId.ToString()) && item.Suggested > 0)
                                    {
                                        prod.price = item.Suggested;
                                        fname.Price = item.Suggested;
                                    }
                                    else
                                    {
                                        prod.price = item.Retail;
                                        fname.Price = item.Retail;
                                    }
                                }
                                if (prod.price <= 0 || fname.Price <=0)
                                {
                                    continue;
                                }
                                if (cattax.Contains(StoreId.ToString()))
                                {
                                    if (item.Department.ToUpper() == "LIQUOR" || item.Department.ToUpper() == "WINE")
                                    {
                                        prod.tax = Convert.ToDecimal(LiquorWineTax);
                                    }
                                    else if (item.Department.ToUpper() == "BEER")
                                    {
                                        prod.tax = Convert.ToDecimal(BeerTax);
                                    }   
                                    else if (item.Department.ToUpper() == "BAR & ACCESSORIES" || item.Department.ToUpper() == "Miscellaneous")
                                    {
                                        prod.tax = Convert.ToDecimal(MiscNonAlcoholTax);
                                    }   
                                    else if (item.Department.ToUpper() == "FOOD ONLY")
                                    {
                                        prod.tax = Convert.ToDecimal(GroceryTax);
                                    }
                                    else
                                    {
                                        prod.tax = Convert.ToDecimal(tax);
                                    }
                                }
                                else
                                {
                                    prod.tax = tax;
                                }
                                if (item.QuantityOnHand > 0 && item.QuantityOnHand < 9999)
                                {
                                    prod.qty = Convert.ToInt32(item.QuantityOnHand);
                                }
                                else
                                {
                                    //continue;
                                }
                                if (!string.IsNullOrEmpty(item.upc))
                                {
                                    prod.upc = "#" + item.upc.ToString().Trim();
                                    fname.upc = "#" + item.upc.ToString().Trim();
                                }
                                else
                                {
                                    continue;
                                }
                                if (IsSalePrice)
                                {
                                    DateTime StartDate = DateTime.Now.Date;
                                    DateTime EndDate = DateTime.Now.Date;
                                    double DiscountAmount = 0;
                                    double DiscountPercent = 0;

                                    bool ExitLoop = false;

                                    foreach (var xItem in promo)
                                    {
                                        if (xItem.EndDate > DateTime.Now.Date && xItem.Description != "Club Pricing")
                                        {
                                            foreach (var yItem in xItem.PromotionItems)
                                            {
                                                if (yItem.SkuNumber == item.SkuNumber)
                                                {
                                                    StartDate = xItem.StartDate;
                                                    EndDate = xItem.EndDate;
                                                    DiscountAmount = yItem.DiscountAmount;
                                                    DiscountPercent = yItem.DiscountPercent;

                                                    ExitLoop = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (ExitLoop)
                                        {
                                            break;
                                        }
                                    }
                                    DateTime today = DateTime.Now.Date;
                                    if (EndDate > today)
                                    {
                                        
                                        if (s_price.Contains(StoreId.ToString()) && EndDate.Year.ToString() == "2059")
                                        {
                                            prod.sprice = 0;
                                        }
                                        else
                                        {
                                            if (DiscountAmount > 0)
                                            {
                                                prod.sprice = item.Retail - Convert.ToDecimal(DiscountAmount);
                                            }
                                            else
                                            {
                                                prod.sprice = item.Retail - (item.Retail * Convert.ToDecimal(DiscountPercent) / 100);
                                            }
                                        }
                                        
                                        if (s_price0.Contains(StoreId.ToString()))                                                                                                           //12166 removed sprice
                                        {
                                            if (prod.sku == "#8379")
                                            {
                                                prod.sprice = 0;
                                            }
                                            else if (prod.sku == "#5402")
                                            {
                                                prod.sprice = 0;
                                            }
                                            else if (prod.sku == "#3354")
                                            {
                                                prod.sprice = 0;
                                            }
                                            else if (prod.sku == "#3327")
                                            {
                                                prod.sprice = 0;
                                            }
                                            else if (prod.sku == "#12659")
                                            {
                                                prod.sprice = 0;
                                            }
                                            else
                                            {
                                                prod.sprice = item.Retail - Convert.ToDecimal(DiscountAmount);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        prod.sprice = 0;
                                    }

                                    if (prod.sprice > 0)
                                    {
                                        prod.start = StartDate.ToString("MM/dd/yyyy");
                                        prod.end = EndDate.ToString("MM/dd/yyyy");
                                    }
                                    else
                                    {
                                        prod.start = "";
                                        prod.end = "";
                                    }
                                }
                                else
                                {
                                    if (item.Size == "750ML" || item.Size == "1.0L" || item.Size == "1.75L" || item.Size == "1.5L" || item.Size == "3.0L" || item.Size == "5.0L" || item.Size == "2L")
                                    {
                                        string cat_sprice = ConfigurationManager.AppSettings["cat_sprice"];
                                        if (cat_sprice.Contains(StoreId.ToString()) && item.Department.ToUpper() == "LIQUOR")
                                        {
                                            prod.sprice = prod.price - ((decimal)10 / 100 * prod.price);
                                            prod.start = DateTime.Now.ToString("MM/dd/yyyy");
                                            prod.end = "12/31/2020";
                                        }
                                        else if (cat_sprice.Contains(StoreId.ToString()) && item.Department.ToUpper() == "WINE")
                                        {
                                            prod.sprice = prod.price - ((decimal)20 / 100 * prod.price);
                                            prod.start = DateTime.Now.ToString("MM/dd/yyyy");
                                            prod.end = "12/31/2020";
                                        }
                                        else
                                        {
                                            prod.sprice = 0;
                                            prod.start = "";
                                            prod.end = "";
                                        }
                                    }
                                    else
                                    {
                                        prod.sprice = 0;
                                        prod.start = "";
                                        prod.end = "";
                                    }
                                    
                                    if (cat_price.Contains(StoreId.ToString()))
                                    {

                                        if (item.Department.ToUpper() == "LIQUOR")
                                        {
                                            if (item.Size == "750ML" || item.Size == "1.0L" || item.Size == "1.5L" || item.Size == "1.75L")
                                            {
                                                prod.price = prod.price - ((decimal)0.10 / 100 * prod.price);
                                                fname.Price = fname.Price - ((decimal)0.10 / 100 * prod.price);
                                            }
                                        }
                                        else if (item.Department.ToUpper() == "WINE")
                                        {
                                            if (item.Size == "750ML" || item.Size == "1.0L" || item.Size == "1.5L" || item.Size == "3.0L" || item.Size == "4.0L" || item.Size == "5.0L")
                                            {
                                                prod.price = prod.price - ((decimal)0.20 / 100 * prod.price);
                                                fname.Price = fname.Price - ((decimal)0.20 / 100 * prod.price);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        prod.price = item.Retail;
                                        fname.Price = item.Retail;
                                    }
                                }
                                
                                string day = DateTime.Now.DayOfWeek.ToString().ToUpper();
                                if (day == "WEDNESDAY")
                                {
                                    decimal prc = 0;
                                    if (fname.pcat.ToUpper() == "WINE")
                                    {
                                        if (wedsaleprice.Contains(StoreId.ToString()))
                                        {
                                            prc = prod.price - ((decimal)20 / 100 * prod.price);
                                            if (prod.sprice > 0)
                                            {
                                                if (prc < prod.sprice)
                                                {
                                                    prod.sprice = prc;
                                                    fname.Price = item.Retail;
                                                    prod.start = DateTime.Now.ToString("MM/dd/yyyy");
                                                    prod.end = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                                }
                                            }
                                            else
                                            {
                                                prod.sprice = prc;
                                                fname.Price = prc;
                                                prod.start = DateTime.Now.ToString("MM/dd/yyyy");
                                                prod.end = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                            }
                                        }
                                    }
                                }
                                
                                if (day == "THURSDAY")
                                {
                                    decimal prc = 0;
                                    if (fname.pcat.ToUpper() == "WINE")
                                    {
                                        if (trsdysprice.Contains(StoreId.ToString()))
                                        {
                                            prc = prod.price - ((decimal)15 / 100 * prod.price);
                                            if (prod.sprice > 0)
                                            {
                                                if (prc < prod.sprice)
                                                {
                                                    prod.sprice = prc;
                                                    fname.Price = prc;
                                                    prod.start = DateTime.Now.ToString("MM/dd/yyyy");
                                                    prod.end = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                                }
                                            }
                                            else
                                            {
                                                prod.sprice = prc;
                                                fname.Price = prc;
                                                prod.start = DateTime.Now.ToString("MM/dd/yyyy");
                                                prod.end = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                            }
                                        }
                                    }
                                }
                                if (prod.price <= 0 || fname.Price <= 0)
                                {
                                    continue;
                                }
                                prod.altupc1 = "";
                                prod.altupc2 = "";
                                prod.altupc3 = "";
                                prod.altupc4 = "";
                                prod.altupc5 = "";

                                if (WebActive && prod.qty > 0)
                                {
                                    exProd.Add(prod);
                                    fullname.Add(fname);
                                }
                            }
                        }
                    }
                    else
                    {
                        prod.storeid = StoreId;
                        prod.StoreProductName = item.Name;
                        fname.pname = item.Name;
                        prod.Storedescription = Regex.Replace(item.Description, @"^""|""$|\\n?", "").Replace("'", string.Empty);
                        fname.pdesc = Regex.Replace(item.Description, @"^""|""$|\\n?", "").Replace("'", string.Empty);
                        prod.pack = 1;
                        prod.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                        fname.sku = "#" + Convert.ToString(item.SkuNumber).Trim();
                        fname.pcat = item.Department;
                        fname.pcat1 = item.Category;
                        fname.pcat2 = "";
                        fname.region = "";
                        fname.country = "";
                        fname.uom = item.Size;
                        
                        if (uomassize.Contains(StoreId.ToString()))
                        {
                            prod.uom = item.Size;
                        }
                        if (item.Retail > 0)
                        {
                            prod.price = item.Retail;
                            fname.Price = item.Retail;
                        }
                        if(prod.price <=0)
                        {
                            continue;
                        }
                        prod.tax = tax;
                        if (item.QuantityOnHand > 0)
                        {
                            prod.qty = Convert.ToInt32(item.QuantityOnHand);
                        }
                        else
                        {
                            //continue;
                        }
                        if (!string.IsNullOrEmpty(item.upc))
                        {
                            prod.upc = "#" + item.upc.ToString().Trim();
                            fname.upc = "#" + item.upc.ToString().Trim();
                        }
                        else
                        {
                            //continue;
                        }
                        prod.sprice = 0;
                        prod.start = "";
                        prod.end = "";
                        prod.altupc1 = "";
                        prod.altupc2 = "";
                        prod.altupc3 = "";
                        prod.altupc4 = "";
                        prod.altupc5 = "";
                        
                        if (specialprice.Contains(StoreId.ToString()) && prod.upc == "#991072799996") //13806
                        {
                            prod.price = 75.00M;
                            fname.Price = 75.00M;
                        }
                        if (specialprice.Contains(StoreId.ToString())&& prod.upc == "#991072700003")
                        {
                            prod.price = 70.00M;
                            fname.Price = 70.00M;
                        }
                        
                        if (catdeposit.Contains(StoreId.ToString()))
                        {
                            if (fname.pcat1 == "KEGS")
                            {
                                prod.deposit = item.Deposit.ToString();
                            }
                            else
                            {
                                prod.deposit = "0";
                            }
                        }
                        
                        if (catqty.Contains(StoreId.ToString()))
                        {
                            if (fname.pcat == "DEPOSITS" && fname.pcat1 == "TAP DEPOSITS")
                            {
                                prod.qty = 999;
                            }
                            if (fname.pcat == "ICE" && fname.pcat1 == "ICE")
                            {


                                prod.qty = 999;
                            }
                        }
                        if (WebActive)
                        {
                            exProd.Add(prod);
                            fullname.Add(fname);
                        }
                    }

                }
                foreach (var item in result1)
                {
                    if (frequentstores.Contains(StoreId.ToString()))
                    {
                        frequentshopper = new FrequentShopper();
                        onetimefile = new Cus();
                        frequentshopper.storeid = StoreId;
                        frequentshopper.currentfs = item.Points;
                        frequentshopper.cdollars = 0;
                        frequentshopper.points2c = 10000 - item.Points;
                        frequentshopper.loyaltyno = item.RewardsNumber;
                        fshopper.Add(frequentshopper);
                    }
                }
                string UploadPath = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + StoreId + "\\Upload\\" + "PRODUCT" + StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                string UploadPathfull = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + StoreId + "\\Upload\\" + "FULLNAME" + StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                string FrequentShopperPath = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + StoreId + "\\Upload\\" + "FREQUENT" + StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                string CustomerPath = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + StoreId + "\\Upload\\" + "CUSTOMER" + StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                if (differentmapping.Contains(StoreId.ToString()))
                {
                    CreateCSVFromGenericList<ExportProductFor10380>(exprod10380, UploadPath);
                    CreateCSVFromGenericList<clsFullnameModel>(fullname, UploadPathfull);
                }
                else if (frequentstores.Contains(StoreId.ToString()))
                {
                    CreateCSVFromGenericList<ExportProduct>(exProd, UploadPath);
                    CreateCSVFromGenericList<clsFullnameModel>(fullname, UploadPathfull);
                    CreateCSVFromGenericList<FrequentShopper>(fshopper, FrequentShopperPath);
                }
                else
                {
                    CreateCSVFromGenericList<ExportProduct>(exProd, UploadPath);
                    CreateCSVFromGenericList<clsFullnameModel>(fullname, UploadPathfull);
                }
            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in MPOWER_POS@" + DateTime.UtcNow + " GMT", ex.Message + StoreId + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
        }
    }
    public class clsProduct
    {
        public List<Results> results { get; set; }
        public int CurrentPage { get; set; }
        public int TotalItemCount { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
    }
    public class Results
    {
        public string SkuNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public long QuantityOnHand { get; set; }
        public long QuantityAvailable { get; set; }
        public string Vintage { get; set; }
        public string Cost { get; set; }
        public double Deposit { get; set; }
        public decimal Retail { get; set; }
        public decimal Suggested { get; set; }
        public int LocationId { get; set; }
        public string upc { get; set; }
        public string Size { get; set; }
        public string Category { get; set; }
        public bool WebActive { get; set; }
        public AdditionalUpcs AdditionalUpc { get; set; }

        public string CustomerNumber { get; set; }
        public int CustomerId { get; set; }
        public string RewardsNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime CustomerSince { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime Birthday { get; set; }
        public double Points { get; set; }
        public bool NoDiscounts { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public int Id { get; set; }
    }
    public class AdditionalUpcs
    {
        public string Upc { get; set; }
        public string Description { get; set; }
        public string CaseQuantity { get; set; }
        public decimal Retail { get; set; }
        public string PosQuantity { get; set; }
    }

    public class Cus
    {
        public string CustomerNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CustomerSince { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
    }
    public class RootObject
    {
        public string ID { get; set; }
        public string SizeMajorNameID { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
        public string Weight { get; set; }
        public string uomid { get; set; }
        public string BottleDeposit { get; set; }
        public string Number { get; set; }
        public string SizeGroup { get; set; }
        public string Deleted { get; set; }
        public int PackColumnValue { get; set; }
    }
    public class DepositRoot
    {
        public string Size { get; set; }
        public decimal Deposit { get; set; }
    }

    // For Customer

    public class Address
    {
        public string Company { get; set; }
        public bool PrimaryShipping { get; set; }
        public bool PrimaryBilling { get; set; }
        public string FirstAddressName { get; set; }
        public string LastAddressName { get; set; }
        public string Phone { get; set; }
        public object Name { get; set; }
        public int AddressType { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public object County { get; set; }
        public string Country { get; set; }
    }

    public class PhoneNumber
    {
        public string Number { get; set; }
        public string Description { get; set; }
        public bool Primary { get; set; }
    }

    public class CustomerGroup
    {
        public string Name { get; set; }
    }

    public class CustomerResult
    {
        public string CustomerNumber { get; set; }
        public int CustomerId { get; set; }
        public string RewardsNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime CustomerSince { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime Birthday { get; set; }
        public double Points { get; set; }
        public bool NoDiscounts { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public List<Address> Addresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<CustomerGroup> CustomerGroups { get; set; }
    }

    public class clsCustomer
    {
        public int CurrentPage { get; set; }
        public int TotalCustomerCount { get; set; }
        public int TotalPages { get; set; }
        public int CustomersPerPage { get; set; }
        public List<CustomerResult> customerResult { get; set; }
    }
    // For Promotions Sprice

    public class PromotionItem
    {
        public string SkuNumber { get; set; }
        public double DiscountAmount { get; set; }
        public double DiscountPercent { get; set; }
        public double MinimumQuantity { get; set; }
    }

    public class Result
    {
        public int PromotionId { get; set; }
        public string Description { get; set; }
        public List<int> LocationIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Notes { get; set; }
        public string CouponCode { get; set; }
        public string CampaignName { get; set; }
        public List<object> CustomerGroupIds { get; set; }
        public List<PromotionItem> PromotionItems { get; set; }
    }

    public class Promotion
    {
        public int CurrentPage { get; set; }
        public int TotalItemCount { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public List<Result> Results { get; set; }
    }
}