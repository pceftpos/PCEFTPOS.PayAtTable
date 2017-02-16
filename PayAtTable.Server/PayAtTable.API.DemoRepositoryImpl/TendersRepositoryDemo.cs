using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayAtTable.Server.Data;
using PayAtTable.Server.Models;
using PayAtTable.API.Helpers;


namespace PayAtTable.Server.DemoRepository
{
    public class TendersRepositoryDemo: ITendersRepository
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Models.Tender CreateTender(Models.Tender t)
        {
            // Validate the order id
            if (SampleData.Current.Orders.Find((order) => order.Id.Equals(t.OrderId)) == null)
                throw new InvalidRequestException(String.Format("Order id {0} not found", t.OrderId));

            // Give the tender and id and add it to our tender list
            t.Id = (SampleData.Current.LastTenderId++).ToString();
            SampleData.Current.Tenders.Add(t);
            return t;
        }

        public Models.Tender UpdateTender(Models.Tender tender)
        {
            // Find the tender
            var t = SampleData.Current.Tenders.Find((t2) => t2.Id.Equals(tender.Id));
            if (t == null)
                throw new ResourceNotFoundException(String.Format("Tender id {0} not found", tender.Id));

            // Find the order
            var o = SampleData.Current.Orders.Find((order) => order.Id.Equals(tender.OrderId));
            if (o == null)
                throw new InvalidRequestException(String.Format("Order id {0} not found", tender.OrderId));


            // If Tender is moving from an incomplete to complete state, reduce the amount
            if (t.TenderState == TenderState.Pending && tender.TenderState == TenderState.CompleteSuccessful)
            {
                o.AmountOwing -= tender.AmountPurchase;
                // If our order isn't owing anymore, set to complete
                if (o.AmountOwing == 0)
                {
                    o.OrderState = OrderState.Complete;
                    o.TableId = string.Empty;
                }
            }

            // Update our tender
            t.AmountPurchase = tender.AmountPurchase;
            t.OriginalAmountPurchase = tender.OriginalAmountPurchase;
            t.TenderState = tender.TenderState;

            return t;
        }


    }
}
