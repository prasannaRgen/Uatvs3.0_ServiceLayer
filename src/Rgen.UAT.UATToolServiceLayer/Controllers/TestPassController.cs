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
using System.Globalization;
using System.Data.Common;
using Microsoft.AspNetCore.Http;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Rgen.UAT.UATToolServiceLayer.Controllers
{
    [Route("api/[controller]")]


    public class TestPassController : Controller
    {
        private string _errorText = "ErrorDetails";
        private string _statusText = "Success";
        private string _schemaNameParameterName = "@SchemaName";
        private string _returnParameter = "@Ret_Parameter";
        private string _statementTypeParameterName = "@StatementType";

        private clsDbContext _context;
        public TestPassController(clsDbContext context)
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




        [HttpGet, Route("GetTestPassDetailForProjectId/{projectId}/{userId}")]
        public JsonResult GetTestPassDetailForProjectId(int projectId, string userId)
        //[HttpGet, Route("GetTestPassDetailForProjectId/{projectId}")]
        //public JsonResult GetTestPassDetailForProjectId(int projectId)
        {
            try
            {
                string _SpUserId = HttpContext.Request.Headers["LoggedInUserSPUserId"];
                // string userid = Convert.ToInt32("userId").ToString();
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


                //  string listTestMgr;

                List<TestPass> listTP = new List<TestPass>();
                List<ProjectUser> listMgr = new List<ProjectUser>();
                //string SchemaName = new clsUatClient(_context).GetClientSchema(Appurl);

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.SpTPSelect";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProjectId", SqlDbType.Int) { Value = projectId });
                    //   cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = _SpUserId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.NVarChar, 500) { Value = SchemaName });

                    var statementType = "S2";
                    SqlParameter sttype = new SqlParameter("@StatementType", SqlDbType.NVarChar, 10);
                    sttype.Value = statementType;
                    cmd.Parameters.Add(sttype);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var retObject = new List<dynamic>();



                    using (var dr = cmd.ExecuteReader())

                    {
                        if (dr.HasRows)
                        {
                            int userNameOrdinal = dr.GetOrdinal("User_Name");
                            int aliasOrdinal = dr.GetOrdinal("User_Alias");
                            int emailOrdinal = dr.GetOrdinal("User_EmailId");
                            //  int securityIdOrdinal = "";
                            int spUserIdOrdinal = dr.GetOrdinal("User_SpUserID");

                            // for listTP
                            int testPassIdOrdinal = dr.GetOrdinal("TestPass_Id");
                            int projectIdOrdinal = dr.GetOrdinal("Project_Id");
                            int testPassNameOrdinal = dr.GetOrdinal("TestPass_Name");
                            int testPassDespOrdinal = dr.GetOrdinal("TestPass_Description");
                            int tpEndDateOrdinal = dr.GetOrdinal("End_date");
                            int tpStartDateOrdinal = dr.GetOrdinal("Start_date");
                            int tpStatusOrdinal = dr.GetOrdinal("Status");
                            int totalTestCaseCountOrdinal = dr.GetOrdinal("tcCount");



                            while (dr.Read())
                            {
                                listMgr = new List<ProjectUser>();
                                listMgr.Add(new ProjectUser
                                {

                                    userName = (dr.IsDBNull(userNameOrdinal)) == true ? "" : Convert.ToString(dr[userNameOrdinal]),
                                    alias = (dr.IsDBNull(aliasOrdinal)) == true ? "" : Convert.ToString(dr[aliasOrdinal]),
                                    email = (dr.IsDBNull(emailOrdinal)) == true ? "" : Convert.ToString(dr[emailOrdinal]),
                                    securityId = "",
                                    spUserId = (dr.IsDBNull(spUserIdOrdinal)) == true ? "" : Convert.ToString(dr[spUserIdOrdinal]),

                                });
                                listTP.Add(new TestPass
                                {

                                    testPassId = (dr.IsDBNull(testPassIdOrdinal) == true ? "" : Convert.ToString(dr["TestPass_Id"])),
                                    projectId = (dr.IsDBNull(projectIdOrdinal) == true ? "" : Convert.ToString(dr["Project_Id"])),
                                    testPassName = (dr.IsDBNull(testPassNameOrdinal) == true ? "" : Convert.ToString(dr["TestPass_Name"])),
                                    testPassDesp = (dr.IsDBNull(testPassDespOrdinal) == true ? "" : Convert.ToString(dr["TestPass_Description"])),
                                    tpEndDate = Convert.ToDateTime(Convert.ToString(dr["End_date"])).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                                    tpStartDate = Convert.ToDateTime(Convert.ToString(dr["Start_date"])).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                                    tpStatus = (dr.IsDBNull(tpStatusOrdinal)) == true ? "" : ReplaceStatus(Convert.ToString(dr["Status"])),
                                    totalTestCaseCount = (dr.IsDBNull(totalTestCaseCountOrdinal)) == true ? "" : Convert.ToString(dr["tcCount"]),
                                    listTestMgr = listMgr
                                });

                                //retObject.Add(dr);
                            }
                        }

                    }
                    return Json(listTP);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }


        private string GetShortStatus(string Status)
        {
            string retVal = string.Empty;
            if (string.IsNullOrEmpty(Status))
            {
                retVal = string.Empty;
            }
            else if (Status.ToUpper() == "ACTIVE")
            {
                retVal = "A";
            }
            else if (Status.ToUpper() == "ON HOLD")
            {
                retVal = "H";
            }
            else if (Status.ToUpper() == "COMPLETE")
            {
                retVal = "C";
            }
            else
            {
                retVal = Status;
            }

            return retVal;
        }

        private string ReplaceStatus(string Status)
        {
            string retVal = string.Empty;
            if (string.IsNullOrEmpty(Status))
            {
                retVal = string.Empty;
            }
            else if (Status.ToUpper() == "A")
            {
                retVal = "Active";
            }
            else if (Status.ToUpper() == "H")
            {
                retVal = "On Hold";
            }
            else if (Status.ToUpper() == "C")
            {
                retVal = "Completed";
            }
            else
            {
                retVal = Status;
            }

            return retVal;
        }





        [HttpGet, Route("GetAllTesterRolePFNCountForTestPassId/{testpassid}")]
        public TestPassTesterRolePFNCount GetAllTesterRolePFNCountForTestPassId(string testpassid)
        {
            try
            {
                Dictionary<string, string> _result = new Dictionary<string, string>();
                string _SpUserId = HttpContext.Request.Headers["LoggedInUserSPUserId"];
                string AppUrl = HttpContext.Request.Headers["appurl"];
                string SchemaName = "";
                if (!string.IsNullOrEmpty(AppUrl))
                {
                    SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    return null;
                }
                string status = "";
                string statementtype = "";

                if (string.IsNullOrEmpty(testpassid))
                {
                    //send service level Exception as service response
                    _result.Add(this._errorText, "testpassid is required");
                    // return _result;
                }

                List<TesterRolePFNCount> listTesterRolePFNCount = new List<TesterRolePFNCount>();
                TestPassTesterRolePFNCount objMain = null;

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spTesterRolewise";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@TestPassId", SqlDbType.VarChar, 500) { Value = testpassid });
                    cmd.Parameters.Add(new SqlParameter("@StatementType", SqlDbType.VarChar, 500) { Value = "Select" });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });
                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();
                    var retObject = new List<dynamic>();
                    using (var dr = cmd.ExecuteReader())
                    {
                        int count = 0;
                        int tcCount = 0;
                        int testingTime = 0;
                        string testPassStatus = "";
                        int roleid = 0;
                        int userid = 0;
                        while (dr.Read())
                        {
                            //if (dr.HasRows)
                            //{
                            int Status = dr.GetOrdinal("Status");
                            int CurrentETT = dr.GetOrdinal("CurrentETT");
                            int User_Name = dr.GetOrdinal("User_Name");
                            int User_EmailId = dr.GetOrdinal("User_EmailId");
                            int Role_Name = dr.GetOrdinal("Role_Name");
                            int passSteps = dr.GetOrdinal("passSteps");
                            int failSteps = dr.GetOrdinal("failSteps");
                            int ncSteps = dr.GetOrdinal("ncSteps");
                            int Feedback = dr.GetOrdinal("Feedback");

                            //listttt
                            List<clsTestPassTesterRolePFNCountList> lst = new List<clsTestPassTesterRolePFNCountList>();
                            lst.Add(new clsTestPassTesterRolePFNCountList()
                            {

                                Status = Convert.ToString(dr[Status]),
                                CurrentETT = Convert.ToString(dr[CurrentETT]),
                                User_Name = Convert.ToString(dr[User_Name]),
                                User_EmailId = Convert.ToString(dr[User_EmailId]),
                                Role_Name = Convert.ToString(dr[Role_Name]),
                                passSteps = Convert.ToString(dr[passSteps]),
                                failSteps = Convert.ToString(dr[failSteps]),
                                ncSteps = Convert.ToString(dr[ncSteps]),
                                Feedback = Convert.ToString(dr[Feedback]),


                            });

                            count++;
                            testPassStatus = (dr.IsDBNull(Status)) == true ? "" : Convert.ToString(dr["Status"]);
                            roleid = Convert.ToInt32(dr["Role_ID"]);
                            userid = Convert.ToInt32(dr["User_Id"]);
                            tcCount++;
                            if (!(dr.IsDBNull(dr.GetOrdinal("CurrentETT"))))
                                testingTime = testingTime + Convert.ToInt32(dr["CurrentETT"]);
                            if (lst.Count == count)
                            // if (dr.FieldCount == count)
                            {
                                listTesterRolePFNCount.Add(new TesterRolePFNCount
                                {
                                    testerSpUserId = (dr["User_Id"] == null) ? "" : Convert.ToString(dr["User_Id"]),
                                    testerName = (dr.IsDBNull(User_Name)) == true ? "" : Convert.ToString(dr["User_Name"]),
                                    testerEmail = (dr.IsDBNull(User_EmailId)) == true ? "" : Convert.ToString(dr["User_EmailId"]),
                                    testerRoleId = (dr["Role_ID"] == null) ? "" : Convert.ToString(dr["Role_ID"]),
                                    testerRoleName = (dr.IsDBNull(Role_Name)) == true ? "" : Convert.ToString(dr["Role_Name"]),
                                    passCount = (dr.IsDBNull(passSteps)) == true ? "" : Convert.ToString(dr["passSteps"]),
                                    failCount = (dr.IsDBNull(failSteps)) == true ? "" : Convert.ToString(dr["failSteps"]),
                                    NCCount = (dr.IsDBNull(ncSteps)) == true ? "" : Convert.ToString(dr["ncSteps"]),
                                    TCCount = Convert.ToString(tcCount),
                                    TestingTime = Convert.ToString(testingTime),
                                    FeedbackAvailable = (dr.IsDBNull(Feedback)) == true ? "" : Convert.ToString(dr["Feedback"])
                                });
                            }

                            else if (Convert.ToInt32(dr["Role_ID"]) != roleid || Convert.ToInt32(dr["User_Id"]) != userid)

                            {
                                listTesterRolePFNCount.Add(new TesterRolePFNCount
                                {
                                    testerSpUserId = (dr["User_Id"] == null) ? "" : Convert.ToString(dr["User_Id"]),
                                    testerName = (dr.IsDBNull(User_Name)) == true ? "" : Convert.ToString(dr["User_Name"]),
                                    testerEmail = (dr.IsDBNull(User_EmailId)) == true ? "" : Convert.ToString(dr["User_EmailId"]),
                                    testerRoleId = (dr["Role_ID"] == null) ? "" : Convert.ToString(dr["Role_ID"]),
                                    testerRoleName = (dr.IsDBNull(Role_Name)) == true ? "" : Convert.ToString(dr["Role_Name"]),
                                    passCount = (dr.IsDBNull(passSteps)) == true ? "" : Convert.ToString(dr["passSteps"]),
                                    failCount = (dr.IsDBNull(failSteps)) == true ? "" : Convert.ToString(dr["failSteps"]),
                                    NCCount = (dr.IsDBNull(ncSteps)) == true ? "" : Convert.ToString(dr["ncSteps"]),
                                    TCCount = Convert.ToString(tcCount),
                                    TestingTime = Convert.ToString(testingTime),
                                    FeedbackAvailable = (dr.IsDBNull(Feedback)) == true ? "" : Convert.ToString(dr["Feedback"])
                                });
                                tcCount = 0;
                                testingTime = 0;
                            }


                        }
                        objMain = new TestPassTesterRolePFNCount();
                        objMain.testPassId = testpassid;
                        objMain.testPassStatus = testPassStatus;
                        objMain.listTesterRolePFNCount = listTesterRolePFNCount;
                        // }
                    }
                    return objMain;
                }
            }

            catch (Exception ex)
            {

                return null;
            }


            //   return objMain;


        }












        /// <summary>
        /// new InsertUpdateTestPass using  Dictionary
        /// </summary>
        /// <param name="testpassId"></param>
        /// <returns></returns>
        [HttpPost, Route("InsertUpdateTestPass")]
        public Dictionary<string, string> InsertUpdateTestPass([FromBody]TestPass testpass)
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();



            List<ProjectUser> listMgr = new List<ProjectUser>();

            if (string.IsNullOrEmpty(testpass.testPassName) || string.IsNullOrEmpty(testpass.tpStartDate) || string.IsNullOrEmpty(testpass.tpEndDate))
            {
                //send service level Exception as service response
                _result.Add(this._errorText, "Mandatory Fields data is required");
                return _result;
            }
            else if (testpass.listTestMgr.Count > 0)
            {
                if (string.IsNullOrEmpty(testpass.listTestMgr[0].alias) || string.IsNullOrEmpty(testpass.listTestMgr[0].email) || string.IsNullOrEmpty(testpass.listTestMgr[0].userName) || string.IsNullOrEmpty(testpass.listTestMgr[0].spUserId))
                {
                    //send service level Exception as service response
                    _result.Add(this._errorText, "TestManager complete details required");
                    return _result;
                }
            }
            else if (testpass.listTestMgr.Count == 0)
            {
                //send service level Exception as service response
                _result.Add(this._errorText, "TestManager detail required");
                return _result;
            }

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
                    _result.Add(this._errorText, "Invalid Appurl");
                    return _result;

                }


                string statementType = string.Empty;
                if (string.IsNullOrEmpty(testpass.testPassId))
                    statementType = "Insert";
                else
                    statementType = "Update";


                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {

                    cmd.CommandText = "UAT.SpTestPass";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@TestPassId", SqlDbType.Int) { Value = testpass.testPassId });
                    cmd.Parameters.Add(new SqlParameter("@ProjectId", SqlDbType.Int) { Value = testpass.projectId });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrId", SqlDbType.Int) { Value = testpass.listTestMgr[0].spUserId });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrName", SqlDbType.VarChar, 500) { Value = testpass.listTestMgr[0].userName });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrAlias", SqlDbType.VarChar, 500) { Value = testpass.listTestMgr[0].alias });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrEmailId", SqlDbType.VarChar, 500) { Value = testpass.listTestMgr[0].email });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrSecurityId", SqlDbType.Int) { Value = testpass.listTestMgr[0].securityId });
                    cmd.Parameters.Add(new SqlParameter("@TestPassName", SqlDbType.NVarChar, 500) { Value = testpass.testPassName });
                    cmd.Parameters.Add(new SqlParameter("@DisplayID", SqlDbType.NVarChar, 500) { Value = testpass.testPassDisplayId });
                    cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) { Value = testpass.testPassDesp });
                    cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.VarChar, 500) { Value = testpass.tpStartDate });
                    cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.VarChar, 500) { Value = testpass.tpEndDate });
                    cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar, 500) { Value = testpass.tpStatus });
                    if (!string.IsNullOrEmpty(testpass.tpStatus))
                        cmd.Parameters[cmd.Parameters.Count - 1].Value = GetShortStatus(testpass.tpStatus);
                    else
                        cmd.Parameters[cmd.Parameters.Count - 1].Value = DBNull.Value;
                    cmd.Parameters.Add(new SqlParameter("@StatementType", SqlDbType.VarChar, 500) { Value = statementType });
                    SqlParameter outparam = new SqlParameter("@Ret_Parameter", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.NVarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(outparam);
                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();
                    //  int i = cmd.ExecuteNonQuery();

                    int result = cmd.ExecuteNonQuery();
                    string _retValue = Convert.ToString(cmd.Parameters["@Ret_Parameter"].Value);
                    if (result == -1)
                    {
                        if (!string.IsNullOrEmpty(_retValue))
                        {
                            _result.Add("Value", _retValue);
                            _result.Add(this._statusText, "Done");
                        }
                        else
                            _result.Add(this._statusText, "Done");
                    }

                }
            }
            catch (Exception ex)
            {

                _result.Add(this._errorText, ex.Message);
            }
            return _result;

        }


        [HttpDelete, Route("DeleteTestPass")]
        public JsonResult DeleteTestPass([FromBody] ClsTestPassParm testpassId)
        {

            if (string.IsNullOrEmpty(testpassId.testPassId))
            {
                Dictionary<string, string> oError = new Dictionary<string, string>();
                oError.Add("ERROR", "testpassId is required");
                return Json(oError);
            }


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


                string statementType = string.Empty;

                string SuccessMsg = "";
                //string SchemaName = new clsUatClient(_context).GetClientSchema(Appurl);
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {

                    cmd.CommandText = "[UAT].[SpTestPass]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@TestPassId", SqlDbType.Int) { Value = testpassId.testPassId });
                    cmd.Parameters.Add(new SqlParameter("@StatementType", SqlDbType.VarChar, 500) { Value = "Delete" });
                    cmd.Parameters.Add(new SqlParameter("@UserCId", SqlDbType.Int) { Value = new clsUtility().GetLoggedInUserSPUserId() == "" ? null : new clsUtility().GetLoggedInUserSPUserId() });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.NVarChar, 500) { Value = SchemaName });
                    SqlParameter outRet_Parameter = new SqlParameter("@Ret_Parameter", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outRet_Parameter);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    int i = cmd.ExecuteNonQuery();

                    var outparam1 = outRet_Parameter.Value;
                    if (i == -1)
                    {

                        if (outparam1 != null && Convert.ToString(outparam1) == "SUCCESS")
                        {

                            SuccessMsg = "Done";
                        }
                    }
                    return Json(SuccessMsg);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }

        }








        #region 'UploadAttachment'

        public bool inFile(IFormFile file)
        {
            return (file != null && file.Length > 0) ? true : false;
        }
        [HttpPost, Route("UploadAttachmentForMail")]
        public string UploadAttachmentForMail()
        {

            byte[] fileRecord = null;
            string outval = "";
            string _msg = "";
            try
            {
                string SchemaName = "";


                string AppUrl = HttpContext.Request.Headers["appurl"];
                string SpUserId = HttpContext.Request.Headers["LoggedInUserSPUserId"];

                if (!string.IsNullOrEmpty(AppUrl))
                {
                    SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {
                    _msg = "Invalid Url";

                }
                var files = HttpContext.Request.Form.Files;
                var _data = HttpContext.Request.Form;
                if (inFile(Request.Form.Files[0]))
                {


                    string fileType = Request.Form.Files[0].ContentType;
                    Stream file_strm = Request.Form.Files[0].OpenReadStream();
                    string file_Name = Path.GetFileName(Request.Form.Files[0].FileName);
                    int fileSize = Convert.ToInt32(Request.Form.Files[0].Length);
                    fileRecord = new byte[fileSize];
                    file_strm.Read(fileRecord, 0, fileSize);
                }




                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {



                    cmd.CommandText = "[UAT].[spAddTpAttachment]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@FileName", SqlDbType.VarChar, 500) { Value = "Att.png" });
                    cmd.Parameters.Add(new SqlParameter("@AttachmentImg", SqlDbType.VarBinary, 500000000) { Value = fileRecord });

                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@StatementType", SqlDbType.VarChar, 500) { Value = "InsertData" });

                    cmd.Parameters.Add(new SqlParameter("@outval", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });
                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    int i = cmd.ExecuteNonQuery();
                    outval = Convert.ToString(cmd.Parameters["@outval"].Value);


                }

            }
            catch (Exception ex)
            {

                _msg = (ex.Message);
            }
            return outval;

        }


        [HttpGet, Route("GetAttachmentFile")]
        public byte[] GetAttachmentFile(string id, string appurl)
        {

            byte[] fileContent = new byte[0];
            try
            {
                string SchemaName = "";

                string AppUrl = appurl;

                string _SpUserId = HttpContext.Request.Headers["LoggedInUserSPUserId"];

                if (!string.IsNullOrEmpty(AppUrl))
                {
                    SchemaName = new clsUatClient(_context).GetClientSchema(AppUrl);
                }
                else
                {


                }

                var filename = "";


                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "[UAT].[spAddTpAttachment]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.NVarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@StatementType", SqlDbType.VarChar, 500) { Value = "GetAttach" });
                    cmd.Parameters.Add(new SqlParameter("@AttachmentId", SqlDbType.Int) { Value = id });

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            filename = dataReader["FileName"].ToString();

                            fileContent = (byte[])dataReader["AttachmentImg"];
                            Response.ContentType = "image/png";
                            Response.Headers.Add("content-disposition", "attachment;filename=" + dataReader["FileName"].ToString());
                            Response.Body.WriteAsync(fileContent, 0, fileContent.Length);
                        }

                        // return File(fileContent, contentType, filename);

                    }

                }
            }
            catch (Exception ex)
            {

                return null;
            }
            return fileContent;

        }

        #endregion


















        // POST api/TestPass
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        //POST: /TestPass
        [HttpPost, Route("InsertUpdateTestPass_1")]
        public JsonResult InsertUpdateTestPass_1([FromBody]TestPass testpass)
        {

            if (string.IsNullOrEmpty(testpass.testPassName) || string.IsNullOrEmpty(testpass.tpStartDate) || string.IsNullOrEmpty(testpass.tpEndDate))
            {

                Dictionary<string, string> oError = new Dictionary<string, string>();
                oError.Add("ERROR", "Mandatory fields are required");
                return Json(oError);
            }
            else if (testpass.listTestMgr.Count > 0)
            {
                if (string.IsNullOrEmpty(testpass.listTestMgr[0].alias) || string.IsNullOrEmpty(testpass.listTestMgr[0].email) || string.IsNullOrEmpty(testpass.listTestMgr[0].userName) || string.IsNullOrEmpty(testpass.listTestMgr[0].spUserId))
                {

                    Dictionary<string, string> oError = new Dictionary<string, string>();
                    oError.Add("ERROR", "TestManager complete details required");
                    return Json(oError);
                }
            }
            else if (testpass.listTestMgr.Count == 0)
            {

                Dictionary<string, string> oError = new Dictionary<string, string>();
                oError.Add("ERROR", "TestManager detail required");
                return Json(oError);
            }

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


                string statementType = string.Empty;
                if (string.IsNullOrEmpty(testpass.testPassId))
                    statementType = "Insert";
                else
                    statementType = "Update";



                string SuccessMsg = "";
                //  string SchemaName = new clsUatClient(_context).GetClientSchema(Appurl);
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {

                    cmd.CommandText = "UAT.SpTestPass";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@TestPassId", SqlDbType.Int) { Value = testpass.testPassId });
                    cmd.Parameters.Add(new SqlParameter("@ProjectId", SqlDbType.Int, 500) { Value = testpass.projectId });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrId", SqlDbType.Int, 500) { Value = testpass.listTestMgr[0].spUserId });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrName", SqlDbType.VarChar, 500) { Value = testpass.listTestMgr[0].userName });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrAlias", SqlDbType.VarChar, 500) { Value = testpass.listTestMgr[0].alias });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrEmailId", SqlDbType.VarChar, 500) { Value = testpass.listTestMgr[0].email });
                    cmd.Parameters.Add(new SqlParameter("@TestMgrSecurityId", SqlDbType.Int, 500) { Value = testpass.listTestMgr[0].securityId });
                    cmd.Parameters.Add(new SqlParameter("@TestPassName", SqlDbType.NVarChar, 500) { Value = testpass.testPassName });
                    cmd.Parameters.Add(new SqlParameter("@DisplayID", SqlDbType.NVarChar, 500) { Value = testpass.testPassDisplayId });
                    cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) { Value = testpass.testPassDesp });
                    cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date, 500) { Value = testpass.tpStartDate });
                    cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date, 500) { Value = testpass.tpEndDate });
                    cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar, 500) { Value = testpass.tpStatus });
                    if (!string.IsNullOrEmpty(testpass.tpStatus))
                        cmd.Parameters[cmd.Parameters.Count - 1].Value = GetShortStatus(testpass.tpStatus);
                    else
                        cmd.Parameters[cmd.Parameters.Count - 1].Value = DBNull.Value;
                    cmd.Parameters.Add(new SqlParameter("@StatementType", SqlDbType.VarChar, 500) { Value = statementType });
                    SqlParameter outparam = new SqlParameter("@Ret_Parameter", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.NVarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(outparam);
                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();
                    int i = cmd.ExecuteNonQuery();
                    var outparam1 = outparam.Value;
                    if (i == -1)
                    {
                        if (Convert.ToString(outparam1) != "" && outparam1 != null)
                        {
                            //SuccessMsg = "Insert Successfully..!!";
                            SuccessMsg = "Done";
                        }
                    }
                    return Json(SuccessMsg);
                }





            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }

        }






    }
}


