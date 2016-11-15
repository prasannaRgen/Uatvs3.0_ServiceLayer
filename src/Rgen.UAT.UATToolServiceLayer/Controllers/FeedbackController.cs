using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rgen.UAT.UATToolServiceLayer.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.IO;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Rgen.UAT.UATToolServiceLayer.Controllers
{
    [Route("api/[controller]")]
    public class FeedbackController : Controller
    {
        private string _errorText = "ErrorDetails";
        private string _statusText = "Success";
        private string _schemaNameParameterName = "@SchemaName";
        private string _returnParameter = "@Ret_Parameter";
        private string _statementTypeParameterName = "@StatementType";

        private clsDbContext _context;
        public FeedbackController(clsDbContext context)
        {
            _context = context;
        }

        // GET: api/values
        [HttpGet]
        public string Get()
        {
            return "value";
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        //Get TestPass/TestCase level feedback, rating value along with Feedback_Type configured feedback type in TestPass Table
        // Also get Feedback of Tester from TestStepPlan Table for a TestStep with a particular role of Tester
        [HttpGet, Route("GetFeedback/{projectId}/{type}")]
        public List<clsFeedBack> GetFeedback(int projectId, string type)
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
                    return null;
                }
                string status = string.Empty;
                string statementType = string.Empty;

                List<clsFeedBack> listFB = new List<clsFeedBack>();

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spGetFeedBack";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProjectId", SqlDbType.Int) { Value = projectId });
                    cmd.Parameters.Add(new SqlParameter("@LoginSpUserId", SqlDbType.Int) { Value = _SpUserId });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.NVarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var retObject = new List<dynamic>();
                    using (var dr = cmd.ExecuteReader())
                    {
                        int testpassId = dr.GetOrdinal("TestPass_Id");
                        int testpassName = dr.GetOrdinal("TestPass_Name");
                        int testcaseId = dr.GetOrdinal("TestCase_Id");
                        int testcaseName = dr.GetOrdinal("TestCase_Name");
                        int userId = dr.GetOrdinal("spUser_Id");
                        int testerName = dr.GetOrdinal("Tester_Name");
                        int Role_ID = dr.GetOrdinal("Role_ID");
                        int roleName = dr.GetOrdinal("Role_Name");
                        int Feedback_Type = dr.GetOrdinal("Feedback_Type");
                        int Feedback = dr.GetOrdinal("Feedback");
                        int Rating = dr.GetOrdinal("Rating");
                        int RatingFeedId = dr.GetOrdinal("RatingFeedId");
                        int TestStep_ID = dr.GetOrdinal("TestStep_ID");
                        int TestStep_ActionName = dr.GetOrdinal("TestStep_ActionName");
                        int Expected_Result = dr.GetOrdinal("Expected_Result");
                        int TestStepPlan_Id = dr.GetOrdinal("TestStepPlan_Id");
                        int Actual_Result = dr.GetOrdinal("Actual_Result");
                        int TesterStatusText = dr.GetOrdinal("TesterStatusText");
                        int TspFeedback = dr.GetOrdinal("TspFeedback");

                        int count = 0;
                        while (dr.Read())
                        {
                            List<clsTestFeedBack> lst = new List<clsTestFeedBack>();
                            lst.Add(new clsTestFeedBack()
                            {
                                testpassId = Convert.ToString(dr[testpassId]),
                                testcaseId = Convert.ToString(dr[testcaseId]),
                                Role_ID = Convert.ToString(dr[Role_ID]),
                                userId = Convert.ToString(dr[userId]),
                                testpassName = Convert.ToString(dr[testpassName]),
                                testcaseName = Convert.ToString(dr[testcaseName]),
                                testerName = Convert.ToString(dr[testerName]),
                                Role_Name = Convert.ToString(dr[roleName]),
                                Feedback_Type = Convert.ToString(dr[Feedback_Type]),
                                Feedback = Convert.ToString(dr[Feedback]),
                                Rating = Convert.ToString(dr[Rating]),
                                RatingFeedId = Convert.ToString(dr[RatingFeedId]),
                                TestStep_ID = Convert.ToString(dr[TestStep_ID]),
                                TestStep_ActionName = Convert.ToString(dr[TestStep_ActionName]),
                                Expected_Result = Convert.ToString(dr[Expected_Result]),
                                TestStepPlan_Id = Convert.ToString(dr[TestStepPlan_Id]),
                                Actual_Result = Convert.ToString(dr[Actual_Result]),
                                TesterStatusText = Convert.ToString(dr[TesterStatusText]),
                                TspFeedback = Convert.ToString(dr[TspFeedback]),
                            });
                            lst = lst.OrderBy(z => z.testpassId).OrderBy(z => z.testcaseId).OrderBy(z => z.Role_ID).OrderBy(z => z.userId).ToList();

                            clsFeedBack fbObject = null;
                            List<FeedbackUnique> lstUnique = new List<FeedbackUnique>();
                            List<FeedbackTesterDetail> listTesterDetail = null;

                            count++;
                            int tpid = Convert.ToInt32(dr["TestPass_ID"]);
                            int tcid = Convert.ToInt32(dr["TestCase_ID"]);
                            int rid = Convert.ToInt32(dr["Role_ID"]);
                            int uid = Convert.ToInt32(dr["spUser_Id"]);

                            if (lstUnique.Any(z => z.TPassId == tpid && z.TCaseId == tcid && z.RoleId == rid && z.spUserId == uid) == false)
                            {
                                lstUnique.Add(new FeedbackUnique()
                                {
                                    TPassId = tpid,
                                    TCaseId = tcid,
                                    RoleId = rid,
                                    spUserId = uid
                                });

                                fbObject = new clsFeedBack();

                                fbObject.testpassId = (dr.IsDBNull(testpassId)) == true ? "" : Convert.ToString(dr["TestPass_Id"]);
                                fbObject.testpassName = (dr.IsDBNull(testpassName)) == true ? "" : Convert.ToString(dr["TestPass_Name"]);
                                fbObject.testcaseId = (dr.IsDBNull(testcaseId)) == true ? "" : Convert.ToString(dr["TestCase_Id"]);
                                fbObject.testcaseName = (dr.IsDBNull(testcaseName)) == true ? "" : Convert.ToString(dr["TestCase_Name"]);
                                fbObject.userId = (dr.IsDBNull(userId)) == true ? "" : Convert.ToString(dr["spUser_Id"]);
                                fbObject.testerName = (dr.IsDBNull(testerName)) == true ? "" : Convert.ToString(dr["Tester_Name"]);

                                listTesterDetail = new List<FeedbackTesterDetail>();

                                if (type == "0")/*testpass*/
                                {
                                    listTesterDetail.Add(new FeedbackTesterDetail()
                                    {
                                        roleId = (dr.IsDBNull(Role_ID)) == true ? "" : Convert.ToString(dr["Role_ID"]),
                                        roleName = (dr.IsDBNull(roleName)) == true ? "" : Convert.ToString(dr["Role_Name"]),
                                        fBType = (dr.IsDBNull(Feedback_Type)) == true ? "" : Convert.ToString(dr["Feedback_Type"]),
                                        tpTcFeedback = (dr.IsDBNull(Feedback)) == true ? "" : Convert.ToString(dr["Feedback"]),
                                        tpTcRating = (dr.IsDBNull(Rating)) == true ? "" : Convert.ToString(dr["Rating"]),
                                        feedbackRatingId = (dr.IsDBNull(RatingFeedId)) == true ? "" : Convert.ToString(dr["RatingFeedId"])
                                    });
                                }
                                else if (type == "1")/*teststep*/
                                {
                                    listTesterDetail.Add(new FeedbackTesterDetail()
                                    {
                                        testStepId = (dr.IsDBNull(TestStep_ID)) == true ? "" : Convert.ToString(dr["TestStep_ID"]),
                                        testStepName = (dr.IsDBNull(TestStep_ActionName)) == true ? "" : Convert.ToString(dr["TestStep_ActionName"]),
                                        expectedResult = (dr.IsDBNull(Expected_Result)) == true ? "" : Convert.ToString(dr["Expected_Result"]),
                                        testplanId = (dr.IsDBNull(TestStepPlan_Id)) == true ? "" : Convert.ToString(dr["TestStepPlan_Id"]),
                                        actualResult = (dr.IsDBNull(Actual_Result)) == true ? "" : Convert.ToString(dr["Actual_Result"]),
                                        roleId = (dr.IsDBNull(Role_ID)) == true ? "" : Convert.ToString(dr["Role_ID"]),
                                        roleName = (dr.IsDBNull(roleName)) == true ? "" : Convert.ToString(dr["Role_Name"]),
                                        status = (dr.IsDBNull(TesterStatusText)) == true ? "" : Convert.ToString(dr["TesterStatusText"]),
                                        tsFeedback = (dr.IsDBNull(TspFeedback)) == true ? "" : Convert.ToString(dr["TspFeedback"])
                                    });
                                }
                                if (lst.Count == count)
                                {
                                    fbObject.listTesterDetail = listTesterDetail;
                                    listFB.Add(fbObject);
                                }
                                else if ((Convert.ToInt32(dr["TestPass_ID"]) != tpid || Convert.ToInt32(dr["TestCase_ID"]) != tcid || Convert.ToInt32(dr["Role_ID"]) != rid || Convert.ToInt32(dr["spUser_Id"]) != uid))
                                {
                                    fbObject.listTesterDetail = listTesterDetail;
                                    listFB.Add(fbObject);
                                }
                            }
                            else
                            {
                                if (type == "0")
                                {
                                    listTesterDetail.Add(new FeedbackTesterDetail()
                                    {
                                        roleId = (dr.IsDBNull(Role_ID)) == true ? "" : Convert.ToString(dr["Role_ID"]),
                                        roleName = (dr.IsDBNull(roleName)) == true ? "" : Convert.ToString(dr["Role_Name"]),
                                        fBType = (dr.IsDBNull(Feedback_Type)) == true ? "" : Convert.ToString(dr["Feedback_Type"]),
                                        tpTcFeedback = (dr.IsDBNull(Feedback)) == true ? "" : Convert.ToString(dr["Feedback"]),
                                        tpTcRating = (dr.IsDBNull(Rating)) == true ? "" : Convert.ToString(dr["Rating"]),
                                        feedbackRatingId = (dr.IsDBNull(RatingFeedId)) == true ? "" : Convert.ToString(dr["RatingFeedId"])
                                    });

                                }
                                else if (type == "1")
                                {
                                    listTesterDetail.Add(new FeedbackTesterDetail()
                                    {
                                        testStepId = (dr.IsDBNull(TestStep_ID)) == true ? "" : Convert.ToString(dr["TestStep_ID"]),
                                        testStepName = (dr.IsDBNull(TestStep_ActionName)) == true ? "" : Convert.ToString(dr["TestStep_ActionName"]),
                                        expectedResult = (dr.IsDBNull(Expected_Result)) == true ? "" : Convert.ToString(dr["Expected_Result"]),
                                        testplanId = (dr.IsDBNull(TestStepPlan_Id)) == true ? "" : Convert.ToString(dr["TestStepPlan_Id"]),
                                        actualResult = (dr.IsDBNull(Actual_Result)) == true ? "" : Convert.ToString(dr["Actual_Result"]),
                                        roleId = (dr.IsDBNull(Role_ID)) == true ? "" : Convert.ToString(dr["Role_ID"]),
                                        roleName = (dr.IsDBNull(roleName)) == true ? "" : Convert.ToString(dr["Role_Name"]),
                                        status = (dr.IsDBNull(TesterStatusText)) == true ? "" : Convert.ToString(dr["TesterStatusText"]),
                                        tsFeedback = (dr.IsDBNull(TspFeedback)) == true ? "" : Convert.ToString(dr["TspFeedback"])
                                    });
                                }
                                if (lst.Count == count)
                                {
                                    fbObject.listTesterDetail = listTesterDetail;
                                    listFB.Add(fbObject);
                                }
                                else if ((Convert.ToInt32(dr["TestPass_ID"]) != tpid || Convert.ToInt32(dr["TestCase_ID"]) != tcid || Convert.ToInt32(dr["Role_ID"]) != rid || Convert.ToInt32(dr["spUser_Id"]) != uid))
                                {
                                    fbObject.listTesterDetail = listTesterDetail;
                                    listFB.Add(fbObject);
                                }
                            }
                        }
                    }
                    return listFB;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //Insert Feedback in TestStepPlan Table from Feedback Page
        [HttpPost, Route("InsertUpdateFeedBack")]
        public Dictionary<string, string> InsertUpdateFeedBack([FromBody]FeedBackTestSteps oFeedback)
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();
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
                    _result.Add(this._errorText, "Invalid Appurl");
                    return _result;
                }
                if (string.IsNullOrEmpty(oFeedback.feedback))
                {
                    _result.Add(this._errorText, "Feedback is required");
                    return _result;
                }
                else if (string.IsNullOrEmpty(oFeedback.testStepPlanId))
                {
                    _result.Add(this._errorText, "TestStepPlanId is required");
                    return _result;
                }

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spFeedbackInsUpd";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Feedback", SqlDbType.VarChar, 8000) { Value = oFeedback.feedback });
                    cmd.Parameters.Add(new SqlParameter("@TestStepPlanId", SqlDbType.Int) { Value = oFeedback.testStepPlanId });
                    cmd.Parameters.Add(new SqlParameter("@UserCId", SqlDbType.Int) { Value = new clsUtility().GetLoggedInUserSPUserId() == "" ? null : new clsUtility().GetLoggedInUserSPUserId() });
                    SqlParameter outparam = new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.NVarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(outparam);
                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();
                    int retValPos = cmd.ExecuteNonQuery();
                    if (retValPos != 0)
                    {
                        string ReturnParamValue = Convert.ToString(cmd.Parameters["@Ret_Parameter"].Value);
                        if (!string.IsNullOrEmpty(ReturnParamValue) && ReturnParamValue.ToLower() == "success")
                        {
                            _result.Add(this._statusText, "Done");
                        }
                        else
                        {
                            List<System.Data.Common.DbParameter> _outParameter = new List<System.Data.Common.DbParameter>();
                            _outParameter.Add(cmd.Parameters["@Ret_Parameter"]);
                            _result.Add(this._errorText, _outParameter.ToString());
                        }
                    }
                    else
                    {
                        List<System.Data.Common.DbParameter> _outParameter = new List<System.Data.Common.DbParameter>();
                        foreach (System.Data.Common.DbParameter outP in _outParameter)
                            if (outP.Direction == ParameterDirection.Output)
                                _outParameter.Add(outP);
                        //  _result.Add(this._errorText, E);
                    }
                }
            }
            catch (Exception ex)
            {
                _result.Add(this._errorText, ex.Message);
            }
            return _result;
        }


        [HttpDelete, Route("DeleteFeedback")]
        public Dictionary<string, string> DeleteFeedback(string feedbackId)
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();
            try
            {
                List<TestCase> listProjectUsers = new List<TestCase>();
                if (string.IsNullOrEmpty(feedbackId))
                {
                    Dictionary<string, string> oError = new Dictionary<string, string>();
                    oError.Add("ERROR", "Feedback Id is required!");
                    return _result;
                }
                string AppUrl = HttpContext.Request.Headers["appurl"];
                string SchemaName = "";
                if (!string.IsNullOrEmpty(AppUrl))
                {
                    SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    _result.Add(this._errorText, "Invalid Appurl");
                    return _result;
                }

                string statementType = string.Empty;

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "[UAT].[spfeedbackRating]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@feedbackId", SqlDbType.Int) { Value = feedbackId });
                    cmd.Parameters.Add(new SqlParameter("@StatementType", SqlDbType.VarChar, 500) { Value = "Delete" });
                    cmd.Parameters.Add(new SqlParameter("@UserCId", SqlDbType.Int) { Value = new clsUtility().GetLoggedInUserSPUserId() == "" ? null : new clsUtility().GetLoggedInUserSPUserId() });
                    SqlParameter outparam = new SqlParameter("@Ret_Parameter", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.NVarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(outparam);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();
                    int retValPos = cmd.ExecuteNonQuery();
                    if (retValPos != 0)
                    {

                        string ReturnParamValue = Convert.ToString(cmd.Parameters["@Ret_Parameter"].Value);
                        if (!string.IsNullOrEmpty(ReturnParamValue) && ReturnParamValue.ToLower() == "success")
                        {
                            _result.Add(this._statusText, "Done");
                        }
                        else
                        {
                            List<System.Data.Common.DbParameter> _outParameter = new List<System.Data.Common.DbParameter>();
                            _outParameter.Add(cmd.Parameters["@Ret_Parameter"]);
                            _result.Add(this._errorText, _outParameter.ToString());
                        }
                    }
                    else
                    {
                        List<System.Data.Common.DbParameter> _outParameter = new List<System.Data.Common.DbParameter>();
                        foreach (System.Data.Common.DbParameter outP in _outParameter)
                            if (outP.Direction == ParameterDirection.Output)
                                _outParameter.Add(outP);

                        //  _result.Add(this._errorText, E);
                    }
                }
            }
            catch (Exception ex)
            {

                _result.Add(this._errorText, ex.Message);
            }
            return _result;
        }

        // Get Data for Detailed Analysis Page
        [HttpGet, Route("GetDropdownDataForDetailAnalysis_Portfolio")]
        public List<DropdownDataForDetailAnalysis> GetDropdownDataForDetailAnalysis_Portfolio()
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
                    return null;
                }
                List<DropdownDataForDetailAnalysis> detailedAnalysisResult = new List<DropdownDataForDetailAnalysis>();

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spDetailAnalysisDropDown_Portfolio";
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
                            List<clsTestterList> oTester = new List<clsTestterList>();

                            #region ' XMl Parsing '

                            #region Project Lead Conversion

                            string _versionLeadName = string.Empty;
                            if (dataReader["projectLead"] != null)
                            {
                                using (XmlReader reader = XmlReader.Create(new StringReader("<users>" + dataReader["projectLead"] + "</users>")))
                                {
                                    XmlDocument xml = new XmlDocument();
                                    xml.Load(reader);
                                    XmlNodeList TesterList = xml.GetElementsByTagName("user");
                                    foreach (XmlNode node in TesterList)
                                    {
                                        if (node["userName"] != null)
                                            _versionLeadName = node["userName"].InnerText;
                                        break;
                                    }
                                }
                            }
                            #endregion

                            #region Test Manager Conversion

                            string _testManagerName = string.Empty;
                            if (dataReader["testManagerDetails"] != null)
                            {
                                using (XmlReader reader = XmlReader.Create(new StringReader("<users>" + dataReader["testManagerDetails"] + "</users>")))
                                {
                                    XmlDocument xml = new XmlDocument();
                                    xml.Load(reader);
                                    XmlNodeList xmlNodeList = xml.GetElementsByTagName("user");
                                    foreach (XmlNode node in xmlNodeList)
                                    {
                                        if (node["userName"] != null)
                                            _testManagerName = node["userName"].InnerText;

                                        break;
                                    }
                                }
                            }
                            #endregion

                            #region Tester Conversion

                            string _testerSPID = string.Empty, _testerName = string.Empty;
                            if (dataReader["rowTesterDetails"] != null)
                            {
                                using (XmlReader reader = XmlReader.Create(new StringReader("<users>" + dataReader["rowTesterDetails"] + "</users>")))
                                {
                                    XmlDocument xml = new XmlDocument();
                                    xml.Load(reader);
                                    XmlNodeList xmlNodeList = xml.GetElementsByTagName("user");
                                    foreach (XmlNode node in xmlNodeList)
                                    {
                                        if (node["spUserId"] != null)
                                            _testerSPID = node["spUserId"].InnerText;

                                        if (node["userName"] != null)
                                            _testerName = node["userName"].InnerText;

                                        break;
                                    }
                                }
                            }
                            #endregion

                            #region Tester Conversion New
                            string _tSPID = string.Empty, _tName = string.Empty;
                            string _tRoleID = string.Empty, _tRoleName = string.Empty;
                            if (dataReader["TesterDetailsRowNew"] != null)
                            {
                                using (XmlReader reader = XmlReader.Create(new StringReader("<users>" + dataReader["TesterDetailsRowNew"] + "</users>")))
                                {
                                    XmlDocument xml = new XmlDocument();
                                    xml.Load(reader);
                                    XmlNodeList xmlNodeList = xml.GetElementsByTagName("user");
                                    foreach (XmlNode node in xmlNodeList)
                                    {
                                        XmlElement companyElement = (XmlElement)node;

                                        if (node["TUserId"] != null)
                                            _tSPID = node["TUserId"].InnerText;

                                        if (node["TUserName"] != null)
                                            _tName = node["TUserName"].InnerText;

                                        if (node["TRoleId"] != null)
                                            _tRoleID = node["TRoleId"].InnerText;

                                        if (node["TRoleName"] != null)
                                            _tRoleName = node["TRoleName"].InnerText;

                                        oTester.Add(new clsTestterList
                                        {
                                            testerId = (companyElement.GetElementsByTagName("TUserId") != null) ? Convert.ToString(companyElement.GetElementsByTagName("TUserId")[0].InnerText) : "",
                                            testerName = (companyElement.GetElementsByTagName("TUserName") != null) ? Convert.ToString(companyElement.GetElementsByTagName("TUserName")[0].InnerText) : "",
                                            roleId = (companyElement.GetElementsByTagName("TRoleId") != null) ? Convert.ToString(companyElement.GetElementsByTagName("TRoleId")[0].InnerText) : "",
                                            roleName = (companyElement.GetElementsByTagName("TRoleName") != null) ? Convert.ToString(companyElement.GetElementsByTagName("TRoleName")[0].InnerText) : ""
                                        });
                                    }
                                }
                            }
                            #endregion

                            #endregion

                            int projectId = dataReader.GetOrdinal("projectId");
                            int testPassId = dataReader.GetOrdinal("testPassId");
                            int testPassName = dataReader.GetOrdinal("testPassName");
                            int tpEndDate = dataReader.GetOrdinal("tpEndDate");
                            int roleId = dataReader.GetOrdinal("roleId");
                            int roleName = dataReader.GetOrdinal("roleName");
                            int projectName = dataReader.GetOrdinal("projectName");
                            int projectVersion = dataReader.GetOrdinal("projectVersion");

                            detailedAnalysisResult.Add(new DropdownDataForDetailAnalysis()
                            {
                                projectId = (dataReader.IsDBNull(projectId)) == true ? "" : Convert.ToString(dataReader["projectId"]),
                                projectName = (dataReader.IsDBNull(projectName)) == true ? "" : Convert.ToString(dataReader["projectName"]),
                                projectVersion = (dataReader.IsDBNull(projectVersion)) == true ? "" : Convert.ToString(dataReader["projectVersion"]),
                                versionLead = _versionLeadName,

                                testpassId = (dataReader.IsDBNull(testPassId)) == true ? "" : Convert.ToString(dataReader["testPassId"]),
                                testpassName = (dataReader.IsDBNull(testPassName)) == true ? "" : Convert.ToString(dataReader["testPassName"]),
                                testManager = _testManagerName,
                                tpEndDate = (dataReader.IsDBNull(tpEndDate)) == true ? "" : Convert.ToString(dataReader["tpEndDate"]),

                                testerId = _testerSPID,
                                testerName = _testerName,
                                lstTesterList = oTester,
                                roleId = (dataReader.IsDBNull(roleId)) == true ? "" : Convert.ToString(dataReader["roleId"]),
                                roleName = (dataReader.IsDBNull(roleName)) == true ? "" : Convert.ToString(dataReader["roleName"]),
                            });
                        }
                        return detailedAnalysisResult;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}