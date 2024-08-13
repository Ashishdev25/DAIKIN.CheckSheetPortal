using System.Text;

namespace DAIKIN.CheckSheetPortal.Common
{
    public static class Util
    {
        public const string SettngsId = "662a1be9e314ceb036814155";
        public const string UserNotFound = "User not found";
        public const string LoginIdAndPasswordRequired = "Login ID and Password are required";
        public const string UserNotActiveContactAdministrator = "The user is not active. Please contact the administrator";
        public const string IncorrectLoginIdOrPassword = "Incorrect Login ID or Password";
        public const string UserEmailAlreadyExists = "User email already exists";
        public const string RoleNameAlreadyExists = "Role name already exists";
        public const string ApiKeyIncorrect = "API key is incorrect";
        public const string CheckSheetsAlreadyGeneratedForToday = "The checksheets are already generated for today";
        public const string NoCheckSheetsFound = "No checksheets found";
        public const string NoRecordsFound = "No records found";
        public const string PleaseSelectDateRange = "Please select a date range";
        public const string FromDateAndToDateCannotBeEmpty = "From date and to date cannot be empty";
        public const string DateRangeWithin10Days = "The date range should be within 10 days";
        public const string NgOrAbnormalCanUseRecordNotAllowedForBulkUpdate = "NG or Abnormal Can Use record is not allowed for bulk update";
        public const string ApprovedCheckSheetsCannotBeUpdated = "Approved check sheets cannot be updated";
        public const string OnlyInProgressCheckSheetsCanBeSubmitted = "Only in-progress check sheets can be submitted";
        public const string OnlyInProgressCheckSheetsCanBeDeleted = "Only in-progress check sheets can be deleted";
        public const string PleaseCompleteAllCheckpointsBeforeSubmission = "Please complete all checkpoints before submission";
        public const string OnlySubmittedCheckSheetsCanBeApproved = "Only submitted check sheets can be approved";
        public const string CommentsCannotBeEmptyForNg = "Comments cannot be empty for NG";
        public const string CheckSheetNotFound = "Check sheet not found";
        public const string AtLeastOneReviewerShouldBeSelected = "At least one reviewer should be selected";
        public const string AtLeastOneApproverShouldBeSelected = "At least one approver should be selected";
        public const string ControlNoStationLineCombinationExists = "The combination of ControlNo+Station+Line entered already exists";
        public const string DepartmentIdNotFound = "Department ID not found";
        public const string EquipmentIdNotFound = "Equipment ID not found";
        public const string LineIdNotFound = "Line ID not found";
        public const string MaintenanceClassIdNotFound = "Maintenance Class ID not found";
        public const string StationIdNotFound = "Station ID not found";
        public const string LocationIdNotFound = "Location ID not found";
        public const string SubLocationIdNotFound = "SubLocation ID not found";
        public const string WeekdaysCannotBeEmptyForWeekly = "Weekdays cannot be empty for the frequency type Weekly";
        public const string MonthDaysCannotBeEmptyForMonthly = "MonthDays cannot be empty for the frequency type Monthly";
        public const string YearlyMonthsYearlyMonthDaysCannotBeEmptyForYearly = "YearlyMonths/YearlyMonthDays cannot be empty for the frequency type Yearly";
        public const string SrNoAlreadyExists = "The serial number already exists";
        public const string CheckpointNameAlreadyExists = "The checkpoint name already exists";
        public const string OnlyReviewedCheckSheetsCanBeApproved = "Only reviewed check sheets can be approved";
        public const string CheckSheetAlreadyApproved = "The check sheet is already approved";
        public const string AllReviewersShouldReviewBeforeApproval = "All reviewers should review before approval";
        public const string CheckSheetAlreadyApprovedByYou = "The check sheet is already approved by you";
        public const string PeerApproverNotFound = "Peer approver not found";
        public const string OnlySubmittedOrPartiallyReviewedCheckSheetsCanBeReviewed = "Only submitted or partially reviewed check sheets can be reviewed";
        public const string ReviewersCanBeChangedOnlyForInProgressAndRejectedCheckSheets = "Reviewers can be changed only for Inprogress & Rejected checksheets";
        public const string ApprovedCheckSheetsCannotBeReviewed = "Approved check sheets cannot be reviewed";
        public const string CheckSheetAlreadyReviewed = "The check sheet is already reviewed";
        public const string CheckSheetAlreadyReviewedByYou = "The check sheet is already reviewed by you";
        public const string PeerReviewerNotFound = "Peer reviewer not found";
        public const string CheckSheetAlreadySubmitted = "The check sheet is already submitted";
        public const string ShiftTimingsAreOver = "Shift timings are over, you cannot Submit the Checksheet";
        public const string PleaseAddDepartmentId = "Please add Department ID";
        public const string PleaseAddAtLeastOneCheckpoint = "Please add at least one checkpoint";
        public const string CodeAndDescriptionCannotBeEmpty = "Code and description cannot be empty";
        public const string CodeAndDescriptionAlreadyExist = "Code and description already exist";
        public const string SettingsNotFound = "Settings not found";
        public const string NoConfigurationFound = "No configuration found";
        public const string LoginIdAlreadyExists = "Login ID already exists";
        public const string NewPasswordAndConfirmPasswordRequired = "New password and confirm password are required";
        public const string PleaseEnterValidCheckSheetId = "Please enter a valid CheckSheetId";
        public const string NewPasswordAndConfirmPasswordShouldBeTheSame = "New password and confirm password should be the same";
        public const string InvalidUser = "Invalid User";
        public const string InvalidId = "Invalid Id";
        public const string InvalidCheckSheetId = "Invalid CheckSheetId";
        public const string PleaseEnterRejectionComments = "Please enter rejection comments";
        public const string PleaseEnterChangeDetails = "Please enter change details";
        public const string RecordUpdatedSuccessfully = "Record updated successfully";
        public const string BuildRecordsInsertedSuccessfully = "Build records inserted successfully";
        public const string ImageUploadedSuccessfully = "Image uploaded successfully";
        public const string UpdatedSuccessfully = "Updated successfully";
        public const string CheckSheetSubmittedSuccessfully = "Check sheet submitted successfully";
        public const string CheckSheetApprovedSuccessfully = "Check sheet approved successfully";
        public const string CheckRecordUpdatedSuccessfully = "Check record updated successfully";
        public const string CheckSheetCreatedSuccessfully = "Check sheet created successfully";
        public const string NewRevisionCreatedSuccessfully = "New revision created successfully";
        public const string WorkflowUpdatedSuccessfully = "Workflow updated successfully";
        public const string CheckSheetReplicatedSuccessfully = "Check sheet replicated successfully";
        public const string CheckPointCreatedSuccessfully = "Checkpoint created successfully";
        public const string CheckSheetReviewedSuccessfully = "Check sheet reviewed successfully";
        public const string CheckSheetRejectedSuccessfully = "Check sheet rejected successfully";
        public const string NoApprovedCheckSheets = "There are no approved check sheets";
        public const string ConfigurationAddedSuccessfully = "Configuration added successfully";
        public const string ConfigurationUpdatedSuccessfully = "Configuration updated successfully";
        public const string UserCreatedSuccessfully = "User created successfully";
        public const string UserUpdatedSuccessfully = "User updated successfully";
        public const string PasswordChangedSuccessfully = "Password changed successfully";
        public static string GetRevisionText(int version)
        {
            if (version < 1)
            {
                throw new ArgumentException("Value must be greater than or equal to 1.", nameof(version));
            }

            StringBuilder result = new StringBuilder();

            while (version > 0)
            {
                version--; // Adjust for 0-indexed count
                char remainder = (char)('A' + (version % 26));
                result.Insert(0, remainder);
                version /= 26;
            }

            return result.ToString();
        }
        public static string GetDateLongText(DateTime dateTime)
        {
            return dateTime.ToString("dd.MMM.yyyy HH:mm:ss");
        }
        public static DateTime GetISTLocalDate()
        {
            return DateTime.UtcNow.AddMinutes(330);
        }
        public static string GetMonthName(int monthNumber)
        {
            if (monthNumber < 1 || monthNumber > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(monthNumber), "Month number must be between 1 and 12.");
            }
            var date = new DateTime(DateTime.Now.Year, monthNumber, 1);
            return date.ToString("MMMM");
        }
        public static string GetOrdinalWithSuperscript(int day)
        {
            if (day >= 11 && day <= 13)
            {
                return day + "ᵗʰ";
            }
            switch (day % 10)
            {
                case 1: return day + "ˢᵗ";
                case 2: return day + "ⁿᵈ";
                case 3: return day + "ʳᵈ";
                default: return day + "ᵗʰ";
            }
        }
        public static string GetWeekdayName(int weekdayNumber)
        {
            if (weekdayNumber < 0 || weekdayNumber > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(weekdayNumber), "Weekday number must be between 0 and 6.");
            }
            var dayOfWeek = (DayOfWeek)weekdayNumber;
            return dayOfWeek.ToString();
        }
    }
}
