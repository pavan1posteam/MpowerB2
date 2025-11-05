using MPower.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace MPower
{
    class Program
    {
        static string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        static string MaxCount = ConfigurationManager.AppSettings["MaxCount"];
        private static void Main(string[] args)
        {
            try
            {              
                POSSettings pOSSettings = new POSSettings();
                pOSSettings.IntializeStoreSettings();
                var storeSetting = pOSSettings.PosDetails.Select(m => m.StoreSettings).ToList();

                var objPOSList = new List<List<POSModel>>();
                var POSList = new List<POSModel>();

                while (storeSetting.Count > 0)
                {
                    POSList = new List<POSModel>();
                    foreach (var store in storeSetting)
                    {
                        var storeId = store.StoreId;                        
                        var baseUrl = store.POSSettings.BaseUrl;
                        var apiKey = store.POSSettings.APIKey;
                        var locationId = store.POSSettings.LocationId;
                        var tax = store.POSSettings.tax;
                        var liquorWineTax = store.POSSettings.LiquorWineTax;
                        var beerTax = store.POSSettings.BeerTax;
                        var miscNonAlcoholTax = store.POSSettings.MiscNonAlcoholTax;
                        var groceryTax = store.POSSettings.GroceryTax;
                        var isSalePrice = store.POSSettings.IsSalePrice;                        
                        var isExists = POSList.Any(a => a.ApiKey == apiKey);
                        int maxcount = 0;
                        if (string.IsNullOrEmpty(MaxCount))
                        {
                            MaxCount = "5";
                            maxcount = Convert.ToInt32(MaxCount);
                        }
                        else
                        {
                            maxcount = Convert.ToInt32(MaxCount);
                            if (maxcount <= 0)
                                maxcount = 5;
                        }
                        if (!isExists && POSList.Count < maxcount)
                        {
                            POSList.Add(new POSModel { StoreId = storeId, BaseUrl = baseUrl, ApiKey = apiKey, LocationId = locationId, tax = tax, IsSalePrice = isSalePrice, LiquorWineTax=liquorWineTax, BeerTax = beerTax, MiscNonAlcoholTax = miscNonAlcoholTax,GroceryTax=groceryTax });
                        }
                    }                   
                    if (POSList.Count > 0)
                    {
                        objPOSList.Add(POSList);
                    }
                    storeSetting.RemoveAll(r => objPOSList.SelectMany(s => s).Any(ss => ss.ApiKey == r.POSSettings.APIKey && ss.StoreId == r.StoreId));
                }
                var TaskList = new List<Task>();
                foreach (var obj in objPOSList)
                {
                    TaskList = new List<Task>();
                    foreach (var o in obj)
                    {
                        TaskList.Add(Task.Factory.StartNew(() => Mpower(o)));
                    }
                    Task.WaitAll(TaskList.ToArray());
                    Console.WriteLine();
                }
                #region Old Approach
                //Thread t1 = new Thread(Thread1);
                //Thread t2 = new Thread(Thread2);
                //Thread t3 = new Thread(Thread3);
                //Thread t4 = new Thread(Thread4);
                //Thread t5 = new Thread(Thread5);
                //t1.Start();
                //t2.Start();
                //t3.Start();
                //t4.Start();
                //t5.Start();
                //POSSettings pOSSettings = new POSSettings();
                //pOSSettings.IntializeStoreSettings();
                //foreach (POSSetting current in pOSSettings.PosDetails)
                //{
                //    if (current.PosName.ToUpper() == "MPOWER")
                //    {
                //        //if (current.StoreSettings.StoreId == 10384)
                //        //{
                //        current.StoreSettings.POSSettings.BaseUrl = "https://mpowerapi.azurewebsites.net/api/v1/Items/search?";
                //        clsmPower clsmPower = new clsmPower(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.BaseUrl, current.StoreSettings.POSSettings.APIKey, DeveloperId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.LocationId, current.StoreSettings.POSSettings.Code, current.StoreSettings.POSSettings.IsSalePrice);
                //        Console.WriteLine();
                //        //}

                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in MPOWER_POS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
        }
        public static void Mpower(POSModel model)
        {
            if(model.StoreId == 10394)
            {
                model.BaseUrl = "https://mpowerapi.azurewebsites.net/api/v1/Items/search?";
                clsmPower clsmPower = new clsmPower(model.StoreId, model.BaseUrl, model.ApiKey, model.tax, model.LocationId, model.Code, model.IsSalePrice, model.LiquorWineTax, model.BeerTax, model.MiscNonAlcoholTax, model.GroceryTax);
                Console.WriteLine();
            }                                                           
        }
        public class POSModel
        {
            public int StoreId { get; set; }
            public string BaseUrl { get; set; }
            public string ApiKey { get; set; }
            public int LocationId { get; set; }
            public decimal tax { get; set; }
            public bool IsSalePrice { get; set; }
            public string accessToken { get; set; }
            public string Code { get; set; }
            public decimal LiquorWineTax { get; set; }
            public decimal BeerTax { get; set; }
            public decimal MiscNonAlcoholTax { get; set; }
            public decimal GroceryTax { get; set; }
        }
        #region Old Approach
        //public static void Thread1()
        //{
        //    POSSettings pOSSettings = new POSSettings();
        //    pOSSettings.IntializeStoreSettings();
        //    var total = pOSSettings.PosDetails.Count();
        //    var pageSize = 10; // set your page size, which is number of records per page

        //    var page = 1; // set current page number, must be >= 1 (ideally this value will be passed to this logic/function from outside)

        //    var skip = pageSize * (page - 1);

        //    var canPage = skip < total;

        //    if (!canPage) // do what you wish if you can page no further
        //        return;

        //    var t1 = pOSSettings.PosDetails
        //                 .Skip(skip)
        //                 .Take(pageSize)
        //                 .ToArray();
        //    foreach (POSSetting current in t1)
        //    {
        //        if (current.PosName.ToUpper() == "MPOWER")
        //        {
        //            //if (current.StoreSettings.StoreId == 10384)
        //            //{
        //            current.StoreSettings.POSSettings.BaseUrl = "https://mpowerapi.azurewebsites.net/api/v1/Items/search?";
        //            clsmPower clsmPower = new clsmPower(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.BaseUrl, current.StoreSettings.POSSettings.APIKey, DeveloperId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.LocationId, current.StoreSettings.POSSettings.Code, current.StoreSettings.POSSettings.IsSalePrice);
        //            Console.WriteLine();
        //            //}

        //        }
        //    }
        //}
        //public static void Thread2()
        //{
        //    POSSettings pOSSettings = new POSSettings();
        //    pOSSettings.IntializeStoreSettings();
        //    var total = pOSSettings.PosDetails.Count();
        //    var pageSize = 10; // set your page size, which is number of records per page

        //    var page = 2; // set current page number, must be >= 1 (ideally this value will be passed to this logic/function from outside)

        //    var skip = pageSize * (page - 1);

        //    var canPage = skip < total;

        //    if (!canPage) // do what you wish if you can page no further
        //        return;

        //    var t2 = pOSSettings.PosDetails
        //                 .Skip(skip)
        //                 .Take(pageSize)
        //                 .ToArray();
        //    foreach (POSSetting current in t2)
        //    {
        //        if (current.PosName.ToUpper() == "MPOWER")
        //        {
        //            //if (current.StoreSettings.StoreId == 10384)
        //            //{
        //            current.StoreSettings.POSSettings.BaseUrl = "https://mpowerapi.azurewebsites.net/api/v1/Items/search?";
        //            clsmPower clsmPower = new clsmPower(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.BaseUrl, current.StoreSettings.POSSettings.APIKey, DeveloperId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.LocationId, current.StoreSettings.POSSettings.Code, current.StoreSettings.POSSettings.IsSalePrice);
        //            Console.WriteLine();
        //            //}

        //        }
        //    }
        //}
        //public static void Thread3()
        //{
        //    POSSettings pOSSettings = new POSSettings();
        //    pOSSettings.IntializeStoreSettings();
        //    var total = pOSSettings.PosDetails.Count();
        //    var pageSize = 10; // set your page size, which is number of records per page

        //    var page = 3; // set current page number, must be >= 1 (ideally this value will be passed to this logic/function from outside)

        //    var skip = pageSize * (page - 1);

        //    var canPage = skip < total;

        //    if (!canPage) // do what you wish if you can page no further
        //        return;

        //    var t3 = pOSSettings.PosDetails
        //                 .Skip(skip)
        //                 .Take(pageSize)
        //                 .ToArray();
        //    foreach (POSSetting current in t3)
        //    {
        //        if (current.PosName.ToUpper() == "MPOWER")
        //        {
        //            //if (current.StoreSettings.StoreId == 10384)
        //            //{
        //            current.StoreSettings.POSSettings.BaseUrl = "https://mpowerapi.azurewebsites.net/api/v1/Items/search?";
        //            clsmPower clsmPower = new clsmPower(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.BaseUrl, current.StoreSettings.POSSettings.APIKey, DeveloperId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.LocationId, current.StoreSettings.POSSettings.Code, current.StoreSettings.POSSettings.IsSalePrice);
        //            Console.WriteLine();
        //            //}

        //        }
        //    }
        //}
        //public static void Thread4()
        //{
        //    POSSettings pOSSettings = new POSSettings();
        //    pOSSettings.IntializeStoreSettings();
        //    var total = pOSSettings.PosDetails.Count();
        //    var pageSize = 10; // set your page size, which is number of records per page

        //    var page = 4; // set current page number, must be >= 1 (ideally this value will be passed to this logic/function from outside)

        //    var skip = pageSize * (page - 1);

        //    var canPage = skip < total;

        //    if (!canPage) // do what you wish if you can page no further
        //        return;

        //    var t4 = pOSSettings.PosDetails
        //                 .Skip(skip)
        //                 .Take(pageSize)
        //                 .ToArray();
        //    foreach (POSSetting current in t4)
        //    {
        //        if (current.PosName.ToUpper() == "MPOWER")
        //        {
        //            //if (current.StoreSettings.StoreId == 10384)
        //            //{
        //            current.StoreSettings.POSSettings.BaseUrl = "https://mpowerapi.azurewebsites.net/api/v1/Items/search?";
        //            clsmPower clsmPower = new clsmPower(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.BaseUrl, current.StoreSettings.POSSettings.APIKey, DeveloperId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.LocationId, current.StoreSettings.POSSettings.Code, current.StoreSettings.POSSettings.IsSalePrice);
        //            Console.WriteLine();
        //            //}

        //        }
        //    }
        //}
        //public static void Thread5()
        //{
        //    POSSettings pOSSettings = new POSSettings();
        //    pOSSettings.IntializeStoreSettings();
        //    var total = pOSSettings.PosDetails.Count();
        //    var pageSize = 10; // set your page size, which is number of records per page

        //    var page = 5; // set current page number, must be >= 1 (ideally this value will be passed to this logic/function from outside)

        //    var skip = pageSize * (page - 1);

        //    var canPage = skip < total;

        //    if (!canPage) // do what you wish if you can page no further
        //        return;

        //    var t5 = pOSSettings.PosDetails
        //                 .Skip(skip)
        //                 .Take(pageSize)
        //                 .ToArray();
        //    foreach (POSSetting current in t5)
        //    {
        //        if (current.PosName.ToUpper() == "MPOWER")
        //        {
        //            //if (current.StoreSettings.StoreId == 10384)
        //            //{
        //            current.StoreSettings.POSSettings.BaseUrl = "https://mpowerapi.azurewebsites.net/api/v1/Items/search?";
        //            clsmPower clsmPower = new clsmPower(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.BaseUrl, current.StoreSettings.POSSettings.APIKey, DeveloperId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.LocationId, current.StoreSettings.POSSettings.Code, current.StoreSettings.POSSettings.IsSalePrice);
        //            Console.WriteLine();
        //            //}

        //        }
        //    }
        //}
        #endregion
    }
}
