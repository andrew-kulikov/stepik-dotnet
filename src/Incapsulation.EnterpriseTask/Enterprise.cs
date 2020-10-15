using System;
using System.Linq;

namespace Incapsulation.EnterpriseTask
{
    public class Enterprise
    {
        private string inn;

        public Enterprise(Guid guid)
        {
            Guid = guid;
        }

        public Guid Guid { get; }

        public string Name { get; set; }

        public string Inn
        {
            get => inn;
            set
            {
                if (value.Length != 10 || int.TryParse(value, out var res))
                {
                    throw new ArgumentException();
                }

                inn = value;
            }
        }

        public DateTime EstablishDate { get; set; }


        public TimeSpan ActiveTimeSpan => DateTime.Now - EstablishDate;

        public double GetTotalTransactionsAmount()
        {
            DataBase.OpenConnection();

            return DataBase.Transactions().Where(z => z.EnterpriseGuid == Guid).Sum(t => t.Amount);
        }
    }
}