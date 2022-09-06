using Common.MasterModel;
using DataAccess.CustomModel;
using DataAccess.DataModels;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Business.Service
{
    public class BroadcastReportManager
    {
        CategoryManager _categoryManager;
        IVRPhoneManager _phoneManager;
        OragnizationManager _objOragnizationManager;
        ChatRoomManager _chatRoomManager;

        //constructor
        public BroadcastReportManager()
        {
            _categoryManager = new CategoryManager();
            _phoneManager = new IVRPhoneManager();
            _objOragnizationManager = new OragnizationManager();
            _chatRoomManager = new ChatRoomManager();
        }
        public LayoutBroadcastModel GetLayoutBroadCastModel()
        {
            LayoutBroadcastModel layoutBroadcastModel = new LayoutBroadcastModel();

            if (ManagerBase.CurrentSuperAdminSession.SuperAdminId != 0)
            {
                List<Common.MasterModel.OrganizationVm> organizationVms = _objOragnizationManager.GetAllOrganization();
                layoutBroadcastModel.iVRPhoneVms = _phoneManager.GetIVRPhoneofAllOrganizations();
                layoutBroadcastModel.OrganizationsList = organizationVms;
                layoutBroadcastModel.CategoryList = _categoryManager.GetAllCategories();
            }
            else if (ManagerBase.CurrentUserSession.UserId != 0)
            {
                layoutBroadcastModel.CategoryList = ManagerBase.CurrentUserSession.lstCategoryPermissionVm.Where(x=>x.Run_Report==true).ToList();
                layoutBroadcastModel.iVRPhoneVms = _chatRoomManager.GetIVRPhoneAccordingToCategoryPermission();
            }

            return layoutBroadcastModel;
        }
        // Add Columns to Grid Column Table which take care of column list according to different modules
        public void AddColumnsToGridColumns()
        {
            // Add GridColumns
            CommonManager commonManager = new CommonManager();
            List<TableSetting> tableSettings = new List<TableSetting>();
            var Properties = new BroadcastReportVm().GetType().GetProperties();
            int count = 1;

            foreach (var Property in Properties)
            {
                var tableSetting = new TableSetting();
                tableSetting.ColumnName = Property.Name;
                tableSetting.DisplayName = Property.Name;
                tableSetting.IsVisible = true;
                tableSetting.Sequence = count;
                tableSettings.Add(tableSetting);
                count++;
            }

            commonManager.AddGridColumns(tableSettings, "BroadcastReport");
        }
        //Get Saved Columns order and visibility for table
        public List<TableSetting> GetColumnModel()
        {
            EntityModel context = new EntityModel();
            using (IUnitofWork _unitofWork = new UnitOfWork(context))
            {
                List<TableSetting> tableSettings = new List<TableSetting>();
                if (ManagerBase.CurrentSuperAdminSession.SuperAdminId != 0)
                {
                    var ModalColumns = _unitofWork.SuperAdminRepository.GetSuperAdminColumnSettingsModel(ManagerBase.CurrentSuperAdminSession.SuperAdminId, "BroadcastReport");
                    ModalColumns.ForEach(x =>
                    {
                        tableSettings.Add(new TableSetting
                        {
                            ColumnName = x.ColumnName,
                            IsVisible = x.IsVisible,
                            Sequence = x.ColumnSequence,
                            ColumnId = x.ColumnId,
                            DisplayName = x.DisplayName
                        });
                    });
                }
                else
                {
                    var ModalColumns = new List<OrganizationUserSettingModel>();
                    if (ManagerBase.CurrentUserSession.IsOrganizationAdmin)
                    {
                        ModalColumns = _unitofWork.CommonRepository.GetOrganizationUserSettingModel(ManagerBase.CurrentUserSession.OrganizationId, null, Enum.GetName(typeof(ModuleName), ModuleName.BroadcastReport));
                    }
                    else
                    {
                        ModalColumns = _unitofWork.CommonRepository.GetOrganizationUserSettingModel(ManagerBase.CurrentUserSession.OrganizationId, ManagerBase.CurrentUserSession.UserId, Enum.GetName(typeof(ModuleName), ModuleName.BroadcastReport));

                    }

                    ModalColumns.ForEach(x =>
                    {
                        tableSettings.Add(new TableSetting
                        {
                            ColumnName = x.ColumnName,
                            IsVisible = x.IsVisible,
                            Sequence = x.ColumnSequence,
                            ColumnId = x.ColumnId,
                            DisplayName = x.DisplayName
                        });
                    });
                }
                return tableSettings;
            }
        }
        //Save Columns order and Visibility in database
        public bool SavePeopleOrganizationUserSetting(List<TableSetting> tableSettings)
        {
            EntityModel context = new EntityModel();
            using (IUnitofWork _unitofWork = new UnitOfWork(context))
            {
                var module = ModuleName.BroadcastReport;
                if (ManagerBase.CurrentSuperAdminSession.SuperAdminId != 0)
                {
                    var columnSettings = _unitofWork.SuperAdminRepository.GetSuperAdminColumnSettings(ManagerBase.CurrentSuperAdminSession.SuperAdminId, Enum.GetName(typeof(ModuleName), module));

                    if (columnSettings.Count > 0)
                    {
                        _unitofWork.SuperAdminRepository.RemoveSuperAdminColumnSetting(columnSettings);
                    }

                    List<SuperAdminColumnSettings> superAdminColumnSettings = new List<SuperAdminColumnSettings>();

                    tableSettings.ForEach(x =>
                    {
                        superAdminColumnSettings.Add(new SuperAdminColumnSettings
                        {
                            SuperAdminId = ManagerBase.CurrentSuperAdminSession.SuperAdminId,
                            ColumnSequence = x.Sequence,
                            IsVisible = x.IsVisible,
                            ColumnId = x.ColumnId
                        });
                    });

                    _unitofWork.SuperAdminRepository.AddSuperAdminColumnSettings(superAdminColumnSettings);
                }
                else if (!ManagerBase.CurrentUserSession.IsOrganizationAdmin)
                {
                    var columnSettings = _unitofWork.CommonRepository.GetOrganizationUserSetting(ManagerBase.CurrentUserSession.OrganizationId, ManagerBase.CurrentUserSession.UserId, Enum.GetName(typeof(ModuleName), module));

                    if (columnSettings.Count > 0)
                    {
                        _unitofWork.CommonRepository.RemoveOrganizationUserSetting(columnSettings);
                    }

                    List<OrganizationUserSetting> organizationUserSettings = new List<OrganizationUserSetting>();

                    tableSettings.ForEach(x =>
                    {
                        organizationUserSettings.Add(new OrganizationUserSetting
                        {
                            UserId = ManagerBase.CurrentUserSession.UserId,
                            OrganizationId = ManagerBase.CurrentUserSession.OrganizationId,
                            ColumnSequence = x.Sequence,
                            IsVisible = x.IsVisible,
                            ColumnId = x.ColumnId,
                            CreatedDate = DateTime.UtcNow
                        });
                    });

                    _unitofWork.CommonRepository.AddOrganizationUserSetting(organizationUserSettings);
                }
                else
                {
                    var data = _unitofWork.CommonRepository.GetOrganizationUserSetting(ManagerBase.CurrentUserSession.OrganizationId, null, Enum.GetName(typeof(ModuleName), module));

                    if (data.Count > 0)
                    {
                        _unitofWork.CommonRepository.RemoveOrganizationUserSetting(data);
                    }

                    List<OrganizationUserSetting> organizationUserSettings = new List<OrganizationUserSetting>();

                    tableSettings.ForEach(x =>
                    {
                        organizationUserSettings.Add(new OrganizationUserSetting
                        {
                            OrganizationId = ManagerBase.CurrentUserSession.OrganizationId,
                            ColumnSequence = x.Sequence,
                            IsVisible = x.IsVisible,
                            ColumnId = x.ColumnId,
                            CreatedDate = DateTime.UtcNow
                        });
                    });

                    _unitofWork.CommonRepository.AddOrganizationUserSetting(organizationUserSettings);
                }

                _unitofWork.Save();
            }
            return true;
        }
        public List<IVRPhoneVm> GetIVRPhoneAccordingToSelectedCategories(List<int> categoryIds)
        {
            EntityModel context = new EntityModel();

            using (IUnitofWork _unitofWork = new UnitOfWork(context))
            {
                IVRPhoneManager objIvrManager = new IVRPhoneManager();
                List<List<IVRPhoneVm>> iVRPhoneVms = new List<List<IVRPhoneVm>>();

                var allIVRPhones = objIvrManager.GetIVRPhoneofAllOrganizations();
                var permittedCategoriesCount = categoryIds.Count();
                if (permittedCategoriesCount > 0)
                {
                    allIVRPhones = allIVRPhones.Where(c => categoryIds.Select(y => y).Any(z => c.CategoryId.Equals(z))).ToList();
                }
                else
                {
                    allIVRPhones = null;
                }



                return allIVRPhones;
            }

        }
        public List<List<IVRPhoneVm>> GetIVRPhoneAccordingToSelectedOrganization(List<int> organizationIds)
        {
            EntityModel context = new EntityModel();

            using (IUnitofWork _unitofWork = new UnitOfWork(context))
            {
                IVRPhoneManager objIvrManager = new IVRPhoneManager();
                var allIVRPhones = new List<List<IVRPhoneVm>>();
                if (organizationIds.Count > 0)
                {
                    foreach (int organizationId in organizationIds)
                    {
                        allIVRPhones.Add((objIvrManager.GetAllOrganizationIVRPhone(organizationId)));                       
                    }
                }
                return allIVRPhones;
            }

        }
        //Convert Broadcast's records from List to DataTable
        public DataTable ToDataTable<T>(List<T> items, List<string> columns)
        {
            DataTable transactionRecordsTable = new DataTable(typeof(T).Name);

            //Get all the properties by using reflection   
            var Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            var byOrder = new List<PropertyInfo>();

            columns.ForEach(x =>
            {
                var data = Props.Where(prop => prop.Name == BroadcastReportModuleOrderMpping.GetColumnValueByKey(x)).FirstOrDefault();
                if (data != null)
                    byOrder.Add(data);
            });

            Props = byOrder;

            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                transactionRecordsTable.Columns.Add(prop.Name);
            }

            foreach (T item in items)
            {
                var values = new object[Props.Count];
                for (int i = 0; i < Props.Count; i++)
                {
                    if (Props[i].PropertyType == typeof(DateTime?))
                    {
                        var date = (DateTime?)Props[i].GetValue(item, null);
                        if (date != null)
                        {
                            values[i] = date.GetValueOrDefault().ToString("MM/dd/yy");
                        }
                        else
                        {
                            values[i] = string.Empty;
                        }
                    }
                    else if (Props[i].Name == "MessageType")
                    {
                        var type = (int)Props[i].GetValue(item, null);
                        if (type == 1)
                        {
                            values[i] = "SMS";
                        }
                        else
                        {
                            values[i] = "MMS";
                        }
                    }
                    else
                    {
                        values[i] = Props[i].GetValue(item, null);
                    }
                }
                transactionRecordsTable.Rows.Add(values);
            }

            return transactionRecordsTable;
        }
        //Get Broadcast Report from Database
        public List<BroadcastFieldsVm> GetBroadcastReportWithFilter(int start, int length, string SortDir, string SortCol, string search, BroadCastFilterVM transactionsFilterVM)
        {
            EntityModel context = new EntityModel();
            using (IUnitofWork _unitofWork = new UnitOfWork(context))
            {
                string FromDateTime = string.Empty;
                string ToDateTime = string.Empty;

                List<string> categories = transactionsFilterVM.CategoryIds != null ? transactionsFilterVM.CategoryIds.Select(i => '"' + i.ToString() + '"').ToList() : new List<string>();

                BroadcastFilterSPVM filterSetting = new BroadcastFilterSPVM
                {
                    CategoryIds = categories != null ? string.Join(",", categories) : string.Empty,
                    OrganizationIds = transactionsFilterVM.OrganizationIds != null ? string.Join(",", transactionsFilterVM.OrganizationIds) : string.Empty,
                    TerminalIds = transactionsFilterVM.TerminalIds != null ? string.Join(",", transactionsFilterVM.TerminalIds) : string.Empty,
                    MinAmount = transactionsFilterVM.MinAmount.ToString(),
                    MaxAmount = transactionsFilterVM.MaxAmount.ToString(),
                    AccountId = transactionsFilterVM.AccountId.ToString(),
                    Segments = transactionsFilterVM.Segments,
                    SmsSid = transactionsFilterVM.SmsSid,
                    BroadcastFrom = transactionsFilterVM.BroadcastFrom != null ? transactionsFilterVM.BroadcastFrom : string.Empty,
                    BroadcastTo = transactionsFilterVM.BroadcastTo != null ? transactionsFilterVM.BroadcastTo : string.Empty,
                    Textstatus = transactionsFilterVM.Textstatus != null ? string.Join(",", transactionsFilterVM.Textstatus) : string.Empty,

                };

                if (!string.IsNullOrEmpty(filterSetting.BroadcastFrom))
                {
                    DateTime parsedDateTimeFrom = DateTime.ParseExact(filterSetting.BroadcastFrom.Split(' ')[0], "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    filterSetting.BroadcastFrom = parsedDateTimeFrom.ToString("yyyy-MM-dd");
                }

                if (!string.IsNullOrEmpty(filterSetting.BroadcastTo))
                {
                    DateTime parsedDateTimeTo = DateTime.ParseExact(filterSetting.BroadcastTo.Split(' ')[0], "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    filterSetting.BroadcastTo = parsedDateTimeTo.ToString("yyyy-MM-dd");
                }

                List<BroadcastFieldsVm> transactionsHistory = _unitofWork.ChatRoomRepository.GetBroadcastReport(start, length, SortDir, SortCol, search, filterSetting, ManagerBase.CurrentUserSession.OrganizationId, ManagerBase.CurrentUserSession.UserId);

                return transactionsHistory;
            }
        }
    }
}
