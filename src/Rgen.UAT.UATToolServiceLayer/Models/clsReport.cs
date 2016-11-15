using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Rgen.UAT.UATToolServiceLayer.Models
{
    [DataContract]
    public class Reports
    {
        /// <summary>
        /// TestPassId
        /// </summary>
        [DataMember]
        public string testPassId { get; set; }
        /// <summary>
        /// Test Pass Name
        /// </summary>
        [DataMember]
        public string testPassName { get; set; }
        /// <summary>
        /// Test Pass Description
        /// </summary>
        [DataMember]
        public string description { get; set; }
        /// <summary>
        /// Test Manager Name
        /// </summary>
        [DataMember]
        public string testMgrName { get; set; }
        /// <summary>
        /// Test Pass Due Date
        /// </summary>
        [DataMember]
        public string dueDate { get; set; }
        /// <summary>
        /// Passed Test Steps in Testpass
        /// </summary>        
        [DataMember]
        public string passedTestSteps { get; set; }
        /// <summary>
        /// Failed Test Steps in Testpass
        /// </summary>
        [DataMember]
        public string failedTestSteps { get; set; }
        /// <summary>
        /// Not Completed Test Steps in Testpass
        /// </summary>
        [DataMember]
        public string notCompletedTestSteps { get; set; }
        /// <summary>
        /// Test CaseName Id
        /// </summary>
        [DataMember]
        public string testCaseId { get; set; }

        /// <summary>
        /// Test CaseName
        /// </summary>
        [DataMember]
        public string testCaseName { get; set; }

        /// <summary>
        /// Test Step Name
        /// </summary>
        [DataMember]
        public string testStepName { get; set; }

        /// <summary>
        /// test step id
        /// </summary>
        [DataMember]
        public string testStepId { get; set; }

        /// <summary>
        /// Expected Result of test step
        /// </summary>
        [DataMember]
        public string expectedResult { get; set; }

        /// <summary>
        /// Tester ID
        /// </summary>
        [DataMember]
        public string testerId { get; set; }

        /// <summary>
        /// Tester Name
        /// </summary>
        [DataMember]
        public string testerName { get; set; }

        /// <summary>
        /// list of testcases of test pass
        /// </summary>
        [DataMember]
        public List<ReportTesterRoleStatus> listRptTesterRoleStatus { get; set; }
    }

    [DataContract]
    public class ReportTesterRoleStatus
    {
        [DataMember]
        public string teststepPlanId { get; set; }

        /// <summary>
        /// Tester Role Id
        /// </summary>
        [DataMember]
        public string roleId { get; set; }

        /// <summary>
        /// Tester Role Name
        /// </summary>
        [DataMember]
        public string roleName { get; set; }

        /// <summary>
        /// test Case status
        /// </summary>
        [DataMember]
        public string status { get; set; }

        /// <summary>
        /// Actual Result of test step
        /// </summary>
        [DataMember]
        public string actualResult { get; set; }
    }


    [DataContract]
    public class ReportUniqueObject
    {
        [DataMember]
        public int TPID { get; set; }
        [DataMember]
        public int TSID { get; set; }
        [DataMember]
        public int UID { get; set; }
    }
}
