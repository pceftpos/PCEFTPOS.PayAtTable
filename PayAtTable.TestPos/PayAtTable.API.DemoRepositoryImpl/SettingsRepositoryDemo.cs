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
    public class SettingsRepositoryDemo: ISettingsRepository
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Settings GetSettings(SettingsOptions options)
        {
            var tenderOptions = new List<TenderOption>();

            if (options.TenderTypes?.Count > 0)
            {
                tenderOptions.AddRange(options.TenderTypes);
            }

            // Create receipt options and add a default value
            var receiptOptions = new List<ReceiptOption>();
            receiptOptions.Add(new ReceiptOption()
            {
                Id = "0",
                ReceiptType = ReceiptType.Order,
                DisplayName = "Customer"
            });

            if (options.IsMultiplePrintOptions)
            {
                receiptOptions.Add(new ReceiptOption()
                {
                    Id = "1",
                    ReceiptType = ReceiptType.Order,
                    DisplayName = "Extended"
                });
            }

            //Create Header printer options
            var printerOption = new PrinterOption()
            {
                PrintMode = PrinterMode.POS,
                Location = options.CustomReceiptLocation,
                StaticReceipt = new List<string>()
                {
                    "------------------------",
                    "Some generic text",
                    "that will print before",
                    "every eftpos receipt",
                    "if PrintMode = STATIC",
                    "and location = header",
                }
            };

            return new Settings()
            {
                TenderOptions = tenderOptions,
                ReceiptOptions = receiptOptions,
                PrinterOption = printerOption
            };
        }
    }
}
