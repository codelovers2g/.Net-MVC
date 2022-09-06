using Business.Service;
using Business.Utilities;
using ClosedXML.Excel;
using Common.MasterModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Donation.Controllers
{
    public class BroadcastReportController : Controller
    {
        BroadcastReportManager _broadcastReportManager;
        CategoryManager _categoryManager;
        //Constructor
        public BroadcastReportController()
        {
            _broadcastReportManager = new BroadcastReportManager();
            _categoryManager = new CategoryManager();
        }

        // GET: BroadCast Report
        public ViewResult Index()
        {
            var requiredModel = _broadcastReportManager.GetLayoutBroadCastModel();
            return View("Index", requiredModel);
        }
        //Get List of Column for table 
        [HttpGet]
        public JsonResult GetColumnModal()
        {
            try
            {
                //_broadcastReportManager.AddColumnsToGridColumns();
                var listColumns = _broadcastReportManager.GetColumnModel();
                return Json(listColumns, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.log.Error(ex.Message, ex);
                Log4Net.log.Info("An error occured. Check log for details.");
                return Json(-1, JsonRequestBehavior.AllowGet); 
            }
        }
        [HttpPost]
        public JsonResult GetAllCategoriesofOrg(List<int> organizationIds)
        {
            try
            {
                var categories = _categoryManager.GetAllCategoriesofOrg(organizationIds);
                return Json(categories, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Log Error to database
                Log4Net.log.Error(ex.Message, ex);
                //Log Error to File
                Log4Net.log.Info("An error occured. Check log for details.");
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }
        //Save Column setting for table which includes order and visibility of columns
        [HttpPost]
        public JsonResult SaveFinancialGridOrganizationUserSetting(List<TableSetting> lstTableSetting)
        {
            try
            {
                var isColumnSettingsSaved = _broadcastReportManager.SavePeopleOrganizationUserSetting(lstTableSetting);
                return Json(isColumnSettingsSaved, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.log.Error(ex.Message, ex);
                Log4Net.log.Info("An error occured. Check log for details.");
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult GetFilterMenuData(List<int> categoryIds)
        {
            try
            {
                var filterMenuData = _broadcastReportManager.GetIVRPhoneAccordingToSelectedCategories(categoryIds);
                return Json(filterMenuData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.log.Error(ex.Message, ex);
                Log4Net.log.Info("An error occured. Check log for details.");
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        } 
        [HttpPost]
        public JsonResult GetOrgFilterMenuData(List<int> organizationIds)
        {
            try
            {
                var filterMenuData = _broadcastReportManager.GetIVRPhoneAccordingToSelectedOrganization(organizationIds);
                return Json(filterMenuData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log4Net.log.Error(ex.Message, ex);
                Log4Net.log.Info("An error occured. Check log for details.");
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }
        //Get Broadcast Report from Database
        [HttpPost]
        public JsonResult GetBroadcastReport(DataTableAjaxPostModel Model, BroadCastFilterVM broadcastFilterVm)
        {
            try
            {
                var listTransactions = _broadcastReportManager.GetBroadcastReportWithFilter(Model.start, Model.length, Model.order != null ? Model.order.First().dir : null, Model.order != null ? Model.order.First().column : null, Model.search.value, broadcastFilterVm);
                if (listTransactions != null && listTransactions.Count > 0)
                {
                    var json = Json(new
                    {
                        // this is what datatables wants sending back
                        Model.draw,
                        recordsFiltered = listTransactions[0].Total,
                       recordsTotal = listTransactions[0].Total,
                        data = listTransactions,

                    });
                    json.MaxJsonLength = int.MaxValue;
                    return json;
                }
                else
                {
                    return Json(new
                    {
                        // this is what datatables wants sending back
                        Model.draw,
                       recordsTotal = 0,
                      recordsFiltered = 0,
                        data = listTransactions
                    });
                }
            }
            catch (Exception ex)
            {
                Log4Net.log.Error(ex.Message, ex);
                Log4Net.log.Info("An error occured. Check log for details.");
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult ExportToExcelBroadcastReport(DataTableAjaxPostModel Model, BroadCastFilterVM broadcastFilterVm)
        {
            try
            {
                string handle = Guid.NewGuid().ToString();
                //string interpolation used
                var fileName = $"BroadcastReport({ Guid.NewGuid()}).xlsx";

                var listTransactions = _broadcastReportManager.GetBroadcastReportWithFilter(0, 0, Model.order != null ? Model.order.First().dir : null, Model.order != null ? Model.order.First().column : null, Model.search.value, broadcastFilterVm);
                if (listTransactions != null && listTransactions.Count > 0)
                {
                    //Get columns order and visibilty for table
                    var columnOrder = _broadcastReportManager.GetColumnModel().Where(set => set.IsVisible == true).OrderBy(set => set.Sequence).Select(set => set.DisplayName).ToList();

                    // convert List to datatable according to visibility and order of columns
                    var transactiondata = _broadcastReportManager.ToDataTable(listTransactions, columnOrder);

                    //Create Excel file
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var wsDep = wb.Worksheets.Add(transactiondata);
                        wsDep.Columns().AdjustToContents();

                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            wb.SaveAs(MyMemoryStream);
                            MyMemoryStream.Position = 0;
                            TempData[handle] = MyMemoryStream.ToArray();
                        }
                    }
                    return new JsonResult()
                    {
                        Data = new { FileGuid = handle, FileName = fileName }
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Log4Net.log.Error(ex.Message, ex);
                Log4Net.log.Info("An error occured. Check log for details.");
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }

        // Download Excel file to local system
        [HttpGet]
        public virtual ActionResult DownloadExcelBroadcastReport(string fileGuid, string fileName)
        {
            try
            {
                if (TempData[fileGuid] != null)
                {
                    byte[] data = TempData[fileGuid] as byte[];
                    TempData.Remove("fileGuid");
                    return File(data, "application/vnd.ms-excel", fileName);
                }
                else
                {
                    //redirect to another controller action - whatever fits with your application      
                    return new EmptyResult();
                }

            }
            catch (Exception ex)
            {
                Log4Net.log.Error(ex.Message, ex);
                Log4Net.log.Info("An error occured. Check log for details.");
                return Json(-1, JsonRequestBehavior.AllowGet);

            }
        }
      

    }
}