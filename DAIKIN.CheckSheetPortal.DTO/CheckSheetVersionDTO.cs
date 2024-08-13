using System.ComponentModel.DataAnnotations;

namespace DAIKIN.CheckSheetPortal.DTO
{
    public class ReviewersAndApproversDTO
    {
        [Required(ErrorMessage = "Reviewers must be a valid value.")]
        public IEnumerable<ReviewerDTO> Reviewers { get; set; }
        [Required(ErrorMessage = "Approvers must be a valid value.")]
        public IEnumerable<ApproverDTO> Approvers { get; set; }
    }
    public class CheckSheetCreateDTO : ReviewersAndApproversDTO
    {
        [Required(ErrorMessage = "UniqueId must be a valid value.")]
        public string UniqueId { get; set; }
        [Required(ErrorMessage = "LineId must be a valid value.")]
        public string LineId { get; set; }
        [Required(ErrorMessage = "StationId must be a valid value.")]
        public string StationId { get; set; }
    }
    public class CheckSheetCreateVersionDTO : ReviewersAndApproversDTO
    {
        [Required(ErrorMessage = "Id must be a valid value.")]
        public string Id { get; set; }
    }
    public class CheckSheetReplicateDTO : CheckSheetCreateVersionDTO
    {
        [Required(ErrorMessage = "UniqueId must be a valid value.")]
        public string UniqueId { get; set; }
        [Required(ErrorMessage = "LineId must be a valid value.")]
        public string LineId { get; set; }
        [Required(ErrorMessage = "StationId must be a valid value.")]
        public string StationId { get; set; }
    }
    public class ReviewerDTO
    {
        [Required(ErrorMessage = "ReviewerId must be a valid value.")]
        public string ReviewerId { get; set; }
        [Required(ErrorMessage = "ReviewerName must be a valid value.")]
        public string ReviewerName { get; set; }
        [Required(ErrorMessage = "Department must be a valid value.")]
        public string Department { get; set; }
        public string Email { get; set; }
        public DateTime ReviewedOn { get; set; }
        public bool IsReviewed { get; set; } = false;
        [Range(1, 60, ErrorMessage = "SeqOrder must be greater than 0.")]
        public int SeqOrder { get; set; }
    }
    public class ApproverDTO
    {
        [Required(ErrorMessage = "ApproverId must be a valid value.")]
        public string ApproverId { get; set; }
        [Required(ErrorMessage = "ApproverName must be a valid value.")]
        public string ApproverName { get; set; }
        [Required(ErrorMessage = "Department must be a valid value.")]
        public string Department { get; set; }
        public string Email { get; set; }
        public DateTime ApprovedOn { get; set; }
        public bool IsApproved { get; set; } = false;
        [Range(1, 60, ErrorMessage = "SeqOrder must be greater than 0.")]
        public int SeqOrder { get; set; }
    }
    public class CheckSheetVersionDTO : CheckSheetDTO
    {
        public bool IsReviewed { get; set; }
        public bool IsApproved { get; set; }
        public string Status { get; set; }
        public string Department { get; set; }
        public string Line { get; set; }
        public string MaintenaceClass { get; set; }
        public string Station { get; set; }
        public string Equipment { get; set; }
        public string EquipmentCode { get; set; }
        public string Location { get; set; }
        public string SubLocation { get; set; }
        public DateTime ActivateOn { get; set; }
        public bool IsActivated { get; set; } = false;
        public DateTime RejectedOn { get; set; }
        public string RejectionComments { get; set; }
        public string RejectedBy { get; set; }
        public string WorkFlowStage { get; set; }
        public string WorkFlowUser { get; set; }
        public string CreatedByEmail { get; set; }
        public List<ReviewerDTO> Reviewers { get; set; }
        public List<ApproverDTO> Approvers { get; set; }
        public List<CheckPointDTO> CheckPoints { get; set; }
        public ManageCheckSheetUserActionDTO UserActions { get; set; }
    }
    public class ManageCheckSheetUserActionDTO
    {
        public bool IsReadOnly { get; set; }
        public bool ShowReplicateCheckSheet { get; set; }
        public bool ShowCreateNewVersion { get; set; }
        public bool ShowAddCheckPoint { get; set; }
        public bool ShowSubmit { get; set; }
        public bool ShowReview { get; set; }
        public bool ShowApprove { get; set; }
        public bool ShowReject { get; set; }
        public bool ShowDelete { get; set; }
    }
    public class CheckSheetVersionUpdateDTO : CheckSheetDTO
    {        
        [RegularExpression("^(Save|Submit|Reviewed|Approved)$", ErrorMessage = "Action must be 'Save', 'Submit', 'Reviewed', or 'Approved'.")]
        public string UserAction { get; set; }
        public DateTime ActivateOn { get; set; }
    }
}
