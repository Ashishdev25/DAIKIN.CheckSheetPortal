using AutoMapper;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;

namespace DAIKIN.CheckSheetPortal.Utils
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Configuration, ConfigurationDTO>();
            CreateMap<ReviewerDTO, Reviewer>();
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();
            CreateMap<ApproverDTO, Approver>();
            CreateMap<Reviewer, ReviewerDTO>();
            CreateMap<Approver, ApproverDTO>();
            CreateMap<CheckPointDTO, CheckPoint>();
            CreateMap<CheckPoint, CheckPointDTO>();
            CreateMap<MasterSettings, MasterSettingsDTO>();
            CreateMap<MasterSettingsDTO, MasterSettings>();
            CreateMap<Shift, ShiftDTO>();
            CreateMap<ShiftDTO, Shift>();

            CreateMap<CheckSheetVersionDTO, CheckSheetVersion>();
            CreateMap<CheckSheetVersion, CheckSheetVersionDTO>()
                .ForMember(dest => dest.Reviewers, opt => opt.MapFrom(src => src.Reviewers))
                .ForMember(dest => dest.Approvers, opt => opt.MapFrom(src => src.Approvers))
                .ForMember(dest => dest.CheckPoints, opt => opt.MapFrom(src => src.CheckPoints));
            CreateMap<CheckSheetVersion, CheckSheet>();
            CreateMap<CheckSheet, CheckSheetVersion>()
                .ForMember(dest => dest.CheckPoints, opt => opt.MapFrom(src => src.CheckPoints));
            CreateMap<CheckPoint, CheckPointTransaction>()
                .ForMember(dest => dest.FrequencyText, opt => opt.MapFrom(src => GetFrequencyText(src)))
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => $"{src.Method} (complete in {src.CompleteInSeconds}secs)"));
            CreateMap<CheckPointTransaction, CheckPointTransactionDTO>();
            CreateMap<CheckSheetTransaction, ViewCheckSheetTransactionDTO>();            
            CreateMap<CheckSheetTransaction, CheckSheetTransactionDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status== "Save" ? "In-Progress" : src.Status));
            CreateMap<CheckSheetTransaction, CheckSheetTransactionFullDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status == "Save" ? "In-Progress" : src.Status))
                .ForMember(dest => dest.CheckPointTransactions, opt => opt.MapFrom(src => src.CheckPointTransactions));
        }
        public static string GetFrequencyText(CheckPoint dto)
        {
            var frequency = dto.FrequencyType;

            if (dto.FrequencyType == "Weekly" && dto.WeekDays != null && dto.WeekDays.Any())
            {
                var weekdayNames = dto.WeekDays.Select(weekday => Common.Util.GetWeekdayName(weekday));
                string weekdaysText;
                if (weekdayNames.Count() > 1)
                {
                    weekdaysText = string.Join(", ", weekdayNames.Take(weekdayNames.Count() - 1));
                    weekdaysText += " and " + weekdayNames.Last();
                }
                else
                {
                    weekdaysText = weekdayNames.First();
                }
                frequency += "\n(on " + weekdaysText + ")";
            }
            else if (dto.FrequencyType == "Monthly" && dto.MonthDays != null && dto.MonthDays.Any())
            {
                var monthDayNames = dto.MonthDays.Select(day => Common.Util.GetOrdinalWithSuperscript(day));
                string monthDayText;
                if (monthDayNames.Count() > 1)
                {
                    monthDayText = string.Join(", ", monthDayNames.Take(monthDayNames.Count() - 1));
                    monthDayText += " and " + monthDayNames.Last();
                }
                else
                {
                    monthDayText = monthDayNames.First();
                }
                frequency += "\n(on " + monthDayText + ")";
            }
            else if (dto.FrequencyType == "Yearly" && dto.YearlyMonths != null && dto.YearlyMonths.Any() && dto.YearlyMonthDays != null && dto.YearlyMonthDays.Any())
            {
                var monthNames = dto.YearlyMonths.Select(month => Common.Util.GetMonthName(month));
                string monthNameText;
                if (monthNames.Count() > 1)
                {
                    monthNameText = string.Join(", ", monthNames.Take(monthNames.Count() - 1));
                    monthNameText += " and " + monthNames.Last();
                }
                else
                {
                    monthNameText = monthNames.First();
                }
                frequency += "\n(on " + monthNameText + ")";

                var monthDayNames = dto.YearlyMonthDays.Select(day => Common.Util.GetOrdinalWithSuperscript(day));
                string monthDayText;
                if (monthDayNames.Count() > 1)
                {
                    monthDayText = string.Join(", ", monthDayNames.Take(monthDayNames.Count() - 1));
                    monthDayText += " and " + monthDayNames.Last();
                }
                else
                {
                    monthDayText = monthDayNames.First();
                }
                frequency += "\n(on " + monthDayText + ")";
            }
            return frequency;
        }
    }
}