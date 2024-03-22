using System;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Repositories;

namespace Code.Runtime.Interactors
{
    public class RankingInteractor : Interactor<RankingRepository>
    {
        public void AddRecord(int score)
        {
            _repository.RecordsData.Add(new RecordData()
            {
                RecordDate = DateTime.Now,
                Score = score,
            });
        }

        public RecordData[] GetRecords(SortByDate sortByDate, int length)
        {
            IEnumerable<RecordData> sortedData = _repository.RecordsData;

            switch (sortByDate)
            {
                case SortByDate.Today:
                    sortedData = GetRecordsForDay(DateTime.Now);
                    break;
                case SortByDate.Week:
                    sortedData = GetRecordsForWeek(DateTime.Now);
                    break;
            }
            
            return sortedData.OrderByDescending(r => r.Score).Take(length).ToArray();
        }

        private IEnumerable<RecordData> GetRecordsForDay(DateTime date)
        {
            return _repository.RecordsData
                .Where(r => r.RecordDate.Date == date.Date);
        }

        private IEnumerable<RecordData> GetRecordsForWeek(DateTime startDate)
        {
            DateTime endDate = startDate.AddDays(-7).Date;
            return _repository.RecordsData
                .Where(r => r.RecordDate.Date >= endDate && r.RecordDate.Date <= startDate.Date);
        }
        
        public enum SortByDate
        {
            Today,
            Week,
            AllTime
        }
    }
}