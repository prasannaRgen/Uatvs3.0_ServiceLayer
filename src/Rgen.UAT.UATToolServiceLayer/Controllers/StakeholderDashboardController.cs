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
    public class StakeholderDashboardController : Controller
    {
        private clsDbContext _context;
        public StakeholderDashboardController(clsDbContext context)
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

        // Get Project Details for Stakeholder Dashboard Page
        [HttpGet, Route("GetStakeholderProjectDetails")]
        public JsonResult GetStakeholderProjectDetails()
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
                    cmd.CommandText = "UAT.spStakeholderDashboardReportProjectDetails";
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

        // Get TestStep Details for Stakeholder Dashboard Page
        [HttpGet, Route("GetStakeholderTestStep")]
        public List<StakeholderDbTestStep> GetStakeholderTestStep()
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
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "UAT.spStakeholderDashboardTestStepDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SPUserId", SqlDbType.Int) { Value = _SpUserId });
                    cmd.Parameters.Add(new SqlParameter("@SchemaName", SqlDbType.VarChar, 500) { Value = SchemaName });
                    cmd.Parameters.Add(new SqlParameter("@Ret_Parameter", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    List<StakeholderDbTestStep> stakeholderDbTestStepResult = new List<StakeholderDbTestStep>();

                    var retObject = new List<dynamic>();
                    List<StkDBUniqueObject> lstStkDbUnique = new List<StkDBUniqueObject>();
                    StakeholderDbTestStep objTS = null;
                    List<StakeholderDbTSDetails> lstDetails = null;
                    List<clsTestSD> lst = new List<clsTestSD>();
                    int count = 0;

                    using (var dr = cmd.ExecuteReader())
                    {
                        int projectId = dr.GetOrdinal("projectId");
                        int testPassId = dr.GetOrdinal("testPassId");
                        int testPassName = dr.GetOrdinal("testPassName");
                        int created = dr.GetOrdinal("created");
                        int modified = dr.GetOrdinal("modified");
                        int testStepName = dr.GetOrdinal("testStepName");
                        int testStepId = dr.GetOrdinal("testStepId");
                        int projectName = dr.GetOrdinal("projectName");
                        int roleName = dr.GetOrdinal("roleName");
                        int testerName = dr.GetOrdinal("testerName");
                        int testCaseName = dr.GetOrdinal("testCaseName");
                        int testCaseId = dr.GetOrdinal("testCaseId");
                        int roleId = dr.GetOrdinal("roleId");
                        int testerId = dr.GetOrdinal("testerId");
                        int expectedResult = dr.GetOrdinal("expectedResult");
                        int actualResult = dr.GetOrdinal("actualResult");
                        int status = dr.GetOrdinal("status");

                        while (dr.Read())
                        {
                            lst.Add(new clsTestSD()
                            {
                                projectId = Convert.ToString(dr["projectId"]),
                                testPassId = Convert.ToString(dr["testPassId"]),
                                testPassName = Convert.ToString(dr["testPassName"]),
                                created = Convert.ToString(dr["created"]),
                                modified = Convert.ToString(dr["modified"]),
                                testStepName = Convert.ToString(dr["testStepName"]),
                                testStepId = Convert.ToString(dr["testStepId"]),
                                projectName = Convert.ToString(dr["projectName"]),
                                roleName = Convert.ToString(dr["roleName"]),
                                testerName = Convert.ToString(dr["testerName"]),
                                testCaseName = Convert.ToString(dr["testCaseName"]),
                                testCaseId = Convert.ToString(dr["testCaseId"]),
                                roleId = Convert.ToString(dr["roleId"]),
                                testerId = Convert.ToString(dr["testerId"]),
                                expectedResult = Convert.ToString(dr["expectedResult"]),
                                actualResult = Convert.ToString(dr["actualResult"]),
                                status = Convert.ToString(dr["status"]),
                            });

                            count++;
                            int _projectId = (dr.IsDBNull(projectId)) == true ? 0 : Convert.ToInt32(Convert.ToString(dr["projectId"]));
                            int _testPassId = (dr.IsDBNull(testPassId)) == true ? 0 : Convert.ToInt32(Convert.ToString(dr["testPassId"]));
                            int _testCaseId = (dr.IsDBNull(testCaseId)) == true ? 0 : Convert.ToInt32(Convert.ToString(dr["testCaseId"]));
                            int _testerId = (dr.IsDBNull(testerId)) == true ? 0 : Convert.ToInt32(Convert.ToString(dr["testerId"]));
                            int _roleId = (dr.IsDBNull(roleId)) == true ? 0 : Convert.ToInt32(Convert.ToString(dr["roleId"]));

                            if (lstStkDbUnique.Any(z => z.ProjectID == _projectId && z.TPID == _testPassId && z.TCID == _testCaseId && z.TesterID == _testerId && z.RoleID == _roleId) == false)
                            {
                                lstStkDbUnique.Add(new StkDBUniqueObject()
                                {
                                    ProjectID = _projectId,
                                    TPID = _testPassId,
                                    TCID = _testCaseId,
                                    TesterID = _testerId,
                                    RoleID = _roleId
                                });
                                objTS = new StakeholderDbTestStep();
                                objTS.projectId = (dr.IsDBNull(projectId)) == true ? "" : Convert.ToString(dr["projectId"]).Trim();
                                objTS.projectName = (dr.IsDBNull(projectName)) == true ? "" : Convert.ToString(dr["projectName"]).Trim();
                                objTS.testpassId = (dr.IsDBNull(testPassId)) == true ? "" : Convert.ToString(dr["testPassId"]).Trim(); ;
                                objTS.testpassName = (dr.IsDBNull(testPassName)) == true ? "" : Convert.ToString(dr["testPassName"]).Trim();
                                objTS.testcaseId = (dr.IsDBNull(testCaseId)) == true ? "" : Convert.ToString(dr["testCaseId"]).Trim(); ;
                                objTS.testcaseName = (dr.IsDBNull(testCaseName)) == true ? "" : Convert.ToString(dr["testCaseName"]).Trim();
                                objTS.testerId = (dr.IsDBNull(testerId)) == true ? "" : Convert.ToString(dr["testerId"]).Trim(); ;
                                objTS.testerName = (dr.IsDBNull(testerName)) == true ? "" : Convert.ToString(dr["testerName"]).Trim();
                                objTS.roleId = (dr.IsDBNull(roleId)) == true ? "" : Convert.ToString(dr["roleId"]).Trim(); ;
                                objTS.roleName = (dr.IsDBNull(roleName)) == true ? "" : Convert.ToString(dr["roleName"]).Trim();
                                objTS.lstStakeholderDbTSDetails = null;
                                lstDetails = new List<StakeholderDbTSDetails>();

                                lstDetails.Add(new StakeholderDbTSDetails()
                                {
                                    teststepId = (dr.IsDBNull(testStepId)) == true ? "" : Convert.ToString(dr["testStepId"]),
                                    teststepName = (dr.IsDBNull(testStepName)) == true ? "" : Convert.ToString(dr["testStepName"]),
                                    Modified = (dr.IsDBNull(modified)) == true ? "" : Convert.ToString(dr["modified"]),
                                    Created = (dr.IsDBNull(created)) == true ? "" : Convert.ToString(dr["created"]),
                                    expectedResult = (dr.IsDBNull(expectedResult)) == true ? "" : Convert.ToString(dr["expectedResult"]),
                                    actualResult = (dr.IsDBNull(actualResult)) == true ? "" : Convert.ToString(dr["actualResult"]),
                                    status = (dr.IsDBNull(status)) == true ? "" : Convert.ToString(dr["status"])
                                });

                                if (lst.Count == count)
                                {
                                    objTS.lstStakeholderDbTSDetails = lstDetails;
                                    stakeholderDbTestStepResult.Add(objTS);
                                }
                                if ((Convert.ToString(dr["projectId"]) != _projectId.ToString() || Convert.ToString(dr["testPassId"]) != _testPassId.ToString() || Convert.ToString(dr["testCaseId"]) != _testCaseId.ToString() || Convert.ToString(dr["testerId"]) != _testerId.ToString() || Convert.ToString(dr["roleId"]) != _roleId.ToString()))
                                {
                                    objTS.lstStakeholderDbTSDetails = lstDetails;
                                    stakeholderDbTestStepResult.Add(objTS);
                                }
                            }
                            else
                            {
                                lstDetails.Add(new StakeholderDbTSDetails()
                                {
                                    teststepId = (dr.IsDBNull(testStepId)) == true ? "" : Convert.ToString(dr["testStepId"]),
                                    teststepName = (dr.IsDBNull(testStepName)) == true ? "" : Convert.ToString(dr["testStepName"]),
                                    Modified = (dr.IsDBNull(modified)) == true ? "" : Convert.ToString(dr["modified"]),
                                    Created = (dr.IsDBNull(created)) == true ? "" : Convert.ToString(dr["created"]),
                                    expectedResult = (dr.IsDBNull(expectedResult)) == true ? "" : Convert.ToString(dr["expectedResult"]),
                                    actualResult = (dr.IsDBNull(actualResult)) == true ? "" : Convert.ToString(dr["actualResult"]),
                                    status = (dr.IsDBNull(status)) == true ? "" : Convert.ToString(dr["status"])
                                });

                                if (lst.Count == count)
                                {
                                    objTS.lstStakeholderDbTSDetails = lstDetails;
                                    stakeholderDbTestStepResult.Add(objTS);
                                }
                                else if ((Convert.ToString(dr["projectId"]) != _projectId.ToString() || Convert.ToString(dr["testPassId"]) != _testPassId.ToString() || Convert.ToString(dr["testCaseId"]) != _testCaseId.ToString() || Convert.ToString(dr["testerId"]) != _testerId.ToString() || Convert.ToString(dr["roleId"]) != _roleId.ToString()))
                                {
                                    objTS.lstStakeholderDbTSDetails = lstDetails;
                                    stakeholderDbTestStepResult.Add(objTS);
                                }

                            }
                        }
                    }
                    return stakeholderDbTestStepResult;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}




