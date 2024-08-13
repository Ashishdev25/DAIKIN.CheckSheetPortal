using System.Security.Principal;

namespace DAIKIN.CheckSheetPortal.Entities
{
    public class CheckSheetVersion: CheckSheet
    {
        public bool IsReviewed { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;
        public DateTime ActivateOn { get; set; }
        public DateTime RejectedOn { get; set; }
        public string RejectedBy { get; set; }
        public string RejectionComments { get; set; }
        public string WorkFlowStage { get; set; }
        public string WorkFlowUser { get; set; }
        public bool IsActivated { get; set; } = false;
        public string Status { get; set; } = "In Progress";
        public string CreatedByEmail { get; set; }
        public List<Reviewer> Reviewers { get; set; }
        public List<Approver> Approvers { get; set; }
    }
    public class Reviewer
    {
        public string ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public DateTime ReviewedOn { get; set; }
        public bool IsReviewed { get; set; }
        public int SeqOrder { get; set; }
    }
    public class Approver
    {
        public string ApproverId { get; set; }
        public string ApproverName { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public DateTime ApprovedOn { get; set; }
        public bool IsApproved { get; set; }
        public int SeqOrder { get; set; }
    }
}
