namespace MAG.TOF.Domain.Services
{
    public class RequestValidationService
    {
        public int CalculateBusinessDays(DateTime startDate, DateTime endDate)
        {
            if( endDate < startDate)
            {
                throw new ArgumentException("End date must be greater than or equal to start date.");
            }

            int businessDays = 0;

            DateTime currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                    currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDays++;
                }

                currentDate = currentDate.AddDays(1);
            }

            return businessDays;
        }

        public bool isValidDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                return false;
            }

            if (startDate.Date < DateTime.Today)
            {
                return false;
            }

            return true;
        }

        public string FormatOverlapMessage(int requestId, DateTime startDate, DateTime endDate)
        {
            return $"This conflicts with Request #{requestId} ({startDate:MMM dd} - {endDate:MMM dd, yyyy})";
        }
    }
}
