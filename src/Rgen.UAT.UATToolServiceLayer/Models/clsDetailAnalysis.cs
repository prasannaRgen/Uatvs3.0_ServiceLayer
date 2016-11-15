using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rgen.UAT.UATToolServiceLayer.Models
{
    public class clsDetailAnalysis
    {
    }
    public class DetailAnalysis
    {
       
        public string projectId { get; set; }

       
        public string projectName { get; set; }

       
        public string projectVersion { get; set; }

       
        public string versionLead { get; set; }

       
        public string testpassId { get; set; }

       
        public string testpassName { get; set; }

       
        public string testManager { get; set; }

       
        public string testPassDescription { get; set; }

       
        public string tpStartDate { get; set; }

       
        public string tpEndDate { get; set; }


       
        public string testerId { get; set; }

       
        public string testerName { get; set; }

       
        public string roleId { get; set; }

       
        public string roleName { get; set; }

       
        public string testCaseId { get; set; }

       
        public string testCaseName { get; set; }

        

       
        public string totalTestCaseCount { get; set; }

       
        public string totalTestStepCount { get; set; }

       
        public string totalTesterCount { get; set; }


       
        public string testCasePassCount { get; set; }

       
        public string testCaseFailcount { get; set; }

       
        public string testCaseNtCompletedCount { get; set; }

       
        public string testStepPassCount { get; set; }

       
        public string testStepFailcount { get; set; }

       
        public string testStepNtCompletedCount { get; set; }

       
        public List<TesterDataForDetailAnalysis> listTesterData { get; set; }
    }

    public class TesterDataForDetailAnalysis
    {
       
        public string testerId { get; set; }

       
        public string testerName { get; set; }

       
        public string testerArea { get; set; }

       
        public string testerPassCount { get; set; }

       
        public string testerFailcount { get; set; }

       
        public string testerNtCompletedCount { get; set; }

    }

    public class DropdownDataForDetailAnalysis
    {
       
        public string projectId { get; set; }
        public string projectName { get; set; }
        public string projectVersion { get; set; }
        public string versionLead { get; set; }
        
        public string testpassId { get; set; }
        public string testpassName { get; set; }
        public string testManager { get; set; }
        public string tpEndDate { get; set; }
        
        public string testerId { get; set; }
        public string testerName { get; set; }
        public string roleId { get; set; }
        public string roleName { get; set; }
        public List<clsTestterList> lstTesterList { get; set; }
    }
}
