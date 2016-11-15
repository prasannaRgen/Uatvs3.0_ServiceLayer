using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rgen.UAT.UATToolServiceLayer.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Data.SqlClient;
using System.Xml;
using System.IO;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Rgen.UAT.UATToolServiceLayer.Controllers
{
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private clsDbContext _context;
     

        public DashboardController(clsDbContext context) 
        {
            _context = context;
        }
        [HttpGet]
        public string Get()
        {
            return "value";
        }
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet, Route("GetDashboardData")]
        public JsonResult GetDashboardData()
        {
            try
            {
                string AppUrl = HttpContext.Request.Headers["appurl"];
                string _SpUserId = HttpContext.Request.Headers["LoggedInUserSPUserId"];
                string SchemaName = "";
                if (!string.IsNullOrEmpty(AppUrl))
                {
                    SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    return Json("Invalid Url");

                }

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spDashboardReport";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPUserId", SqlDbType.Int) { Value = _SpUserId });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var retObject = new List<dynamic>();

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var dataRow = new ExpandoObject() as IDictionary<string, object>;
                            for (var iFiled = 0; iFiled < dataReader.FieldCount; iFiled++)
                            {
                                dataRow.Add(dataReader.GetName(iFiled), dataReader.IsDBNull(iFiled) ? null : dataReader[iFiled]);
                            }

                            retObject.Add((ExpandoObject)dataRow);
                        }
                    }
                    return Json(retObject);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }




        }
        [HttpGet, Route("GetUserProjectsWithSecurity")]
        public JsonResult GetUserProjectsWithSecurity()
        {
            try
            {
                string AppUrl = HttpContext.Request.Headers["appurl"];
                string SchemaName = "";
                string _SpUserId = HttpContext.Request.Headers["LoggedInUserSPUserId"];
                if (!string.IsNullOrEmpty(AppUrl))
                {
                    SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    return Json("Invalid Url");

                }

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spGetProjectsWithSecurityForUser";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPUserId", SqlDbType.Int) { Value = _SpUserId });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var retObject = new List<dynamic>();

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var dataRow = new ExpandoObject() as IDictionary<string, object>;

                            for (var iFiled = 0; iFiled < dataReader.FieldCount; iFiled++)
                            {

                                dataRow.Add(dataReader.GetName(iFiled), dataReader.IsDBNull(iFiled) ? null : dataReader[iFiled]);
                            }


                            retObject.Add((ExpandoObject)dataRow);
                        }





                    }
                    return Json(retObject);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }

        [HttpGet, Route("GetDropdownDataForDetailAnalysis")]
        public JsonResult GetDropdownDataForDetailAnalysis()
        {

            try
            {
                string AppUrl = HttpContext.Request.Headers["appurl"];
                string SchemaName = "";
                string _SpUserId = HttpContext.Request.Headers["LoggedInUserSPUserId"];
                if (!string.IsNullOrEmpty(AppUrl))
                {
                    SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    return Json("Invalid Url");

                }

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spDetailAnalysisDropDown";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPUserId", SqlDbType.Int) { Value = _SpUserId });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var retObject = new List<dynamic>();
                    TesterList lst = new TesterList();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var dataRow = new ExpandoObject() as IDictionary<string, object>;

                            for (var iFiled = 0; iFiled < dataReader.FieldCount; iFiled++)
                            {
                                var Name = dataReader.GetName(iFiled);
                                var Value = dataReader.IsDBNull(iFiled) ? null : dataReader[iFiled];
                                List<clsTestterList> oTester = new List<clsTestterList>();
                                string valueCheck = Convert.ToString(Value);
                                if (!string.IsNullOrEmpty(valueCheck))
                                {
                                    #region ' XMl Parsing '

                                    if (valueCheck.Contains('<'))
                                    {
                                        if (Convert.ToString(Name) == "TesterDetailsRowNew")
                                        {
                                            Name = "lstTesterList";
                                            using (XmlReader reader = XmlReader.Create(new StringReader("<users>" + Value + "</users>")))
                                            {
                                                XmlDocument xml = new XmlDocument();
                                                xml.Load(reader);
                                                XmlNodeList companyList = xml.GetElementsByTagName("user");
                                                foreach (XmlNode node in companyList)
                                                {
                                                    XmlElement companyElement = (XmlElement)node;

                                                    oTester.Add(new clsTestterList
                                                    {
                                                        testerId = (companyElement.GetElementsByTagName("TUserId") != null) ? Convert.ToString(companyElement.GetElementsByTagName("TUserId")[0].InnerText) : "",
                                                        testerName = (companyElement.GetElementsByTagName("TUserName") != null) ? Convert.ToString(companyElement.GetElementsByTagName("TUserName")[0].InnerText) : "",
                                                        roleId = (companyElement.GetElementsByTagName("TRoleId") != null) ? Convert.ToString(companyElement.GetElementsByTagName("TRoleId")[0].InnerText) : "",
                                                        roleName = (companyElement.GetElementsByTagName("TRoleName") != null) ? Convert.ToString(companyElement.GetElementsByTagName("TRoleName")[0].InnerText) : ""
                                                    });
                                                }
                                            }
                                            lst.lstTesterList = oTester;

                                        }
                                    }

                                    #endregion
                                }


                                dataRow.Add(Name, Name == "lstTesterList" ? lst.lstTesterList : Value);
                            }


                            retObject.Add((ExpandoObject)dataRow);
                        }





                    }
                    return Json(retObject);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }

        [HttpGet, Route("GetDetailAnalysisData")]
        public JsonResult GetDetailAnalysisData(string ProjectId, string TestPassId = null, string TesterSPUserId = null, string RoleId = null)
        {
            try
            {
                string AppUrl = HttpContext.Request.Headers["appurl"];
                string SchemaName = "";
                if (!string.IsNullOrEmpty(AppUrl))
                {
                   SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    return Json("Invalid Url");

                }
                List<DetailAnalysis> listDetailAnalysis = new List<DetailAnalysis>();
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spGetDetailAnalysis";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProjectId", SqlDbType.VarChar, 500) { Value = ProjectId });
                    cmd.Parameters.Add(new SqlParameter("@TestPassId", SqlDbType.VarChar, 500) { Value = TestPassId });
                    cmd.Parameters.Add(new SqlParameter("@TesterSPUserId", SqlDbType.VarChar, 500) { Value = TesterSPUserId });
                    cmd.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.VarChar, 500) { Value = RoleId });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var retObject = new List<dynamic>();

                    using (var dataReader = cmd.ExecuteReader())
                    {

                        while (dataReader.Read())
                        {
                            var dataRow = new ExpandoObject() as IDictionary<string, object>;

                            for (var iFiled = 0; iFiled < dataReader.FieldCount; iFiled++)
                            {

                                var value = dataReader.GetName(iFiled);
                                var name = dataReader.IsDBNull(iFiled) ? null : dataReader[iFiled];

                                dataRow.Add(dataReader.GetName(iFiled), dataReader.IsDBNull(iFiled) ? null : dataReader[iFiled]);
                            }


                            retObject.Add((ExpandoObject)dataRow);
                        }





                    }


                    return Json(retObject);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }

        [HttpGet, Route("GetUsers")]
        public JsonResult GetUsers()
        {
            try
            {
                string AppUrl = HttpContext.Request.Headers["appurl"];
                string SchemaName = "";
                if (!string.IsNullOrEmpty(AppUrl))
                {
                   SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    return Json("Invalid Url");

                }
                List<DetailAnalysis> listDetailAnalysis = new List<DetailAnalysis>();
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.SP_GetUsers";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });


                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var retObject = new List<dynamic>();

                    using (var dataReader = cmd.ExecuteReader())
                    {

                        while (dataReader.Read())
                        {
                            var dataRow = new ExpandoObject() as IDictionary<string, object>;

                            for (var iFiled = 0; iFiled < dataReader.FieldCount; iFiled++)
                            {

                                var value = dataReader.GetName(iFiled);
                                var name = dataReader.IsDBNull(iFiled) ? null : dataReader[iFiled];

                                dataRow.Add(value, Convert.ToString(name));
                            }


                            retObject.Add((ExpandoObject)dataRow);
                        }





                    }


                    return Json(retObject);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }

        [HttpGet,Route("ExportTestersParticipation/{projectId}")]
        public JsonResult ExportTestersParticipation(string projectId)
        {
            try
            {
                string AppUrl = HttpContext.Request.Headers["appurl"];
                string SchemaName = "";
                if (!string.IsNullOrEmpty(AppUrl))
                {
                    SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    return Json("Invalid Url");

                }
                
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spExportTestersParticipation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProjectId", SqlDbType.VarChar, 500) { Value = projectId });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) {Direction=ParameterDirection.Output });


                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var retObject = new List<dynamic>();

                    using (var dataReader = cmd.ExecuteReader())
                    {

                        while (dataReader.Read())
                        {
                            var dataRow = new ExpandoObject() as IDictionary<string, object>;

                            for (var iFiled = 0; iFiled < dataReader.FieldCount; iFiled++)
                            {

                                var value = dataReader.GetName(iFiled);
                                var name = dataReader.IsDBNull(iFiled) ? null : dataReader[iFiled];

                                dataRow.Add(value, Convert.ToString(name));
                            }


                            retObject.Add((ExpandoObject)dataRow);
                        }





                    }


                    return Json(retObject);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }


    }

   
}
